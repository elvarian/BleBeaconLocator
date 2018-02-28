﻿using BleBeaconServer.DataClasses;
using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BleBeaconServer
{
    public class Program
    {
        public static UdpListener udpListener;
        public static BeaconPacketHandler packetHandler;
        public static BeaconDataGrafanaFileWriter grafanaFileWriter;
        //public static BleBeaconServerContext db;

        static bool closing = false;

        public static void Main(string[] args)
        {
            //using (BleBeaconServerContext db = new BleBeaconServerContext())
            //{

            /*Console.WriteLine("Beacons:");
            foreach (BleBeacon beacon in db.BleBeacons)
            {
                Console.WriteLine("Mac: " + beacon.MacAddress);
            } */

            string filename = null;
            int port = 0;
            bool noConsole = false;

            for(int i = 0; i < args.Length; i++)
            {
                if(args[i].StartsWith("-"))
                {
                    if (args[i] == "-f")
                    {
                        if (i + 1 < args.Length)
                        {
                            filename = args[i + 1];
                        }
                    }
                    else if (args[i] == "-p")
                    {
                        if (i + 1 < args.Length)
                            int.TryParse(args[i + 1], out port);
                    }
                    else if (args[i] == "--no-console")
                        noConsole = true;
                }
            }

            if (filename != null && File.Exists(filename))
            {
                grafanaFileWriter = new BeaconDataGrafanaFileWriter(filename);
            }
            else if(filename != null)
            {
                Console.WriteLine("Path '" + filename + "' doesn't exists!");
                return;
            } else
            {
                Console.WriteLine("Please submit a filename");
            }
            
            packetHandler = new BeaconPacketHandler();

            BeaconPacketHandler.BeaconDataReceived += BeaconPacketHandler_BeaconDataReceived;

            udpListener = new UdpListener(port, 524288);

            UdpListener.BeaconPacketReceived += UdpListener_BeaconPacketReceived;

            Console.CancelKeyPress += Console_CancelKeyPress;
            while (!closing)
            {
                
                if(!noConsole)
                    updateConsole(packetHandler.CopyOfDistances, packetHandler.CopyOfLocations);

                if (closing)
                {
                    Debug.WriteLine("Closing down...");
                    
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                    Debug.WriteLine("Sleeping " + 1 + " seconds... ");
                    continue;
                }
            }
            //}
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (closing)
            {
                Console.WriteLine("Ok, ok! Shutting down ASAP.");
                e.Cancel = false;
            }
            else
            {
                Console.WriteLine("CTRL-C detected. Program will shut down as soon as gracefully possible.");
                Console.WriteLine("You can press again to terminate now (WARNING: you might lose event data).");
                closing = true;
                e.Cancel = true;
                udpListener.RequestStop();
                packetHandler.RequestStop();
                grafanaFileWriter.RequestStop();
            }
        }

        private static void BeaconPacketHandler_BeaconDataReceived(BeaconData data)
        {
            if(grafanaFileWriter != null)
                grafanaFileWriter.AddData(data);
        }

        private static void updateConsole(Dictionary<string, Dictionary<BleNode, double>> distances, Dictionary<string, Location> locations)
        {
            Console.Clear();
            foreach(string mac in distances.Keys.OrderBy(m => m))
            {
                foreach(BleNode node in distances[mac].Keys.OrderBy(n => n.Sender))
                {
                    Console.WriteLine(mac + " ".PadRight(5) + node.Sender + " ".PadRight(5) + Math.Round(distances[mac][node], 2));
                }
            }

            Console.WriteLine("\r\n\r\n");

            /*
            foreach(string mac in beaconMap.Keys.OrderBy(m => m))
            {
                foreach(BleNode node in beaconMap[mac].Keys.OrderBy(n => n.Sender))
                {
                    double distance = BeaconPacketHandler.CalculateDistance(-59, beaconMap[mac][node].Last);

                    Console.WriteLine(mac + " ".PadRight(5) + node.Sender + " ".PadRight(5) + Math.Round(distance, 2));
                }
            }

            Console.WriteLine("\r\n\r\n");
            */

            foreach (string mac in locations.Keys.OrderBy(m => m))
            {
                Console.WriteLine(mac + " ".PadRight(5) + "(" + Math.Round(locations[mac].X, 2) + ", " + Math.Round(locations[mac].Y, 2) + ")");
            }
            
        }

        private static void UdpListener_BeaconPacketReceived(BeaconPacket packet)
        {
            packetHandler.AddPacket(packet);
        }
    }
}
