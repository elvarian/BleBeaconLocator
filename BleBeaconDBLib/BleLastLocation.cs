using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BleBeaconDBLib
{
    [Table("ble_last_locations")]
    public class BleLastLocation
    {
        [Key]
        [Column("ble_last_locations_id")]
        public int BleLastLocationsId { get; set; }

        [Column("ble_beacons_id")]
        public int BleBeaconsId { get; set; }

        public BleBeacon Beacon { get; set; }

        [Column("x")]
        public double X { get; set; }
        [Column("y")]
        public double Y { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }
    }
}
