﻿using System;
using LiveHAPI.Core.Interfaces.Repository;
using LiveHAPI.Core.Interfaces.Services;
using LiveHAPI.Core.Model.Network;
using LiveHAPI.Core.ValueModel;

namespace LiveHAPI.Core.Service
{
    public class ActivationService:IActivationService
    {
        private readonly IPracticeRepository _practiceRepository;
        private readonly IPracticeActivationRepository _practiceActivationRepository;
        private readonly IMasterFacilityRepository _masterFacilityRepository;

        public ActivationService(IPracticeRepository practiceRepository, IPracticeActivationRepository practiceActivationRepository, IMasterFacilityRepository masterFacilityRepository)
        {
            _practiceRepository = practiceRepository;
            _practiceActivationRepository = practiceActivationRepository;
            _masterFacilityRepository = masterFacilityRepository;
        }

        public MasterFacility Verify(int code)
        {
            var facility = _masterFacilityRepository.Get(code);

            if (null == facility)
                throw new ArgumentException("Facility does not exist");

            return facility;
        }

        public Practice EnrollPractice(string code)
        {
            var facility = Verify(Convert.ToInt32(code));

            if (null != facility)
            {
                var practice = Practice.CreateFacility(facility);
                _practiceRepository.InsertOrUpdate(practice);
                return practice;
            }
            return null;
        }

        public string GetActivationCode(string code, DeviceIdentity identity, DeviceLocation location=null)
        {
            
            var practice = _practiceRepository.GetByCode(code);

            if (null == practice)
                throw new ArgumentException("Facility Code Not found");

            var activation= practice.ActivateDevice(identity, location);

            if(null==activation)
                throw new ArgumentException("Actrivation not possible");

            _practiceActivationRepository.InsertOrUpdate(activation);

            return activation.ActivationCode;
        }
    }
}