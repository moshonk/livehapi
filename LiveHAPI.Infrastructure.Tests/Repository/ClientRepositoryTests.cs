﻿using System;
using System.Linq;
using LiveHAPI.Core.Interfaces.Repository;
using LiveHAPI.Core.Model.Exchange;
using LiveHAPI.Infrastructure.Repository;
using LiveHAPI.Shared.Enum;
using LiveHAPI.Shared.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace LiveHAPI.Infrastructure.Tests.Repository
{
    [TestFixture]
    public class ClientRepositoryTests
    {
        private LiveHAPIContext _context;
        private IClientRepository _clientRepository;
        private IClientStageRepository _clientStageRepository;

        [SetUp]
        public void SetUp()
        {
             var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = config["connectionStrings:livehAPIConnection"];

            var options = new DbContextOptionsBuilder<LiveHAPIContext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new LiveHAPIContext(options);
            //TestData.Init();
            //TestDataCreator.Init(_context);

            _clientRepository = new ClientRepository(_context);
            _clientStageRepository=new ClientStageRepository(_context);
        }

       
        [Test]
        public void should_Search_Client()
        {
            var personMatches = _clientRepository.Search("H0002").ToList();
            Assert.True(personMatches.Count > 0);
            foreach (var personMatch in personMatches.OrderByDescending(x=>x.Rank))
            {
                Console.WriteLine(personMatch);
                Assert.IsTrue(personMatch.Person.Clients.Count>0);
                Console.WriteLine(personMatch.Person.Clients.First());
            }
        }
        
        [Test]
        public void should_UpdateSync()
        {
            var stages = _clientStageRepository.GetAll().ToList();
            
            _clientRepository.UpdateSyncStatus(stages);
            
            _clientRepository=new ClientRepository(_context);
            var syncedClients = _clientRepository.GetAll().ToList();
            Assert.True(syncedClients.First().SyncStatus==SyncStatus.Synced);
        }
    }
}