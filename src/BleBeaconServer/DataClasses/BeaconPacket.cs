using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class BeaconPacket
    {
        public int device_type = -1;
        public string mac_address = "";
        public int tx_power = 0;
        public int rssi = 0;
        public int sender = 0;
        public BeaconAddon addons;
    }
}
