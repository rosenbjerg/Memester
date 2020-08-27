﻿using Microsoft.EntityFrameworkCore;

namespace Memester.Database
{
    public class HangfireContext : DbContext
    {
        private readonly string _connectionString;

        public static void EnsureCreated(string connectionString)
        {
            using var context = new HangfireContext(connectionString);
            context.Database.EnsureCreated();
        }
        public static void EnsureRecreated(string connectionString)
        {
            using var context = new HangfireContext(connectionString);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private HangfireContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder.UseNpgsql(_connectionString));
        }
    }
}