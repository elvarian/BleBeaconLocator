using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BleBeaconServer.DataClasses.Parsers
{
    public class RuuvitagParser
    {
        BeaconData beaconData = null;

        public RuuvitagParser(BeaconData beaconData)
        {
            this.beaconData = beaconData;
        }

        private byte[] ruuvibytes = new byte[] { 0x72, 0x75, 0x75, 0x2E, 0x76, 0x69, 0x2F, 0x23 }; //ruu.vi/#

        public RuuvitagData ReadData(byte[] data)
        {
            RuuvitagData ruuviData = null;
            if (data != null)
            {
                bool ruuviTagFound = false;

                int i = 0;
                int j = 0;

                int idx = 0;

                foreach (byte b in data)
                {
                    if (i <= (ruuvibytes.Length - 1) &&
                        b == ruuvibytes[i])
                    {
                        ruuviTagFound = true;
                        i++;

                        if (i == ruuvibytes.Length - 1)
                        {
                            idx = j + 2;
                            break;
                        }
                    }
                    else if (ruuviTagFound && i < (ruuvibytes.Length - 1))
                    {
                        ruuviTagFound = false;
                        i = 0;
                    }
                    j++;
                }

                if (ruuviTagFound && data.Length - 1 > idx)
                {
                    int eddisonUrlLength = data.Length - idx - 1;
                    int dataFormat = 0;
                    if (eddisonUrlLength > 8)
                        eddisonUrlLength = 8;
                    byte[] eddisonUrlData = new byte[eddisonUrlLength];
                    
                    j = 0;
                    for (i = idx; i < data.Length; i++)
                    {
                        if (i < data.Length - 1 && j < eddisonUrlData.Length)
                        {
                            eddisonUrlData[j++] = data[i];
                        }
                    }

                    string base64Str = System.Text.Encoding.UTF8.GetString(eddisonUrlData);
                    try
                    {
                        byte[] dataBytes = Convert.FromBase64String(base64Str);
                        
                        if (dataBytes[0] == 2 || dataBytes[0] == 4) //data format 2 or 4
                        {
                            if (dataBytes.Length == 5 || dataBytes.Length == 6)
                            {
                                ruuviData = new RuuvitagData(beaconData);
                                double humidity = dataBytes[1] * 0.5;
                                sbyte temperature = Convert.ToSByte(dataBytes[2]);
                                UInt16 pressureValue = BitConverter.ToUInt16(new byte[] { dataBytes[5], dataBytes[4] }, 0);
                                int pressure = pressureValue + 50000;

                                ruuviData.Humidity = humidity;
                                ruuviData.Pressure = pressure;
                                ruuviData.Temp = temperature;
                            }
                        }
                        else
                        {
                            Trace.WriteLine("[ruuvitagparser] data format not supported. Format: " + dataBytes[0].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("[ruuvitagparser] data: ");
                        foreach(byte b in data)
                        {
                            Trace.Write(string.Format("{0:X} ", b));
                        }
                        Trace.Write("\r\n");
                        Trace.WriteLine("[ruuvitagparser] EddisonUrlLength: " + eddisonUrlLength.ToString() + ", base64Str: " + base64Str);
                        Trace.WriteLine("[ruuvitagparser] Exception caught: " + ex.Message);
                    }
                }



            }
            return ruuviData;
        }
    }
}
