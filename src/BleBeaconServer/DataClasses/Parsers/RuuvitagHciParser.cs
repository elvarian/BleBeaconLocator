using System;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses.Parsers
{
    public class RuuvitagHciParser
    {
        private List<Report> reports;
        public List<Report> Reports
        {
            get { return reports; }
            set { reports = value; }
        }

        private int indexInPacket = 0;
        private int indexInReport = 0;
        private int indexInAdData = 0;
        private int processedReports = 0;
        private RuuvitagData data = new RuuvitagData(null);
        private Report report;
        private AdvertisementData adData;
        
        public RuuvitagData ReadData(byte[] dataArray)
        {
            /*
            if (data != null)
            {
                for (int i = 0; i < dataArray.Length; i++, indexInPacket++)
                {
                    handleByte(dataArray[i]);
                }
            }

            if(data.PacketLength > 0 && indexInPacket >= data.PacketLength + 3)
            {
                return data;
            }
            */
            return null;
       
        }

        /*
        private void handleByte(byte b)
        {
            if (indexInPacket == 0)
            {
                data.PacketType = b & 0xFF;
            }
            else if (indexInPacket == 4)
            {
                data.NumberOfReports = b & 0xFF;
            }
            else if (indexInPacket > 13 && processedReports < data.NumberOfReports)
            {
                handleReport(b);
            }
        }

        private void handleReport(byte b)
        {
            if (indexInReport == 0)
            {
                report = new Report();
                report.Length = b & 0xFF;
                if (reports == null)
                {
                    reports = new List<Report>();
                }
                reports.Add(report);
            }
            else
            {
                handleAdvertisementData(b);
            }

            indexInReport++;
            if (indexInReport >= report.Length + 1)
            {
                indexInReport = 0;
                report = null;
                processedReports++;
            }
        }

        private void handleAdvertisementData(byte b)
        {
            if (indexInAdData == 0)
            {
                adData = new AdvertisementData();
                adData.Length = b & 0xFF;
                if (report.Advertisements == null)
                    report.Advertisements = new List<AdvertisementData>();
                report.Advertisements.Add(adData);
            }
            else if (indexInAdData == 1)
            {
                adData.Type = b & 0xFF;
            }
            else
            {
                if (adData.Data == null)
                    adData.Data = new List<byte>();
                adData.Data.Add(b);
            }
            indexInAdData++;
            if (indexInAdData >= adData.Length + 1)
            {
                indexInAdData = 0;
                adData = null;
            }
        }
        */
    }
}
