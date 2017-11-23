using BleBeaconServer.DataClasses;
using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BleBeaconServer
{
    public class Program
    {
        public static UdpListener udpListener;
        public static BeaconPacketHandler packetHandler;

        //public static BleBeaconServerContext db;

        public static void Main(string[] args)
        {
            //using (BleBeaconServerContext db = new BleBeaconServerContext())
            //{

                /*Console.WriteLine("Beacons:");
                foreach (BleBeacon beacon in db.BleBeacons)
                {
                    Console.WriteLine("Mac: " + beacon.MacAddress);
                } */

                packetHandler = new BeaconPacketHandler();
                udpListener = new UdpListener(8473, 524288);

                UdpListener.BeaconPacketReceived += UdpListener_BeaconPacketReceived;

            while(true)
            {
                Thread.Sleep(1000);
                updateConsole(packetHandler.CopyOfDistances, packetHandler.CopyOfLocations);
            }
            //}
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
