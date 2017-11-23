using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BleBeaconServer.DbEntities
{
    [Table("ble_beacons")]
    public class BleBeacon
    {
        [Key]
        [Column("ble_beacons_id")]
        public int BleBeaconsId { get; set; }
        [Column("mac_address")]
        public string MacAddress { get; set; }

        private string name = "";
        [Column("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return name ?? "";
        }
    }
}
