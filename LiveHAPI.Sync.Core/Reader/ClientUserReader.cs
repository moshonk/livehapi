﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LiveHAPI.Sync.Core.Interface;
using LiveHAPI.Sync.Core.Model;

namespace LiveHAPI.Sync.Core.Reader
{
    public class ClientUserReader : ClientReader<ClientUser>,IClientUserReader
    {
        public ClientUserReader(HttpClient httpClient) : base(httpClient)
        {
        }

        public override Task<IEnumerable<ClientUser>> Read()
        {
            return Read("api/setup/getUsers");
        }
    }
}