using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BleBeaconServer.DataClasses.Parsers
{
    public class APlantParser
    {
        BeaconData beaconData = null;

        public APlantParser(BeaconData data)
        {
            this.beaconData = data;
        }

        private byte[] aplantbytes = new byte[] { 0xB1, 0x49, 0x88, 0xAA, 0x99, 0xB5, 0xC1, 0x51 }; //aplant

        public APlantData ReadData(byte[] data)
        {
            APlantData aplantData = null;
            if (data != null)
            {
                bool aplantTagFound = false;

                int i = 0;
                int j = 0;

                int idx = 0;

                foreach (byte b in data)
                {
                    if (i <= (aplantbytes.Length - 1) &&
                        b == aplantbytes[i])
                    {
                        aplantTagFound = true;
                        i++;

                        if (i == aplantbytes.Length - 1)
                        {
                            idx = j + 2;
                            break;
                        }
                    }
                    else if (aplantTagFound && i < (aplantbytes.Length - 1))
                    {
                        aplantTagFound = false;
                        i = 0;
                    }
                    j++;
                }

                if (aplantTagFound && data.Length >= 30)
                {
                    try
                    {
                        aplantData = new APlantData(beaconData);
                        aplantData.Temp = data[data.Length - 3];
                        aplantData.SoilMoisture = data[data.Length - 4];
                    }catch(Exception ex)
                    {
                        Debug.WriteLine("[aplantparser] Exception caugth: " + ex.Message);
                    }
                }
            }
            return aplantData;
        }
    }
}
