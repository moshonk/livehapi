﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LiveHAPI.Core.Model.Encounters;
using LiveHAPI.Core.Model.Subscriber;
using LiveHAPI.IQCare.Core.Interfaces.Repository;
using LiveHAPI.IQCare.Core.Model;
using LiveHAPI.Shared;
using LiveHAPI.Shared.Custom;
using LiveHAPI.Shared.ValueObject;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LiveHAPI.IQCare.Infrastructure.Repository
{
    public class PatientEncounterRepository : BaseRepository, IPatientEncounterRepository
    {
        private List<SqlAction> _sqlActions;
        public PatientEncounterRepository(EMRContext context) : base(context)
        {
        }

        public void CreateOrUpdate(List<EncounterInfo> encounters, SubscriberSystem subscriberSystem, Location location)
        {

            var sqlA = GenerateSqlSetupActions(subscriberSystem);
            using (SqlConnection conn = new SqlConnection(Context.Database.GetDbConnection().ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlA, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{e}");
                        throw;
                    }
                }
            }


            foreach (var encounterInfo in encounters)
            {
                var sql = GenerateSqlVisitActions(encounterInfo, subscriberSystem, location);

                using (SqlConnection conn = new SqlConnection(Context.Database.GetDbConnection().ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Log.Error($"{e}");
                            throw;
                        }

                    }
                }
                


                var sql2 = GenerateSqlActionsLinkage(encounterInfo, subscriberSystem, location);

                using (SqlConnection conn = new SqlConnection(Context.Database.GetDbConnection().ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Log.Error($"{e}");
                            throw;
                        }
                    }
                }

                var sql32 = GenerateSqlActionsTracing(encounterInfo, subscriberSystem, location);

                using (SqlConnection conn = new SqlConnection(Context.Database.GetDbConnection().ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql32, conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Log.Error($"{e}");
                            throw;
                        }
                    }
                }


                var sql332 = GenerateSqlActionsTesting(encounterInfo, subscriberSystem, location);

                using (SqlConnection conn = new SqlConnection(Context.Database.GetDbConnection().ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql332, conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Log.Error($"{e}");
                            throw;
                        }
                    }
                }
            }
        }

        private string GenerateSqlSetupActions(SubscriberSystem subscriberSystem)
        {
            decimal rank = 0;
            _sqlActions = new List<SqlAction>();
            _sqlActions.AddRange(SetUpEncounters(rank, subscriberSystem)); rank++;
            StringBuilder sqlBuilder = new StringBuilder(" ");
            foreach (var action in _sqlActions.OrderBy(x => x.Rank))
            {
                sqlBuilder.AppendLine(action.Action);
            }
            return sqlBuilder.ToString();
        }
        private string GenerateSqlVisitActions(EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            decimal rank = 0;
            _sqlActions = new List<SqlAction>();
            _sqlActions.Add(new SqlAction(rank, GetSqlDecrptyion())); rank++;
            _sqlActions.Add(InsertLabDetailVisit(rank, encounterInfo, subscriberSystem, location)); rank++;
            if (encounterInfo.ObsLinkages.Count > 0)
            {
                _sqlActions.Add(InsertLinkageDetailVisit(rank, encounterInfo, subscriberSystem, location)); rank++;
            }
            if (encounterInfo.ObsTraceResults.Count > 0)
            {
                _sqlActions.Add(InsertTracingVisit(rank, encounterInfo, subscriberSystem, location)); rank++;
            }

            StringBuilder sqlBuilder = new StringBuilder(" ");
            foreach (var action in _sqlActions.OrderBy(x => x.Rank))
            {
                sqlBuilder.AppendLine(action.Action);
            }
            return sqlBuilder.ToString();
        }
        private string GenerateSqlActionsLinkage(EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            decimal rank = 0;
            _sqlActions = new List<SqlAction>();
            _sqlActions.Add(new SqlAction(rank, GetSqlDecrptyion())); rank++;

            if (encounterInfo.ObsLinkages.Count > 0) 
                _sqlActions.AddRange(InsertLinkage(rank, encounterInfo, subscriberSystem, location)); rank++;


            StringBuilder sqlBuilder = new StringBuilder(" ");
            foreach (var action in _sqlActions.OrderBy(x => x.Rank))
            {
                sqlBuilder.AppendLine(action.Action);
            }
            return sqlBuilder.ToString();
        }
        private string GenerateSqlActionsTracing(EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            decimal rank = 0;
            _sqlActions = new List<SqlAction>();
            _sqlActions.Add(new SqlAction(rank, GetSqlDecrptyion())); rank++;
            if (encounterInfo.ObsTraceResults.Count > 0)
                _sqlActions.AddRange(InsertTracing(rank, encounterInfo, subscriberSystem, location)); rank++;

            StringBuilder sqlBuilder = new StringBuilder(" ");
            foreach (var action in _sqlActions.OrderBy(x => x.Rank))
            {
                sqlBuilder.AppendLine(action.Action);
            }
            return sqlBuilder.ToString();
        }
        private string GenerateSqlActionsTesting(EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            decimal rank = 0;
            _sqlActions = new List<SqlAction>();
            _sqlActions.Add(new SqlAction(rank, GetSqlDecrptyion())); rank++;
            if (encounterInfo.ObsTestResults.Count > 0)
                _sqlActions.AddRange(InsertTesting(rank, encounterInfo, subscriberSystem, location)); rank++;

            StringBuilder sqlBuilder = new StringBuilder(" ");
            foreach (var action in _sqlActions.OrderBy(x => x.Rank))
            {
                sqlBuilder.AppendLine(action.Action);
            }
            return sqlBuilder.ToString();
        }
        private List<SqlAction> SetUpEncounters(decimal rank, SubscriberSystem subscriberSystem)
        {
            List<SqlAction> sqlActions=new List<SqlAction>();
            var subscriberMaps = subscriberSystem.Maps
                .Where(x=>x.HasSubName())
                .GroupBy(x => x.SubName)
                .Select(x => x.FirstOrDefault());

            foreach (var subscriberMap in subscriberMaps)
            {
                sqlActions.Add(new SqlAction(rank,subscriberMap.GetSqlSetupAction()));
                rank++;
            }
            return sqlActions;
        }
        private SqlAction InsertLabDetailVisit(decimal rank, EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            //Lab.VisitTypeId | 116
            //Linkage.VisitTypeId | 117

              //Registration|VisitTypeId
              var visitType = subscriberSystem.Configs.FirstOrDefault(x => x.Area == "HTS" && x.Name == "Lab.VisitTypeId");

            string sql = $@"

                DECLARE @ptnpk int
                DECLARE @visitipk int
                
                SET @ptnpk=(SELECT TOP 1 Ptn_Pk  FROM mst_Patient WHERE mAfyaId ='{encounterInfo.ClientId}');       

                UPDATE 
	                [ord_Visit] 
                SET 
	                [Ptn_Pk]=@ptnpk,
                    [LocationID]='{location.FacilityID}',
                    [VisitDate]='{encounterInfo.EncounterDate:yyyy MMM dd}',
                    [VisitType]= {visitType.Value},
                    [DataQuality]='0',
                    [UserID]='0',
                    [Signature]='0',
                    [UpdateDate]=GETDATE(),
                    [mAfyaVisitType]=1
                WHERE 
	                Ptn_pk=@ptnpk AND LocationId={location.FacilityID} AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} 
                IF @@ROWCOUNT=0
                    INSERT INTO 
                        ord_Visit(Ptn_Pk, LocationID, VisitDate, VisitType,DataQuality,UserID,Signature,CreateDate,mAfyaVisitType)
                    VALUES(
                        @ptnpk,'{location.FacilityID}', '{encounterInfo.EncounterDate:yyyy MMMM dd}', {visitType.Value},'0', '0','0', GETDATE(),1);
                
                SET @visitipk=(SELECT TOP 1 [Visit_Id] FROM [ord_Visit] WHERE Ptn_Pk=@ptnpk AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} ORDER BY CreateDate desc);
