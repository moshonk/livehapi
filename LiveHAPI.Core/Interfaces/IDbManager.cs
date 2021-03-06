﻿using LiveHAPI.Core.Model.Setting;

namespace LiveHAPI.Core.Interfaces
{
    public interface IDbManager
    {
        bool VerfiyServer(DbProtocol dbProtocol);
        bool Verfiy(DbProtocol dbProtocol);
        bool Verfiy(string connectionString);
        string BuildConncetion(DbProtocol dbProtocol);
        DbProtocol ReadConnection(string connectionString);
        
    }
}