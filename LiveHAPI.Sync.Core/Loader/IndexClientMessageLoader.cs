﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveHAPI.Core.Interfaces.Repository;
using LiveHAPI.Core.Model.Encounters;
using LiveHAPI.Core.Model.Exchange;
using LiveHAPI.Shared.Custom;
using LiveHAPI.Shared.Enum;
using LiveHAPI.Shared.Interfaces.Model;
using LiveHAPI.Sync.Core.Exchange;
using LiveHAPI.Sync.Core.Exchange.Clients;
using LiveHAPI.Sync.Core.Exchange.Encounters;
using LiveHAPI.Sync.Core.Exchange.Messages;
using LiveHAPI.Sync.Core.Interface.Extractors;
using LiveHAPI.Sync.Core.Interface.Loaders;
using Serilog;

namespace LiveHAPI.Sync.Core.Loader
{
    public class IndexClientMessageLoader : IIndexClientMessageLoader
    {
        private readonly IPracticeRepository _practiceRepository;
        private readonly IClientStageRepository _clientStageRepository;
        private readonly IClientPretestStageRepository _clientPretestStageRepository;
        private readonly IClientTestingStageExtractor _clientTestingStageExtractor;
        private readonly IClientFinalTestStageExtractor _clientFinalTestStageExtractor;
        private readonly IClientReferralStageExtractor _clientReferralStageExtractor;
        private readonly IClientTracingStageExtractor _clientTracingStageExtractor;
        private readonly IClientLinkageStageExtractor _clientLinkageStageExtractor;

        public IndexClientMessageLoader(IPracticeRepository practiceRepository,
            IClientStageRepository clientStageRepository, IClientPretestStageRepository clientPretestStageRepository,
            IClientTestingStageExtractor clientTestingStageExtractor,
            IClientFinalTestStageExtractor clientFinalTestStageExtractor,
            IClientReferralStageExtractor clientReferralStageExtractor,
            IClientTracingStageExtractor clientTracingStageExtractor,
            IClientLinkageStageExtractor clientLinkageStageExtractor)
        {
            _practiceRepository = practiceRepository;
            _clientStageRepository = clientStageRepository;
            _clientPretestStageRepository = clientPretestStageRepository;
            _clientTestingStageExtractor = clientTestingStageExtractor;
            _clientFinalTestStageExtractor = clientFinalTestStageExtractor;
            _clientReferralStageExtractor = clientReferralStageExtractor;
            _clientTracingStageExtractor = clientTracingStageExtractor;
            _clientLinkageStageExtractor = clientLinkageStageExtractor;
        }

        public async Task<IEnumerable<IndexClientMessage>> Load(Guid? htsClientId = null, params LoadAction[] actions)
        {
            var messages = new List<IndexClientMessage>();
            if (!actions.Any())
                actions = new[] {LoadAction.All};

            //  Set Facility
            var facility = _practiceRepository.GetDefault();
            if (null == facility)
                throw new Exception($"Default Faciltity Not found");

            //      MESSAGE_HEADER

            var facilityCode = facility.Code;
            var header = MESSAGE_HEADER.Create(facilityCode);

            //      NEWCLIENT

            var stagedIndexClients = _clientStageRepository.GetIndexClients();

            if (!htsClientId.IsNullOrEmpty())
                stagedIndexClients = stagedIndexClients.Where(x => x.ClientId == htsClientId);

            foreach (var stagedClient in stagedIndexClients)
            {

                #region PATIENT_IDENTIFICATION

                var pid = PATIENT_IDENTIFICATION.Create(stagedClient);

                #endregion

                ENCOUNTERS encounter = null;
                if (!actions.Contains(LoadAction.RegistrationOnly))
                {
                    var pretests = _clientPretestStageRepository.GetByClientId(stagedClient.ClientId).ToList();
                    var lastPretest = pretests.OrderByDescending(x => x.EncounterDate).FirstOrDefault();

                    //    PRETEST AND TESTING

                    foreach (var pretest in pretests)
                    {
                        var pretestEncounter =
                            await CreateNonPretestEncounters(header, pid, stagedClient, lastPretest, actions);
                        messages.Add(pretestEncounter);
                    }

                    var nonPretest = await CreateNonPretestEncounters(header, pid, stagedClient, lastPretest, actions);

                    messages.Add(nonPretest);
                    
                }
                else
                {
                    messages.Add(new IndexClientMessage(header,
                        new List<NEWCLIENT> {NEWCLIENT.Create(pid, encounter)}));
                }
            }

            return messages;
        }

