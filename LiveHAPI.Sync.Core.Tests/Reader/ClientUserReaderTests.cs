﻿using System;
using System.Linq;
using System.Net.Http;
using LiveHAPI.Sync.Core.Interface;
using LiveHAPI.Sync.Core.Reader;
using NUnit.Framework;

namespace LiveHAPI.Sync.Core.Tests.Reader
{
    [TestFixture]
    public class ClientUserReaderTests
    {
        private readonly string _baseUrl = "http://localhost:3333";
        private HttpClient _httpClient;
        private IClientUserReader _reader;

        [SetUp]
        public void Setup()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _reader = new ClientUserReader(_httpClient);
        }

        [Test]
        public void should_Read_Users()
        {
            var users = _reader.Read().Result.ToList();
            Assert.True(users.Any());
            foreach (var clientUser in users)
            {
                Console.WriteLine(clientUser);
            }
        }
    }
}