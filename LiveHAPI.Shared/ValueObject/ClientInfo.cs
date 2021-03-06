﻿using System;
using System.Collections.Generic;
using LiveHAPI.Shared.Interfaces.Model;

namespace LiveHAPI.Shared.ValueObject
{
    public class ClientInfo:IClient
    {
        public Guid Id { get; set; }
        public string MaritalStatus { get; set; }
        public string KeyPop { get; set; }
        public string OtherKeyPop { get; set; }
        public bool? IsFamilyMember { get; set; }
        public bool? IsPartner { get; set; }
        public Guid? PracticeId { get; set; }
        public string PracticeCode { get; set; }
        public Guid PersonId { get; set; }
        public PersonInfo Person { get; set; }
        public Guid UserId { get; set; }
        public List<IdentifierInfo> Identifiers { get; set; }=new List<IdentifierInfo>();
        public List<RelationshipInfo> Relationships { get; set; } = new List<RelationshipInfo>();
        public List<ClientStateInfo> ClientStates { get; set; }=new List<ClientStateInfo>(); 
        public List<ClientSummaryInfo> ClientSummaries { get; set; }=new List<ClientSummaryInfo>();
        public bool? PreventEnroll { get; set; }
        public bool? AlreadyTestedPos { get; set; }

        public bool HasRelationships()
        {
            return Relationships.Count > 0;
        }

        public ClientInfo()
        {
            IsFamilyMember = IsPartner = false;
        }

        /*
         client.Person.FirstName,
                client.Person.MiddleName,
                client.Person.LastName,
                GetSex(client.Person.Gender),
                client.Person.BirthDate.Value,
                GetDobPrecion(client.Person.BirthDateEstimated.Value),
                client.Identifiers.First().Identifier,
                locationId,
                client.Identifiers.First().RegistrationDate,
                client.Id,
                client.Person.Addresses.FirstOrDefault().Landmark,
                client.Person.Contacts.FirstOrDefault().Phone.ToString()
         */
    }
}