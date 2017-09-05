﻿using LiveHAPI.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace LiveHAPI.Infrastructure
{
    public class LiveHAPIContext : DbContext
    {
        public LiveHAPIContext(DbContextOptions<LiveHAPIContext> options) 
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<County> Counties { get; set; }
        public DbSet<SubCounty> SubCounties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubCounty>(
                x =>
                {
                    x.Property<int>("CountyId");
                    x.HasOne<County>().WithMany().HasForeignKey("CountyId");
                });
        }
    }
}