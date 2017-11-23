using BleBeaconDBLib;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BleBeaconServer.DbEntities
{
    public class BleBeaconServerContext : DbContext
    {
        public virtual DbSet<BleNode> BleNodes { get; set; }
        public virtual DbSet<BleBeacon> BleBeacons { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Map> Maps { get; set; }
        public virtual DbSet<BleDistance> Distances { get; set; }
        public virtual DbSet<BleLastLocation> LastLocations { get; set; }

        public static IConfigurationRoot Configuration { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //string dir = Directory.GetCurrentDirectory();

            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            
            Configuration = builder.Build();

            string host = Configuration["db_host"];
            string database = Configuration["db_database"];
            string username = Configuration["db_username"];
            string password = Configuration["db_password"];

            optionsBuilder.UseNpgsql("Host="+host+";Database="+database+";Username="+username+";Password="+password);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<BleBeacon>(entity =>
            {
                entity.Property(e => e.MacAddress).IsRequired();
                
                entity.Property(e => e.BleBeaconsId).HasDefaultValueSql("nextval('ble_beacons_ble_beacons_id_seq'::regclass)");

                modelBuilder.HasSequence("ble_beacons_ble_beacons_id_seq");
                
            });*/
            modelBuilder.Entity<BleNode>().HasOne(n => n.Map);
            //modelBuilder.Entity<Map>().HasMany(m => m.Nodes).WithOne(n => n.Map);
            modelBuilder.Entity<Location>().HasOne(l => l.BleBeacon);
            modelBuilder.Entity<BleDistance>().HasOne(d => d.Beacon);
            modelBuilder.Entity<BleDistance>().HasOne(d => d.Node);
            modelBuilder.Entity<BleLastLocation>().HasOne(ll => ll.Beacon);
        }
    }
}
