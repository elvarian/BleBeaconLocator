using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class FilterContainer
    {
        private UKF filter = new UKF();
        public UKF Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        private double lastEstimate;
        public double LastEstimate
        {
            get
            {
                return lastEstimate;
            }
            set
            {
                lastEstimate = value;
            }
        }
    }
}
