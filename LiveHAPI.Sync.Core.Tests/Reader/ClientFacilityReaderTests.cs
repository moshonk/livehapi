﻿using System;
using System.Linq;
using LiveHAPI.Sync.Core.Interface.Readers;
using LiveHAPI.Sync.Core.Reader;
using NUnit.Framework;

namespace LiveHAPI.Sync.Core.Tests.Reader
{
    [TestFixture]
    public class ClientFacilityReaderTests
    {
        private readonly string _baseUrl = "http://192.168.1.217/iqcareapi";
        private IClientFacilityReader _reader;

        [SetUp]
        public void Setup()
        {
             _reader = new ClientFacilityReader(new RestClient(_baseUrl));
        }

        [Test]
        public void should_Read_Facilitys()
        {
            var users = _reader.Read().Result.ToList();
            Assert.True(users.Any());
            foreach (var clientFacility in users)
            {
                Console.WriteLine(clientFacility);
            }
        }
    }
}