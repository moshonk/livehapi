﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LiveHAPI.Shared.Custom;
using LiveHAPI.Shared.Model;

namespace LiveHAPI.Core.Model.Network
{
    public class MasterFacility:Entity<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public int AreaCode { get; set; }
        [MaxLength(100)]
        public string AreaInfo { get; set; }

        public MasterFacility()
        {
        }

        public MasterFacility(int id, string name, int areaCode, string areaInfo)
        {
            Id = id;
            Name = name;
            AreaCode = areaCode;
            AreaInfo = areaInfo;
        }

        public override string ToString()
        {
            return $"{Id}-{Name}  ({AreaInfo})";
        }
    }
}