";

            var action = new SqlAction(rank, sql);
            return action;
        }
        private SqlAction InsertLabDetail(decimal rank, EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            
            //Lab.VisitTypeId | 116
            //Linkage.VisitTypeId | 117

            //Registration|VisitTypeId
            var visitType = subscriberSystem.Configs.FirstOrDefault(x => x.Area == "HTS" && x.Name == "Lab.VisitTypeId");

            string sql = $@"

                DECLARE @ptnpk int
                DECLARE @visitipk int
                
                SET @ptnpk=(SELECT TOP 1 Ptn_Pk  FROM mst_Patient WHERE mAfyaId ='{encounterInfo.ClientId}');       

                UPDATE 
	                [ord_Visit] 
                SET 
	                [Ptn_Pk]=@ptnpk,
                    [LocationID]='{location.FacilityID}',
                    [VisitDate]='{encounterInfo.EncounterDate:yyyy MMM dd}',
                    [VisitType]= {visitType.Value},
                    [DataQuality]='0',
                    [UserID]='0',
                    [Signature]='0',
                    [UpdateDate]=GETDATE(),
                    [mAfyaVisitType]=1
                WHERE 
	                Ptn_pk=@ptnpk AND LocationId={location.FacilityID} AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} 
                IF @@ROWCOUNT=0
                    INSERT INTO 
                        ord_Visit(Ptn_Pk, LocationID, VisitDate, VisitType,DataQuality,UserID,Signature,CreateDate,mAfyaVisitType)
                    VALUES(
                        @ptnpk,'{location.FacilityID}', '{encounterInfo.EncounterDate:yyyy MMMM dd}', {visitType.Value},'0', '0','0', GETDATE(),1);
                
                SET @visitipk=(SELECT TOP 1 [Visit_Id] FROM [ord_Visit] WHERE Ptn_Pk=@ptnpk AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} ORDER BY CreateDate desc);
