using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class RuuvitagData: BeaconData
    {
        /*
        private int packetType;
        public int PacketType
        {
            get { return packetType; }
            set { packetType = value; }
        }
        private int eventCode;
        public int EventCode
        {
            get { return eventCode; }
            set { eventCode = value; }
        }
        private int packetLength;
        public int PacketLength
        {
            get { return packetLength; }
            set { packetLength = value; }
        }
        private int subEvent;
        public int SubEvent
        {
            get { return subEvent; }
            set { subEvent = value; }
        }

        private int numberOfReports;
        public int NumberOfReports
        {
            get { return numberOfReports; }
            set { numberOfReports = value; }
        }

        private int eventType;
        public int EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }

        private int peerAddressType;
        public int PeerAddressType
        {
            get { return peerAddressType; }
            set { peerAddressType = value; }
        }
        
        private List<Report> reports;
        public List<Report> Reports
        {
            get { return reports; }
            set { reports = value; }
        }*/
        
        private double humidity;
        public double Humidity
        {
            get { return humidity; }
            set { humidity = value; }
        }

        private sbyte temp;
        public sbyte Temp
        {
            get { return temp; }
            set { temp = value; }
        }

        private int pressure;
        public int Pressure
        {
            get { return pressure; }
            set { pressure = value; }
        }

        

        public RuuvitagData(BeaconData data)
        {
            this.Rssi = data.Rssi;
            this.Mac = data.Mac;
            this.Type = data.Type;
            this.Date = data.Date;
            this.Beacon = data.Beacon;
        }

        /*
        public Report.AdvertisementData findAdvertisementDataByType(int type)
        {
            if (reports == null)
            {
                return null;
            }
            return reports.stream()
                    .filter(r->r.advertisements != null)
                    .flatMap(r->r.advertisements.stream())
                    .filter(a->a.type != null)
                    .filter(a->a.type == type)
                    .findFirst()
                    .orElse(null);
        }*/
    }
}
