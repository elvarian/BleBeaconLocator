using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class PebblebeeData: BeaconData
    {
        private bool buttonOn;
        public bool ButtonOn
        {
            get { return buttonOn; }
            set { buttonOn = value; }
        }

        public PebblebeeData(BeaconData data)
        {
            this.Mac = data.Mac;
            this.Rssi = data.Rssi;
            this.Type = data.Type;
            this.Date = data.Date;
            this.Beacon = data.Beacon;
        }
    }
}
