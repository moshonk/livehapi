﻿using System;
using LiveHAPI.Shared.Interfaces.Model;

namespace LiveHAPI.Shared.ValueObject
{
    public class ContactInfo : IContact
    {
        public Guid Id { get; set; }
        public int Phone { get; set; }
        public Guid PersonId { get; set; }
        public ContactInfo(Guid id,int phone,Guid personId)
        {
            Id = id;
            Phone = phone;
            PersonId = personId;
        }

        public ContactInfo()
        {
        }
    }
}