using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class APlantData: BeaconData
    {
        private int soilMoisture;
        public int SoilMoisture
        {
            get { return soilMoisture; }
            set { soilMoisture = value; }
        }

        private int temp;
        public int Temp
        {
            get { return temp; }
            set { temp = value; }
        }

        public APlantData(BeaconData data)
        {
            this.Mac = data.Mac;
            this.Rssi = data.Rssi;
            this.Type = data.Type;
            this.Date = data.Date;
            this.Beacon = data.Beacon;
            this.Node = data.Node;
        }
    }
}
