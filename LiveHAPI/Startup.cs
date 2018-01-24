﻿  using System;
 using LiveHAPI.Core.Interfaces.Handler;
using LiveHAPI.Core.Interfaces.Repository;
using LiveHAPI.Core.Interfaces.Repository.Subscriber;
using LiveHAPI.Core.Interfaces.Services;
 using LiveHAPI.Core.Model.Encounters;
 using LiveHAPI.Core.Model.Lookup;
using LiveHAPI.Core.Model.QModel;
using LiveHAPI.Core.Model.Studio;
using LiveHAPI.Core.Model.Subscriber;
using LiveHAPI.Core.Service;
using LiveHAPI.Infrastructure;
using LiveHAPI.Infrastructure.Repository;
using LiveHAPI.IQCare.Core.Handlers;
using LiveHAPI.IQCare.Core.Interfaces.Repository;
using LiveHAPI.IQCare.Infrastructure;
using LiveHAPI.IQCare.Infrastructure.Repository;
using LiveHAPI.Shared.ValueObject;
using LiveHAPI.Shared.ValueObject.Meta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
 using Action = LiveHAPI.Core.Model.QModel.Action;

namespace LiveHAPI
{
    public class Startup
    {

        public static IConfiguration Configuration;

        public Startup(IHostingEnvironment env)
        {
            //TODO: Use Environment Variables

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddMvcOptions(o => o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()))
                .AddJsonOptions(o =>o.SerializerSettings.ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            var connectionString = Startup.Configuration["connectionStrings:hAPIConnection"];
            services.AddDbContext<LiveHAPIContext>(o => o.UseSqlServer(connectionString));

            var emrconnectionString = Startup.Configuration["connectionStrings:EMRConnection"];
            services.AddDbContext<EMRContext>(o => o.UseSqlServer(emrconnectionString));

            services.AddScoped<IMasterFacilityRepository, MasterFacilityRepository>();
            services.AddScoped<IObsRepository, ObsRepository>();

            services.AddScoped<IObsTraceResultRepository, ObsTraceResultRepository>();
            services.AddScoped<IObsTestResultRepository, ObsTestResultRepository>();
            services.AddScoped<IObsFinalTestResultRepository, ObsFinalTestResultRepository>();
            services.AddScoped<IObsLinkageRepository, ObsLinkageRepository>();
            services.AddScoped<IObsMemberScreeningRepository, ObsMemberScreeningRepository>();
            services.AddScoped<IObsFamilyTraceResultRepository, ObsFamilyTraceResultRepository>();
            services.AddScoped<IObsPartnerScreeningRepository, ObsPartnerScreeningRepository>();
            services.AddScoped<IObsPartnerTraceResultRepository, ObsPartnerTraceResultRepository>();

            services.AddScoped<IPersonNameRepository, PersonNameRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IPracticeActivationRepository, PracticeActivationRepository>();
            services.AddScoped<IPracticeRepository, PracticeRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();            
            services.AddScoped<ISubCountyRepository, SubCountyRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ICountyRepository, CountyRepository>();
            services.AddScoped<IEncounterRepository, EncounterRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<ISubscriberSystemRepository, SubscriberSystemRepository>();
            

            services.AddScoped<IMetaService, MetaService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IActivationService, ActivationService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IEncounterService, EncounterService>();
            services.AddScoped<IFormsService, FormsService>();

            services.AddScoped<IClientSavedHandler,ClientSavedHandler>();
            services.AddScoped<IEncounterSavedHandler, EncounterSavedHandler>();

            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IPatientEncounterRepository, PatientEncounterRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, LiveHAPIContext context,EMRContext emrContext)
        {
            
            
            



            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //                                    if (!context.AllMigrationsApplied())
            //                                    {
            //                                        context.Database.Migrate();
            //                                        context.EnsureSeeded();
            //                                    }

            Log.Debug($"database initializing...");

            bool imHapi = true;
            string herror = "";
            try
            {
                context.EnsureSeeded();
            }
            catch (Exception e)
            {
                herror = "Seeding";
                imHapi = false;
                Log.Error(new string('<', 30));
                Log.Error($"{e}");
                Log.Error(new string('>', 30));
            }

            Log.Debug($"database initializing... [Views]");
            try
            {
                context.CreateViews();
            }
            catch (Exception e)
            {
                herror = "Views";
                imHapi = false;
                Log.Error($"{e}");
            }

            Log.Debug($"database initializing... [EMR Migrations]");
            try
            {
            
                emrContext.ApplyMigrations();
             
            }
            catch (Exception e)
            {
                herror = "Migrations";
                imHapi = false;
                Log.Error($"{e}");
            }

            Log.Debug($"database initializing... [EMR Mappings]");
            try
            {
              
                emrContext.UpdateTranslations();
            }
            catch (Exception e)
            {
                herror = "Translation";
                imHapi = false;
                Log.Error($"{e}");
            }


            app.UseMvc();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<County,CountyInfo>();
                cfg.CreateMap<SubCounty, SubCountyInfo>();

                cfg.CreateMap<Category, CategoryInfo>();
                cfg.CreateMap<Item, ItemInfo>();
                cfg.CreateMap<CategoryItem, CategoryItemInfo>();

                cfg.CreateMap<PracticeType, PracticeTypeInfo>();
                cfg.CreateMap<IdentifierType, IdentifierTypeInfo>();
                cfg.CreateMap<RelationshipType, RelationshipTypeInfo>();
                cfg.CreateMap<KeyPop, KeyPopInfo>();
                cfg.CreateMap<MaritalStatus, MaritalStatusInfo>();
                cfg.CreateMap<ProviderType, ProviderTypeInfo>();
                cfg.CreateMap<Action, ActionInfo>();
                cfg.CreateMap<Condition, ConditionInfo>();
                cfg.CreateMap<ValidatorType, ValidatorTypeInfo>();
                cfg.CreateMap<CategoryItem, CategoryItemInfo>();
                cfg.CreateMap<ConceptType, ConceptTypeInfo>();
                cfg.CreateMap<Validator, ValidatorInfo>();
                cfg.CreateMap<EncounterType, EncounterTypeInfo>();

                cfg.CreateMap<SubscriberCohort, CohortInfo>();

                cfg.CreateMap<Encounter, EncounterInfo>();
                cfg.CreateMap<Obs, ObsInfo>();
                cfg.CreateMap<ObsTestResult, ObsTestResultInfo>();
                cfg.CreateMap<ObsFinalTestResult, ObsFinalTestResultInfo>();
                cfg.CreateMap<ObsTraceResult, ObsTraceResultInfo>();
                cfg.CreateMap<ObsLinkage, ObsLinkageInfo>();
                cfg.CreateMap<ObsMemberScreening, ObsMemberScreeningInfo>();
                cfg.CreateMap<ObsPartnerScreening, ObsPartnerScreeningInfo>();
                cfg.CreateMap<ObsFamilyTraceResult, ObsFamilyTraceResultInfo>();
                cfg.CreateMap<ObsPartnerTraceResult, ObsPartnerTraceResultInfo>();
            });

            Log.Debug(@"
                            ╔═╗┌─┐┬ ┬┌─┐  ╔╦╗┌─┐┌┐ ┬┬  ┌─┐
                            ╠═╣├┤ └┬┘├─┤  ║║║│ │├┴┐││  ├┤ 
                            ╩ ╩└   ┴ ┴ ┴  ╩ ╩└─┘└─┘┴┴─┘└─┘
                      ");
            Log.Debug("");
            Log.Debug(@"
                                  _        _    ____ ___ 
                                 | |__    / \  |  _ \_ _|
                                 | '_ \  / _ \ | |_) | | 
                                 | | | |/ ___ \|  __/| | 
                                 |_| |_/_/   \_\_|  |___|
                    ");

            if (imHapi)
            {
                Log.Debug($"im hAPI !!!");
            }
            else
            {
                Log.Error($"im NOT hAPI    >*|*< ");
                Log.Error($"cause: {herror}");
            }
           
        }
    }
}
