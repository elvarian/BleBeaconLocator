using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class AdvertisementData
    {
        private int length;
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        private int type;
        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        private List<byte> data;
        public List<byte> Data
        {
            get { return data; }
            set { data = value; }
        }

        public byte[] dataBytes()
        {
            if (data == null)
                return new byte[0];
            else
                return data.ToArray();
        }
    }
}
