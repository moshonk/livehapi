﻿using System;
using System.Linq;
using LiveHAPI.Core.Interfaces.Repository;
using LiveHAPI.Infrastructure.Repository;
using LiveHAPI.Shared.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace LiveHAPI.Infrastructure.Tests.Repository
{
    [TestFixture]
    public class PersonRepositoryTests
    {
        private LiveHAPIContext _context;
        private IPersonRepository _personRepository;

        [SetUp]
        public void SetUp()
        {
             var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = config["connectionStrings:hAPIConnection"];

            var options = new DbContextOptionsBuilder<LiveHAPIContext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new LiveHAPIContext(options);
            TestData.Init();
            TestDataCreator.Init(_context);

            _personRepository = new PersonRepository(_context);
        }

        [Test]
        public void should_Get_Staff()
        {
            var practice = _personRepository.GetStaff().ToList();
            Assert.IsTrue(practice.Count>0);
            foreach (var person in practice)
            {
                Console.WriteLine(person);
            }
        }
    }
}