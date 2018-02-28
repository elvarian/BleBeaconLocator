using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class BeaconData
    {
        public enum Types
        {
            Unknown,
            PebbleBee,
            Ruuvitag,
            APlant
        }

        private static byte[] PEBBLEBEE = new byte[] { 0x19, 0xC1, 0x03, 0x03, 0x02, 0xE0, 0xFF };
        private static byte[] RUUVITAG1 = new byte[] { 0x72, 0x75, 0x75, 0x2E, 0x76, 0x69, 0x2F };
        private static byte[] APLANT = new byte[] { 0xB1, 0x49, 0x88, 0xAA, 0x99, 0xB5, 0xC1};

        private string mac;
        public string Mac
        {
            get { return mac; }
            set { mac = value; }
        }

        private int rssi;
        public int Rssi
        {
            get { return rssi; }
            set { rssi = value; }
        }

        private Types type;
        public Types Type
        {
            get { return type; }
            set { type = value; }
        }

        private DateTime date = DateTime.Now;
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        private BleBeacon beacon;
        public BleBeacon Beacon
        {
            get { return beacon; }
            set { beacon = value; }
        }

        /*
        public BeaconData(byte[] data)
        {
            type = GetType(data);
        }*/

        public static Types GetType(byte[] data)
        {
            Types type = Types.Unknown;
            if (data.Length >= 35)
            {
                bool typeFound = false;
                int j = 28;
                for (int i = 0; i < 7; i++)
                {
                    if(j==28 && data[j] == PEBBLEBEE[i])
                    {
                        type = Types.PebbleBee;
                        typeFound = true;
                    } else if(j == 28 && data[j] == RUUVITAG1[i])
                    {
                        type = Types.Ruuvitag;
                        typeFound = true;
                    } else if(j == 28 && data[j] == APLANT[i])
                    {
                        type = Types.APlant;
                        typeFound = true;
                    }
                    else if(typeFound && type == Types.PebbleBee)
                    {
                        if(data[j] != PEBBLEBEE[i])
                        {
                            typeFound = false;
                            break;
                        }
                    }
                    else if(typeFound && type == Types.Ruuvitag)
                    {
                        if(data[j] != RUUVITAG1[i])
                        {
                            typeFound = false;
                            break;
                        }
                    } else if(typeFound && type == Types.APlant)
                    {
                        if(data[j] != APLANT[i])
                        {
                            typeFound = false;
                            break;
                        }
                    }
                    j++;
                }
            }

            return type;
        }

        public static BeaconData ParseValues(byte[] data)
        {
            BeaconData beaconData = null;
            if (data != null)
            {
                if(data.Length > 13)
                {
                    beaconData = new BeaconData();
                    beaconData.mac = string.Format("{0:X}:{1:X}:{2:X}:{3:X}:{4:X}:{5:X}", data[12], data[11], data[10], data[9], data[8], data[7]);
                    beaconData.rssi = data[data.Length -1];
                }
            }
            return beaconData;
        }
    }
}
