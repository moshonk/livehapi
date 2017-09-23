﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiveHAPI.Shared.Custom;
using LiveHAPI.Shared.Interfaces.Model;
using LiveHAPI.Shared.Model;
using LiveHAPI.Shared.ValueObject;

namespace LiveHAPI.Core.Model.People
{
    public class ClientIdentifier : Entity<Guid>,IEnrollment
    {
        [MaxLength(50)]
        public string IdentifierTypeId { get; set; }
        [MaxLength(100)]
        public string Identifier { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool Preferred { get; set; }
        public Guid ClientId { get; set; }

        public ClientIdentifier()
        {
            Id = LiveGuid.NewGuid();
        }

        private ClientIdentifier(string identifierTypeId, string identifier, DateTime registrationDate)
        {
            IdentifierTypeId = identifierTypeId;
            Identifier = identifier;
            RegistrationDate = registrationDate;
        }

        public override string ToString()
        {
            return $"{IdentifierTypeId}|{Identifier}";
        }

        public static ClientIdentifier Create(IdentifierInfo address)
        {
            return new ClientIdentifier(address.IdentifierTypeId, address.Identifier, address.RegistrationDate);
        }

        public static List<ClientIdentifier> Create(ClientInfo clientInfo)
        {
            var list = new List<ClientIdentifier>();

            foreach (var address in clientInfo.Identifiers)
            {
                list.Add(Create(address));
            }
            return list;
        }
    }
}