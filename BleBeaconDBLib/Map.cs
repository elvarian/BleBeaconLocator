using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BleBeaconServer.DbEntities
{
    [Table("maps")]
    public class Map
    {
        [Key]
        [Column("maps_id")]
        public int MapsId { get; set; }

        [Column("width")]
        public double Width { get; set; }

        [Column("height")]
        public double Height { get; set; }

        //public List<BleNode> Nodes { get; set; }

    }
}
