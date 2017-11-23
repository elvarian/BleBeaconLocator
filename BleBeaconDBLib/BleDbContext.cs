using System;
using System.Collections.Generic;
using System.Text;
using BleBeaconServer.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace BleBeaconDBLib
{
    public class BleDbContext : DbContext
    {
        public virtual DbSet<BleNode> BleNodes { get; set; }
        public virtual DbSet<BleBeacon> BleBeacons { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Map> Maps { get; set; }
        public virtual DbSet<BleDistance> Distances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=sql4.f-solutions.fi;Database=blebeaconserver_fsol;Username=blebeaconserver_fsol;Password=Urj8Pe4UmZuPNa9T");
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
        }
    }
}