        private async Task<IndexClientMessage> CreatePretestEncounters(MESSAGE_HEADER header,
            PATIENT_IDENTIFICATION pid, ClientStage stagedClient, ClientPretestStage pretest,
            params LoadAction[] actions)
        {

            ENCOUNTERS encounter = null;

            //  PLACER_DETAIL
            var placerDetail = PLACER_DETAIL.Create(pretest.UserId, pretest.Id);

            //  PRE_TEST
            PRE_TEST preTest = null;
            if (actions.Contains(LoadAction.All) || actions.Contains(LoadAction.Pretest))
                preTest = PRE_TEST.Create(pretest);

            //  HIV_TESTS
            HIV_TESTS hivTests = null;
            if (actions.Contains(LoadAction.All) || actions.Contains(LoadAction.Testing))
            {
                var allfinalTests = await _clientFinalTestStageExtractor.Extract(stagedClient.ClientId);
                var alltests = await _clientTestingStageExtractor.Extract();

                var finalTest = allfinalTests.Where(x => x.PretestEncounterId == pretest.Id).ToList()
                    .LastOrDefault();
                var tests = alltests.Where(x => x.PretestEncounterId == pretest.Id).ToList();

                if (null != finalTest && tests.Any())
                    hivTests = HIV_TESTS.Create(tests, finalTest);
            }

            // GET THE LAST ONE


            encounter = ENCOUNTERS.Create(placerDetail, preTest, hivTests, null, null, null);

            return new IndexClientMessage(header,
                new List<NEWCLIENT> {NEWCLIENT.Create(pid, encounter)});

        }

        private async Task<IndexClientMessage> CreateNonPretestEncounters(MESSAGE_HEADER header,PATIENT_IDENTIFICATION pid,ClientStage stagedClient,ClientPretestStage lastPretest, params LoadAction[] actions)
        {
            ENCOUNTERS encounter = null;
 
            //  PLACER_DETAIL
                    
            var lastplacerDetail = PLACER_DETAIL.Create(lastPretest.UserId, lastPretest.Id);
                    
            //  NewReferral
            NewReferral newReferral = null;
            if (actions.Contains(LoadAction.All) || actions.Contains(LoadAction.Referral))
            {
                var allReferrals = await _clientReferralStageExtractor.Extract(stagedClient.ClientId);
                var referrall = allReferrals.LastOrDefault();
                newReferral = NewReferral.Create(referrall);
            }

            //  NewTracing
            List<NewTracing> newTracings = new List<NewTracing>();
            if (actions.Contains(LoadAction.All) || actions.Contains(LoadAction.Tracing))
            {
                var allTracing = await _clientTracingStageExtractor.Extract(stagedClient.ClientId);
                newTracings = NewTracing.Create(allTracing.ToList());
            }

            // NewLinkage
            NewLinkage newLinkage = null;
            if (actions.Contains(LoadAction.All) || actions.Contains(LoadAction.Linkage))
            {
                var allLinkages = await _clientLinkageStageExtractor.Extract(stagedClient.ClientId);
                var linkage = allLinkages.LastOrDefault();
                newLinkage = NewLinkage.Create(linkage);
            }
                    
            encounter = ENCOUNTERS.Create(lastplacerDetail,null, null,newReferral,newTracings,newLinkage);

            return new IndexClientMessage(header,
                new List<NEWCLIENT> {NEWCLIENT.Create(pid, encounter)});
        }
        
    }
}