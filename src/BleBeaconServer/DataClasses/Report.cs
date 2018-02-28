using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class Report
    {
        private int length;
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        private List<AdvertisementData> advertisements;
        public List<AdvertisementData> Advertisements
        {
            get { return advertisements; }
            set { advertisements = value; }
        }
    }
}
