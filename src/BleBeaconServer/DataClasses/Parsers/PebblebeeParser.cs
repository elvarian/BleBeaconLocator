using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BleBeaconServer.DataClasses.Parsers
{
    public class PebblebeeParser
    {
        BeaconData beaconData = null;

        public PebblebeeParser(BeaconData data)
        {
            this.beaconData = data;
        }

        private byte[] pebblebeebytes = new byte[] { 0x19, 0xC1, 0x03, 0x03, 0x02, 0xE0, 0xFF }; //pebblebee

        public PebblebeeData ReadData(byte[] data)
        {
            PebblebeeData pebbleData = null;
            if (data != null)
            {
                bool pebbleTagFound = false;

                int i = 0;
                int j = 0;

                int idx = 0;

                foreach (byte b in data)
                {
                    if (i <= (pebblebeebytes.Length - 1) &&
                        b == pebblebeebytes[i])
                    {
                        pebbleTagFound = true;
                        i++;

                        if (i == pebblebeebytes.Length - 1)
                        {
                            idx = j + 2;
                            break;
                        }
                    }
                    else if (pebbleTagFound && i < (pebblebeebytes.Length - 1))
                    {
                        pebbleTagFound = false;
                        i = 0;
                    }
                    j++;
                }

                if (pebbleTagFound && data.Length >= 30)
                {
                    try
                    {
                        pebbleData = new PebblebeeData(beaconData);
                        int value = data[data.Length - 2];
                        if(value == 0)
                            pebbleData.ButtonOn = false;
                        else if(value == 1)
                            pebbleData.ButtonOn = true;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("[pebblebeeparser] Exception caugth: " + ex.Message);
                    }
                }
            }
            return pebbleData;
        }
    }
}
