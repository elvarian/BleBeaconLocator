using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BleBeaconDBLib
{
    [Table("ble_distances")]
    public class BleDistance
    {
        [Key]
        [Column("ble_distances_id")]
        public int BleDistancesId { get; set; }
        
        [Column("ble_beacons_id")]
        public int BleBeaconsId { get; set; }

        public BleBeacon Beacon { get; set; }

        [Column("ble_nodes_id")]
        public int BleNodesId { get; set; }

        public BleNode Node { get; set; }

        [Column("distance")]
        public double Distance { get; set; }
    }
}
