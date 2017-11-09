﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using LiveHAPI.Core.Dispatcher;
using LiveHAPI.Core.Events;
using LiveHAPI.Core.Interfaces.Handler;
using LiveHAPI.Core.Interfaces.Repository;
using LiveHAPI.Core.Interfaces.Services;
using LiveHAPI.Core.Model.Subscriber;
using LiveHAPI.Shared.ValueObject;
using LiveHAPI.Shared.ValueObject.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LiveHAPI.Controllers
{

    [Route("api/cohorts")]
    public class CohortsController : Controller
    {
        private readonly ISubscriberSystemRepository _subscriberSystemRepository;
        private readonly SubscriberSystem _subscriberSystem;
        private readonly IClientService _clientService;

        public CohortsController(ISubscriberSystemRepository subscriberSystemRepository, IClientService clientService)
        {
            _subscriberSystemRepository = subscriberSystemRepository;
            _clientService = clientService;
            _subscriberSystem = _subscriberSystemRepository.GetDefault();
        }

        [Route("lists")]
        [HttpGet]
        public IActionResult GetLists()
        {
            try
            {
                if (null == _subscriberSystem)
                    throw new Exception("Server Systems not configured");

                var cohorts = Mapper.Map<List<CohortInfo>>(_subscriberSystem.Cohorts.ToList());
                return Ok(cohorts);
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                return StatusCode(500, $"{e.Message}");
            }
        }

        [Route("id/{id}")]
        [HttpGet]
        public IActionResult GetCohort(string id)
        {
            Guid cohortId = Guid.Empty;

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            try
            {
                cohortId = new Guid(id);
            }
            catch
            {
            }

            try
            {
                var cohort = _subscriberSystem.Cohorts.FirstOrDefault(x => x.Id == cohortId);

                if (null == cohort)
                    return NotFound();

                var personMatches = _clientService.LoadByCohort(cohort).ToList();


                var personData =new List<RemoteClientInfo>();

                foreach (var personMatch in personMatches)
                {
                    var rc = new RemoteClientInfo();

                    rc.Client = personMatch.RemoteClient.Client;

                    foreach (var client in personMatch.Person.Clients)
                    {
                        var es=new List<EncounterInfo>();
                        foreach (var clientEncounter in client.Encounters)
                        {
                            var e = Mapper.Map<EncounterInfo>(clientEncounter);
                            e.Obses= Mapper.Map<List<ObsInfo>>(clientEncounter.Obses.ToList());
                            e.ObsTestResults = Mapper.Map<List<ObsTestResultInfo>>(clientEncounter.ObsTestResults.ToList());
                            e.ObsFinalTestResults = Mapper.Map<List<ObsFinalTestResultInfo>>(clientEncounter.ObsFinalTestResults.ToList());
                            e.ObsTraceResults = Mapper.Map<List<ObsTraceResultInfo>>(clientEncounter.ObsTraceResults.ToList());
                            e.ObsLinkages = Mapper.Map<List<ObsLinkageInfo>>(clientEncounter.ObsLinkages.ToList());
                            e.ObsMemberScreenings = Mapper.Map<List<ObsMemberScreeningInfo>>(clientEncounter.ObsMemberScreenings.ToList());
                            e.ObsPartnerScreenings = Mapper.Map<List<ObsPartnerScreeningInfo>>(clientEncounter.ObsPartnerScreenings.ToList());
                            e.ObsFamilyTraceResults = Mapper.Map<List<ObsFamilyTraceResultInfo>>(clientEncounter.ObsFamilyTraceResults.ToList());
                            e.ObsPartnerTraceResults = Mapper.Map<List<ObsPartnerTraceResultInfo>>(clientEncounter.ObsPartnerTraceResults.ToList());
                            es.Add(e);
                        }
                        rc.Encounters = es;
                    }
                    personData.Add(rc);
                }



                return Ok(personData);
            }
            catch (Exception e)
            {
                Log.Debug($"Error loading cohort: {e}");
                return StatusCode(500, "Error loading cohort");
            }
        }
    }
}