";

            var action = new SqlAction(rank, sql);
            return action;
        }
        private SqlAction InsertLinkageDetailVisit(decimal rank, EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            //Lab.VisitTypeId | 116
            //Linkage.VisitTypeId | 117

            //Registration|VisitTypeId
            var visitType = subscriberSystem.Configs.FirstOrDefault(x => x.Area == "HTS" && x.Name == "Linkage.VisitTypeId");

            string sql = $@"

                UPDATE 
	                [ord_Visit] 
                SET 
	                [Ptn_Pk]=@ptnpk,
                    [LocationID]='{location.FacilityID}',
                    [VisitDate]='{encounterInfo.EncounterDate:yyyy MMM dd}',
                    [VisitType]= {visitType.Value},
                    [DataQuality]='0',
                    [UserID]='0',
                    [Signature]='0',
                    [UpdateDate]=GETDATE(),
                    [mAfyaVisitType]=1
                WHERE 
	                Ptn_pk=@ptnpk AND LocationId={location.FacilityID} AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} 
                IF @@ROWCOUNT=0
                    INSERT INTO 
                        ord_Visit(Ptn_Pk, LocationID, VisitDate, VisitType,DataQuality,UserID,Signature,CreateDate,mAfyaVisitType)
                    VALUES(
                        @ptnpk,'{location.FacilityID}', '{encounterInfo.EncounterDate:yyyy MMMM dd}', {visitType.Value},'0', '0','0', GETDATE(),1);
                
                SET @visitipk=(SELECT TOP 1 [Visit_Id] FROM [ord_Visit] WHERE Ptn_Pk=@ptnpk AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} ORDER BY CreateDate desc);";

            var action = new SqlAction(rank, sql);
            return action;
        }
        private SqlAction InsertTracingVisit(decimal rank, EncounterInfo encounterInfo, SubscriberSystem subscriberSystem, Location location)
        {
            //Lab.VisitTypeId | 116
            //Linkage.VisitTypeId | 117

            //Registration|VisitTypeId
            var visitType = subscriberSystem.Configs.FirstOrDefault(x => x.Area == "HTS" && x.Name == "Linkage.VisitTypeId");

            string sql = $@"

                UPDATE 
	                [ord_Visit] 
                SET 
	                [Ptn_Pk]=@ptnpk,
                    [LocationID]='{location.FacilityID}',
                    [VisitDate]='{encounterInfo.EncounterDate:yyyy MMM dd}',
                    [VisitType]= {visitType.Value},
                    [DataQuality]='0',
                    [UserID]='0',
                    [Signature]='0',
                    [UpdateDate]=GETDATE(),
                    [mAfyaVisitType]=1
                WHERE 
	                Ptn_pk=@ptnpk AND LocationId={location.FacilityID} AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} 
                IF @@ROWCOUNT=0
                    INSERT INTO 
                        ord_Visit(Ptn_Pk, LocationID, VisitDate, VisitType,DataQuality,UserID,Signature,CreateDate,mAfyaVisitType)
                    VALUES(
                        @ptnpk,'{location.FacilityID}', '{encounterInfo.EncounterDate:yyyy MMMM dd}', {visitType.Value},'0', '0','0', GETDATE(),1);
                
                SET @visitipk=(SELECT TOP 1 [Visit_Id] FROM [ord_Visit] WHERE Ptn_Pk=@ptnpk AND mAfyaVisitType=1 AND [VisitType]={visitType.Value} ORDER BY CreateDate desc);";

            var action = new SqlAction(rank, sql);
            return action;
        }
        private List<SqlAction> InsertLinkage(decimal rank, EncounterInfo encounter, SubscriberSystem subscriberSystem, Location location)
        {
            //Lab.VisitTypeId | 116
            //Linkage.VisitTypeId | 117

            //Registration|VisitTypeId
            var visitType = subscriberSystem.Configs.FirstOrDefault(x => x.Area == "HTS" && x.Name == "Linkage.VisitTypeId");

            //GET MAP
            var actions=new List<SqlAction>();

            var maps = subscriberSystem.Maps.Where(x => x.Name == nameof(ObsLinkage)).ToList();

            if (maps.Count > 0)
            {
                //SINGLE

                var mAfyId = encounter.ObsLinkages.FirstOrDefault().Id;
                var mapTbl = maps.Where(x => x.Mode == "Single").Select(x => x.SubName).Distinct().FirstOrDefault();



                string sql22 = $@"

                DECLARE @ptnpk int
                DECLARE @visitipk int
                
                SET @ptnpk=(SELECT TOP 1 Ptn_Pk  FROM mst_Patient WHERE mAfyaId ='{encounter.ClientId}');               
                SET @visitipk=(SELECT TOP 1 Ptn_Pk  FROM  ord_visit WHERE (Ptn_Pk = @ptnpk) AND (VisitType = {visitType.Value}) AND (mAfyaVisitType = 1));       

                        UPDATE 
	                        [{mapTbl}] 
                        SET 
	                        [mAfyaId]='{mAfyId}',
                            [Visit_Pk]=@visitipk,                    
                            [LocationID]='{location.FacilityID}',
                            [UserID]='0',                
                            [UpdateDate]=GETDATE()
                        WHERE 
	                        mAfyaId='{mAfyId}'

                        IF @@ROWCOUNT=0
                            INSERT INTO 
                                    [{mapTbl}](
                                    ptn_pk, Visit_Pk, LocationID, UserID, CreateDate,mAfyaId)
                            VALUES(@ptnpk,@visitipk, 
                                {location.FacilityID}, 0, GETDATE(),'{mAfyId}');
                    ";

                actions.Add(new SqlAction(rank,sql22));
                rank++;

                var obsLinkage = encounter.ObsLinkages.FirstOrDefault();

                

                if(null!=obsLinkage)
                {
                    foreach (var subscriberMap in maps)
                    {
                        string sql223 = $@"

                        UPDATE 
	                        [{mapTbl}] 
                        SET 
	                        [{subscriberMap.SubField}]= {GetValue(obsLinkage,subscriberMap)}
                        WHERE 
	                        mAfyaId='{mAfyId}';
                    ";
                        actions.Add(new SqlAction(rank, sql223));
                        rank++;
                    }
                }
            }

            return actions;
        }
        private List<SqlAction> InsertTracing(decimal rank, EncounterInfo encounter, SubscriberSystem subscriberSystem, Location location)
        {
            //Linkage.VisitTypeId | 117
            var actions = new List<SqlAction>();
            var visitType = subscriberSystem.Configs.FirstOrDefault(x => x.Area == "HTS" && x.Name == "Linkage.VisitTypeId");
            var maps = subscriberSystem.Maps.Where(x => x.Name == nameof(ObsTraceResult)&&x.HasSubName()).ToList();
            if (maps.Count > 0)
            {
                //MULTII
                var mapTbl = maps.FirstOrDefault(x => x.Mode == "Multi");

                var s = $@"
                            DECLARE @ptnpk int
                            DECLARE @visitipk int
                
                            SET @ptnpk=(SELECT TOP 1 Ptn_Pk  FROM mst_Patient WHERE mAfyaId ='{encounter.ClientId}');               
                            SET @visitipk=(SELECT TOP 1 Ptn_Pk  FROM  ord_visit WHERE (Ptn_Pk = @ptnpk) AND (VisitType = {visitType.Value}) AND (mAfyaVisitType = 1));";

                actions.Add(new SqlAction(rank, s));
                rank++;

                Guid mAfyId;
                foreach (var encounterObsTraceResult in encounter.ObsTraceResults)
                {
                    mAfyId = encounterObsTraceResult.Id;
                    string sql22 = $@"

                        UPDATE 
	                        [{mapTbl.SubName}] 
                        SET 
                            [SectionId]='{mapTbl.SectionId}',
                            [FormID]='{mapTbl.FormId}',
	                        [mAfyaId]='{mAfyId}',
                            [Visit_Pk]=@visitipk,                    
                            [LocationID]='{location.FacilityID}',
                            [UserID]='0',                
                            [UpdateDate]=GETDATE()
                        WHERE 
	                        mAfyaId='{mAfyId}'

                        IF @@ROWCOUNT=0
                            INSERT INTO 
                                    [{mapTbl.SubName}](
                                    ptn_pk, Visit_Pk, LocationID, UserID, CreateDate,mAfyaId,SectionId,FormID)
                            VALUES(@ptnpk,@visitipk, 
                                {location.FacilityID}, 0, GETDATE(),'{mAfyId}','{mapTbl.SectionId}','{mapTbl.FormId}');
                    ";

                    actions.Add(new SqlAction(rank, sql22));
                    rank++;

                    
                        foreach (var subscriberMap in maps)
                        {
                            string sql223 = $@"

                        UPDATE 
	                        [{mapTbl.SubName}] 
                        SET 
	                        [{subscriberMap.SubField}]={GetValue(encounterObsTraceResult, subscriberMap,subscriberSystem)}
                        WHERE 
	                        mAfyaId='{mAfyId}';
                    ";
                            actions.Add(new SqlAction(rank, sql223));
                            rank++;
                        }
                    
                }
            }

            return actions;
        }
        private List<SqlAction> InsertTesting(decimal rank, EncounterInfo encounter, SubscriberSystem subscriberSystem, Location location)
        {
            //Linkage.VisitTypeId | 117
            var actions = new List<SqlAction>();
            var visitType = subscriberSystem.Configs.FirstOrDefault(x => x.Area == "HTS" && x.Name == "Lab.VisitTypeId");
            var maps = subscriberSystem.Maps.Where(x => x.Name == nameof(ObsTestResult)&&x.HasSubName()).ToList();
            if (maps.Count > 0)
            {
             

                var s = $@"
                            DECLARE @ptnpk int
                            DECLARE @visitipk int
                
                            SET @ptnpk=(SELECT TOP 1 Ptn_Pk  FROM mst_Patient WHERE mAfyaId ='{encounter.ClientId}');               
                            SET @visitipk=(SELECT TOP 1 Ptn_Pk  FROM  ord_visit WHERE (Ptn_Pk = @ptnpk) AND (VisitType = {visitType.Value}) AND (mAfyaVisitType = 1));";

                actions.Add(new SqlAction(rank, s));
                rank++;

                //MULTII
                var mapTbl = maps.FirstOrDefault(x => x.Mode == "Multi");
                Guid mAfyId;
                foreach (var result in encounter.ObsTestResults)
                {
                    mAfyId = result.Id;
                    string sql22 = $@"

                        UPDATE 
	                        [{mapTbl.SubName}] 
                        SET 
                            [SectionId]='{mapTbl.SectionId}',
                            [FormID]='{mapTbl.FormId}',
	                        [mAfyaId]='{mAfyId}',
                            [Visit_Pk]=@visitipk,                    
                            [LocationID]='{location.FacilityID}',
                            [UserID]='0',                
                            [UpdateDate]=GETDATE()
                        WHERE 
	                        mAfyaId='{mAfyId}'

                        IF @@ROWCOUNT=0
                            INSERT INTO 
                                    [{mapTbl.SubName}](
                                    ptn_pk, Visit_Pk, LocationID, UserID, CreateDate,mAfyaId,SectionId,FormID)
                            VALUES(@ptnpk,@visitipk, 
                                {location.FacilityID}, 0, GETDATE(),'{mAfyId}','{mapTbl.SectionId}','{mapTbl.FormId}');
                    ";

                    actions.Add(new SqlAction(rank, sql22));
                    rank++;


                    foreach (var subscriberMap in maps)
                    {
                        string sql223 = $@"

                        UPDATE 
	                        [{mapTbl.SubName}] 
                        SET 
	                        [{subscriberMap.SubField}]={GetValue(result, subscriberMap, subscriberSystem)}
                        WHERE 
	                        mAfyaId='{mAfyId}';
                    ";
                        actions.Add(new SqlAction(rank, sql223));
                        rank++;
                    }

                }
            }

            return actions;
        }
        private static string GetValue(object obj, SubscriberMap subscriberMap, SubscriberSystem subscriberSystem=null)
        {
            var propname = subscriberMap.Field;

            var val = obj.GetPropValue(propname);

            if (null == val)
                return $"NULL";


            if (subscriberMap.Type == "Date")
            {
                DateTime vall = obj.GetPropValue<DateTime>(propname);
                return $"'{vall:yyyy MMM dd}'";
            }
            else
            {
                if (null != subscriberSystem)
                {

                    val = GetTranslation($"{subscriberMap.Name}.{subscriberMap.Field}", val.ToString(),
                        subscriberSystem);
                }
            }
            
            return $"'{val}'";
        }
        public static string GetTranslation(string tref, string tval, SubscriberSystem subscriberSystem)
        {
            var translatio = subscriberSystem.Translations.FirstOrDefault(x => x.Ref == tref && x.Code == tval);
            if (null == translatio)
                return tval;

            return translatio.SubCode;
        }
    }
}
