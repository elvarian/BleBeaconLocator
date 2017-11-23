using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BleBeaconServer.DbEntities
{
    [Table("locations")]
    public class Location
    {
        [Key]
        [Column("locations_id")]
        public int LocationsId { get; set; }

        [Column("x")]
        public double X { get; set; }
        [Column("y")]
        public double Y { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("ble_beacons_id")]
        public int BleBeaconsId { get; set; }

        public virtual BleBeacon BleBeacon { get; set; }
        
    }
}
