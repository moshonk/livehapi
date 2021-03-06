﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiveHAPI.Shared.Custom;
using LiveHAPI.Shared.Interfaces.Model;
using LiveHAPI.Shared.Model;
using LiveHAPI.Shared.ValueObject;

namespace LiveHAPI.Core.Model.Encounters
{
    public class ObsFinalTestResult : Entity<Guid>, IObsFinalTestResult
    {
        
        public Guid? FirstTestResult { get; set; }
        public string FirstTestResultCode { get; set; }
        
        public Guid? SecondTestResult { get; set; }
        public string SecondTestResultCode { get; set; }
        
        public Guid? FinalResult { get; set; }
        public string FinalResultCode { get; set; }
        
        public Guid? ResultGiven { get; set; }
        
        public Guid? CoupleDiscordant { get; set; }
        /// <summary>
        /// Accepted to partner listing
        /// </summary>
        public Guid? SelfTestOption { get; set; }
        public Guid? PnsDeclined { get; set; }
        [MaxLength(100)]
        public string Remarks { get; set; }
        public Guid ClientId { get; set; }
        public Guid EncounterId { get; set; }


        public ObsFinalTestResult()
        {
            Id = LiveGuid.NewGuid();
        }

        private ObsFinalTestResult(Guid id,Guid? firstTestResult, string firstTestResultCode, Guid? secondTestResult, string secondTestResultCode, Guid? finalResult, string finalResultCode, Guid? resultGiven, Guid? coupleDiscordant, Guid? selfTestOption, string remarks, Guid? pnsDeclined,Guid encounterId,Guid clientId)
        {
            Id = id;
            FirstTestResult = firstTestResult;
            FirstTestResultCode = firstTestResultCode;
            SecondTestResult = secondTestResult;
            SecondTestResultCode = secondTestResultCode;
            FinalResult = finalResult;
            FinalResultCode = finalResultCode;
            ResultGiven = resultGiven;
            CoupleDiscordant = coupleDiscordant;
            SelfTestOption = selfTestOption;
            Remarks = remarks;
            PnsDeclined = pnsDeclined;
            EncounterId = encounterId;
            ClientId = clientId;
        }

        public static ObsFinalTestResult Create(ObsFinalTestResultInfo obsInfo)
        {
            return new ObsFinalTestResult(obsInfo.Id,obsInfo.FirstTestResult, obsInfo.FirstTestResultCode, obsInfo.SecondTestResult, obsInfo.SecondTestResultCode, obsInfo.FinalResult, obsInfo.FinalResultCode, obsInfo.ResultGiven,
                obsInfo.CoupleDiscordant, obsInfo.SelfTestOption,obsInfo.Remarks, obsInfo.PnsDeclined, obsInfo.EncounterId,obsInfo.ClientId);
        }

        public static List<ObsFinalTestResult> Create(EncounterInfo encounterInfo)
        {
            var list = new List<ObsFinalTestResult>();

            foreach (var obsInfo in encounterInfo.ObsFinalTestResults)
            {
                list.Add(Create(obsInfo));
            }
            return list;
        }
    }
}