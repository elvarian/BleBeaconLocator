using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BleBeaconServer.DbEntities
{
    [Table("ble_nodes")]
    public class BleNode
    {
        [Key]
        [Column("ble_nodes_id")]
        public int BleNodesId { get; set; }
        //[Column("tx_power")]
        //public int TxPower { get; set; }
        [Column("sender")]
        public string Sender { get; set; }
        [Column("x")]
        public double X { get; set; }
        [Column("y")]
        public double Y { get; set; }

        [Column("maps_id")]
        public int MapsId { get; set; }
        public Map Map { get; set; }
    }
}
