using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Main.Models;
using Microsoft.Extensions.Options;

namespace Main.Data
{
    public class MainContext : DbContext
    {
        public MainContext (DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
			if (!optionsBuilder.IsConfigured)
			{
				IConfigurationRoot configuration = new ConfigurationBuilder()
				   .SetBasePath(Directory.GetCurrentDirectory())
				   .AddJsonFile("appsettings.json")
				   .Build();
				var connectionString = configuration.GetConnectionString("DbCoreConnectionString");
				optionsBuilder.UseSqlServer(connectionString);
			}
		}

        public DbSet<Main.Models.Session> Session { get; set; } = default!;

        public DbSet<Main.Models.Customer> Customer { get; set; }

        public Dictionary<string, object> Value { get; set; }

        public DbSet<Main.Models.Streamer> Streamer { get; set; }
    }
}
