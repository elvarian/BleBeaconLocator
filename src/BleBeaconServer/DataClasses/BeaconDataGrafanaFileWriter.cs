using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace BleBeaconServer.DataClasses
{
    public class BeaconDataGrafanaFileWriter
    {
        private string filename = null;

        Dictionary<string, BeaconData> beaconDatas = new Dictionary<string, BeaconData>();

        Thread writerThd;

        private bool shouldStop = false;
        public void RequestStop()
        {
            shouldStop = true;
        }

        public BeaconDataGrafanaFileWriter(string file)
        {
            this.filename = file;

            writerThd = new Thread(writeFile);
            writerThd.Start();
        }

        public void writeFile()
        {
            while (!shouldStop)
            {
                Dictionary<string, BeaconData> beaconDataCopy = new Dictionary<string, BeaconData>();
                lock (beaconDatas)
                {
                    foreach (string mac in beaconDatas.Keys)
                    {
                        beaconDataCopy.Add(mac, beaconDatas[mac]);
                    }
                }

                string tempFile = filename + ".temp";


                try
                {
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(tempFile, false))
                    {
                        foreach (BeaconData data in beaconDataCopy.Values)
                        {
                            if (data.Date.AddMinutes(30) < DateTime.Now)
                            {
                                lock (beaconDatas)
                                {
                                    beaconDatas.Remove(data.Mac);
                                }
                                continue;
                            }
                            else
                            {
                                string name = data.Mac;
                                if (data.Beacon != null && data.Beacon.Name != null && data.Beacon.Name != "")
                                    name = data.Beacon.Name;

                                string node = "Unknown sender";
                                if (data.Node != null && data.Node.Sender != null && data.Node.Sender != "")
                                    node = data.Node.Sender;
                                
                                if (data is RuuvitagData)
                                {
                                    RuuvitagData ruuviData = (RuuvitagData)data;

                                    int pressure = (int)ruuviData.Pressure / 100;
                                    file.WriteLine("tags{mac=\"" + ruuviData.Mac + "\",beaconname=\"" + name + "\",node=\"" + node + "\",type=\"ruuvitag\",valuetype=\"temp\"} " + ruuviData.Temp.ToString("D"));
                                    file.WriteLine("tags{mac=\"" + ruuviData.Mac + "\",beaconname=\"" + name + "\",node=\"" + node + "\",type=\"ruuvitag\",valuetype=\"humidity\"} " + ruuviData.Humidity.ToString("0.00", CultureInfo.CreateSpecificCulture("en-US")));
                                    file.WriteLine("tags{mac=\"" + ruuviData.Mac + "\",beaconname=\"" + name + "\",node=\"" + node + "\",type=\"ruuvitag\",valuetype=\"pressure\"} " + pressure.ToString("D"));
                                }
                                else if (data is APlantData)
                                {
                                    APlantData aplantData = (APlantData)data;

                                    file.WriteLine("tags{mac=\"" + data.Mac + "\",beaconname=\"" + name + "\",node=\"" + node + "\",type=\"aplant\",valuetype=\"temp\"} " + aplantData.Temp.ToString("D"));
                                    file.WriteLine("tags{mac=\"" + data.Mac + "\",beaconname=\"" + name + "\",node=\"" + node + "\",type=\"aplant\",valuetype=\"soilmoisture\"} " + aplantData.SoilMoisture.ToString("D"));
                                }
                            }
                        }
                    }


                    File.Copy(tempFile, filename, true);
                    File.Delete(tempFile);

                } catch(Exception ex)
                {
                    Trace.WriteLine("[beacondatagrafanafilewriter] Exception caugth: " + ex.Message);
                }
                for(int i = 0; i < 15; i++)
                {
                    if (!shouldStop)
                        Thread.Sleep(1000);
                    else break;
                }
                
            }
        }

        public void AddData(BeaconData data)
        {
            lock (beaconDatas)
            {
                if (beaconDatas.ContainsKey(data.Mac))
                {
                    beaconDatas[data.Mac] = data;
                }
                else
                {
                    beaconDatas.Add(data.Mac, data);
                }
            }
        }
    }
}
