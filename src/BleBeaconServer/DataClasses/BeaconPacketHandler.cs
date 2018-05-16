using BleBeaconDBLib;
using BleBeaconServer.DataClasses.Parsers;
using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading;
using static BleBeaconServer.DataClasses.BeaconData;

namespace BleBeaconServer.DataClasses
{
    public class BeaconPacketHandler
    {
        int txPower = -58; //hard coded
        double alfa = 0.75;

        public delegate void BeaconEvent(BeaconData data);
        public static event BeaconEvent BeaconDataReceived;

        Dictionary<string, Dictionary<BleNode, FilterContainer>> beaconMap = new Dictionary<string, Dictionary<BleNode, FilterContainer>>();

        Dictionary<string, Dictionary<BleNode, double>> distances = new Dictionary<string, Dictionary<BleNode, double>>();
        Dictionary<string, Location> locations = new Dictionary<string, Location>();

        Dictionary<BleBeacon, DateTime> lastLocations = new Dictionary<BleBeacon, DateTime>();

        List<BleNode> nodes = new List<BleNode>();
        List<BleBeacon> beacons = new List<BleBeacon>();

        Map map = null;

        Thread distanceCalcThd;
        Thread locationCalcThd;

        private bool shouldStop = false;

        public void RequestStop()
        {
            shouldStop = true;
        }

        public Dictionary<string, Dictionary<BleNode, double>> CopyOfDistances
        {
            get
            {
                Dictionary<string, Dictionary<BleNode, double>> distancesCopy = new Dictionary<string, Dictionary<BleNode, double>>();
                lock(distances)
                {
                    foreach(string mac in distances.Keys)
                    {
                        lock (distances[mac])
                        {
                            foreach (BleNode node in distances[mac].Keys)
                            {
                                Dictionary<BleNode, double> nodeToDistance = new Dictionary<BleNode, double>();
                                nodeToDistance.Add(node, distances[mac][node]);
                                if (distancesCopy.ContainsKey(mac))
                                    distancesCopy[mac][node] = distances[mac][node];
                                else
                                    distancesCopy.Add(mac, nodeToDistance);
                            }
                        }
                    }
                }
                return distancesCopy;
            }
        }

        public Dictionary<string, Location> CopyOfLocations
        {
            get
            {
                Dictionary<string, Location> locationsCopy = new Dictionary<string, Location>();
                lock(locations)
                {
                    foreach(string mac in locations.Keys)
                    {
                        locationsCopy.Add(mac, locations[mac]);
                    }
                }
                return locationsCopy;
            }
        }

       

        public BeaconPacketHandler()
        {
            using(BleBeaconServerContext db = new BleBeaconServerContext())
            {
                if(db.BleNodes != null)
                {
                    foreach(BleNode node in db.BleNodes)
                    {
                        nodes.Add(node);
                    }
                }
                if(db.BleBeacons != null)
                {
                    foreach(BleBeacon beacon in db.BleBeacons)
                    {
                        beacons.Add(beacon);
                    }
                }
                if(db.Maps != null)
                {
                    foreach(Map map in db.Maps)
                    {
                        this.map = map;
                        break;
                    }
                    if (this.map == null)
                    {
                        map = new Map();
                        map.Height = 0;
                        map.Width = 0;
                        db.Maps.Add(map);
                        db.SaveChanges();
                    }
                }
            }
            
            distanceCalcThd = new Thread(distanceCalculation);
            distanceCalcThd.Start();

            locationCalcThd = new Thread(locationCalculation);
            locationCalcThd.Start();
        }

        private void distanceCalculation()
        {
            while(!shouldStop)
            {
                lock (beaconMap)
                {
                    foreach (string mac in beaconMap.Keys)
                    {
                        foreach (BleNode node in beaconMap[mac].Keys)
                        {
                            double rssiFiltered = 0;
                            lock (beaconMap[mac][node])
                            {
                                rssiFiltered = beaconMap[mac][node].LastEstimate;
                            }

                            double distance = CalculateDistance(txPower, rssiFiltered);

                            using (BleBeaconServerContext db = new BleBeaconServerContext())
                            {
                                BleBeacon beacon = beacons.Find(b => b.MacAddress == mac);
                                if (beacon != null)
                                {
                                    bool foundDistance = false;
                                    foreach (BleDistance bleDistance in db.Distances)
                                    {
                                        if (bleDistance.BleBeaconsId == beacon.BleBeaconsId && bleDistance.BleNodesId == node.BleNodesId)
                                        {
                                            bleDistance.Distance = distance;
                                            foundDistance = true;
                                            break;
                                        }
                                    }

                                    if (!foundDistance)
                                    {
                                        BleDistance bleDistance = new BleDistance();
                                        bleDistance.BleBeaconsId = beacon.BleBeaconsId;
                                        bleDistance.BleNodesId = node.BleNodesId;
                                        bleDistance.Distance = distance;
                                        db.Distances.Add(bleDistance);
                                    }
                                    
                                    db.SaveChanges();
                                }
                            }

                            if (!distances.ContainsKey(mac))
                            {
                                lock (distances)
                                    distances.Add(mac, new Dictionary<BleNode, double>());
                            }

                            lock (distances[mac])
                            {
                                if (!distances[mac].ContainsKey(node))
                                    distances[mac].Add(node, distance);
                                else
                                    distances[mac][node] = distance;
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public static double CalculateDistance(float txPower, double rssi)
        {
            double distance = Math.Pow(10, ((rssi-txPower) / (-10*2)));
            return distance;
        }

        private void locationCalculation()
        {
            while(!shouldStop)
            {
                Dictionary<string, Dictionary<BleNode, double>> copyOfDistances = CopyOfDistances;
                foreach (string mac in copyOfDistances.Keys)
                {
                    List<BleNode> nodeList = new List<BleNode>();
                    List<double> distList = new List<double>();

                    /*
                    PointF a = new PointF();
                    PointF b = new PointF();
                    PointF c = new PointF();

                    float dA = 0;
                    float dB = 0;
                    float dC = 0;
                    */

                    lock (distances[mac])
                    {
                        int i = 0;
                        foreach (BleNode node in distances[mac].Keys)
                        {
                            if (i++ > 2)
                                break;

                            nodeList.Add(node);
                            distList.Add(distances[mac][node]);
                        }
                    }

                    if (nodeList.Count == 3 && distList.Count == 3)
                    {
                        /*
                        a = new PointF((float)nodeList[0].X, (float)nodeList[0].Y);
                        b = new PointF((float)nodeList[1].X, (float)nodeList[1].Y);
                        c = new PointF((float)nodeList[2].X, (float)nodeList[2].Y);

                        dA = (float)distList[0];
                        dB = (float)distList[1];
                        dC = (float)distList[2];
                        */

                        //Vector2 vector = GetLocation(nodeList, distList);
                        Vector2 vector = trilaterate2DLinear(nodeList, distList);
                        //PointF point = GetLocationWithCenterOfGravity(a, b, c, dA, dB, dC);
                        
                        Location location = new Location();
                        location.X = vector.X;
                        location.Y = vector.Y;

                        //location.X = point.X;
                        //location.Y = point.Y;
                        location.Date = DateTime.Now;

                        BleBeacon beacon = beacons.Find(bea => bea.MacAddress == mac);

                        if (beacon != null)
                        {
                            location.BleBeaconsId = beacon.BleBeaconsId;

                            /*
                            using (BleBeaconServerContext db = new BleBeaconServerContext())
                            {
                                foreach (BleNode node in nodeList)
                                {
                                    bool foundDistance = false;
                                    foreach (BleDistance distance in db.Distances)
                                    {
                                        if (distance.BleBeaconsId == beacon.BleBeaconsId && distance.BleNodesId == node.BleNodesId)
                                        {
                                            distance.Distance = distList[nodeList.IndexOf(node)];
                                            foundDistance = true;
                                            break;
                                        }
                                    }

                                    if (!foundDistance)
                                    {
                                        BleDistance distance = new BleDistance();
                                        distance.BleBeaconsId = beacon.BleBeaconsId;
                                        distance.BleNodesId = node.BleNodesId;
                                        distance.Distance = distList[nodeList.IndexOf(node)];
                                        db.Distances.Add(distance);
                                    }
                                }

                                db.SaveChanges();
                            }*/
                            
                            using (BleBeaconServerContext db = new BleBeaconServerContext())
                            {
                                bool lastLocationFound = false;
                                foreach (BleLastLocation bleLastLocation in db.LastLocations)
                                {
                                    if (bleLastLocation.BleBeaconsId == beacon.BleBeaconsId)
                                    {
                                        bleLastLocation.Date = location.Date;
                                        bleLastLocation.X = location.X;
                                        bleLastLocation.Y = location.Y;
                                        lastLocationFound = true;
                                        break;
                                    }
                                }

                                if (!lastLocationFound)
                                {
                                    BleLastLocation lastLocation = new BleLastLocation();
                                    lastLocation.BleBeaconsId = beacon.BleBeaconsId;
                                    lastLocation.Date = location.Date;
                                    lastLocation.X = location.X;
                                    lastLocation.Y = location.Y;
                                    db.LastLocations.Add(lastLocation);
                                }
                                //Saving location history data to database
                                if (!lastLocations.ContainsKey(beacon) || lastLocations[beacon] <= DateTime.Now.AddHours(-1))
                                {
                                    if (!lastLocations.ContainsKey(beacon))
                                        lastLocations.Add(beacon, location.Date);
                                    else
                                        lastLocations[beacon] = location.Date;

                                    db.Locations.Add(location);
                                }
                                db.SaveChanges();
                            }
                        
                            
                            lock (locations)
                            {
                                if (locations.ContainsKey(mac))
                                {
                                    locations[mac] = location;
                                }
                                else
                                    locations.Add(mac, location);
                            }
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public void AddPacket(BeaconPacket packet)
        {
            
            BleNode node = nodes.Find(n => n.Sender == packet.Sender);

            if (node == null)
            {
                node = new BleNode();
                node.Sender = packet.Sender;
                node.MapsId = map.MapsId;
                nodes.Add(node);

                using (BleBeaconServerContext db = new BleBeaconServerContext())
                {
                    db.BleNodes.Add(node);
                    db.SaveChanges();
                }
            }
            
            Types type = BeaconData.GetType(packet.ByteData);
            
            BeaconData beaconData = BeaconData.ParseValues(packet.ByteData);
            if (beaconData != null && type != Types.Unknown)
            {
                if (node != null)
                    beaconData.Node = node;
                if (beaconData.Mac != null && beaconData.Mac != "")
                {
                    BleBeacon beacon = beacons.Find(n => n.MacAddress == beaconData.Mac);
                    if (beacon == null)
                    {
                        beacon = new BleBeacon();
                        beacon.MacAddress = beaconData.Mac;
                        beacons.Add(beacon);

                        using (BleBeaconServerContext db = new BleBeaconServerContext())
                        {
                            db.BleBeacons.Add(beacon);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        beaconData.Beacon = beacon;
                    }
                }
            }

            if (type == Types.Ruuvitag)
            {
                RuuvitagParser parser = new RuuvitagParser(beaconData);
                RuuvitagData ruuviData = parser.ReadData(packet.ByteData);

                if (ruuviData != null)
                {
                    if (BeaconDataReceived != null)
                        BeaconDataReceived(ruuviData);

                    AddToNodeMap(node, ruuviData);
                }
            } else if(type == Types.APlant)
            {
                APlantParser parser = new APlantParser(beaconData);
                APlantData aplantData = parser.ReadData(packet.ByteData);

                if(aplantData != null)
                {
                    if (BeaconDataReceived != null)
                        BeaconDataReceived(aplantData);

                    AddToNodeMap(node, aplantData);
                }
            } else if(type == Types.PebbleBee)
            {
                PebblebeeParser parser = new PebblebeeParser(beaconData);
                PebblebeeData pebbleData = parser.ReadData(packet.ByteData);

                if(pebbleData != null)
                {
                    if (BeaconDataReceived != null)
                        BeaconDataReceived(pebbleData);

                    AddToNodeMap(node, pebbleData);
                }
            }

            /*if(packet.mac_address != null && packet.mac_address != "")
            {
                BleBeacon beacon = beacons.Find(n => n.MacAddress == packet.mac_address);
                if(beacon == null)
                {
                    beacon = new BleBeacon();
                    beacon.MacAddress = packet.mac_address;
                    beacons.Add(beacon);

                    using (BleBeaconServerContext db = new BleBeaconServerContext())
                    {
                        db.BleBeacons.Add(beacon);
                        db.SaveChanges();
                    }
                }
                //Dictionary<BleNode, RollingList<int>> nodeMap;
                Dictionary<BleNode, FilterContainer> nodeMap;
                if (beaconMap.ContainsKey(packet.mac_address))
                {
                    nodeMap = beaconMap[packet.mac_address];
                } else
                {
                    //nodeMap = new Dictionary<BleNode, RollingList<int>>();
                    lock (beaconMap)
                    {
                        nodeMap = new Dictionary<BleNode, FilterContainer>();
                        beaconMap.Add(packet.mac_address, nodeMap);
                    }
                }

                if(nodeMap != null)
                {
                    if(nodeMap.ContainsKey(node))
                    {
                        lock (nodeMap[node])
                        {
                            //nodeMap[node].Add(packet.rssi);

                            nodeMap[node].Filter.Update(new[] { (double)packet.rssi });
                            nodeMap[node].LastEstimate = nodeMap[node].Filter.getState()[0];
                        }
                    } else
                    {
                        //RollingList<int> list = new RollingList<int>(100);
                        FilterContainer container = new FilterContainer();
                        container.Filter = new UKF();
                        //UKF filter = new UKF();
                        //list.Add(packet.rssi);
                        lock (container)
                        {
                            container.Filter.Update(new[] { (double)packet.rssi });
                            container.LastEstimate = container.Filter.getState()[0];
                        }
                        //nodeMap.Add(node, list);
                        lock (nodeMap)
                        {
                            nodeMap.Add(node, container);
                        }
                    }
                }*/

            //}

        }

        private void AddToNodeMap(BleNode node, BeaconData data)
        {
            string mac = data.Mac;
            double rssi = data.Rssi;
            
            //Dictionary<BleNode, RollingList<int>> nodeMap;
            Dictionary<BleNode, FilterContainer> nodeMap;
            if (mac != null && rssi != 0)
            {
                if (beaconMap.ContainsKey(mac))
                {
                    nodeMap = beaconMap[mac];
                }
                else
                {
                    //nodeMap = new Dictionary<BleNode, RollingList<int>>();
                    lock (beaconMap)
                    {
                        nodeMap = new Dictionary<BleNode, FilterContainer>();
                        beaconMap.Add(mac, nodeMap);
                    }
                }

                if (nodeMap != null)
                {
                    if (nodeMap.ContainsKey(node))
                    {
                        lock (nodeMap[node])
                        {
                            //nodeMap[node].Add(packet.rssi);

                            nodeMap[node].Filter.Update(new[] { (double)rssi });
                            nodeMap[node].LastEstimate = nodeMap[node].Filter.getState()[0];
                        }
                    }
                    else
                    {
                        //RollingList<int> list = new RollingList<int>(100);
                        FilterContainer container = new FilterContainer();
                        container.Filter = new UKF();
                        //UKF filter = new UKF();
                        //list.Add(packet.rssi);
                        lock (container)
                        {
                            container.Filter.Update(new[] { (double)rssi });
                            container.LastEstimate = container.Filter.getState()[0];
                        }
                        //nodeMap.Add(node, list);
                        lock (nodeMap)
                        {
                            nodeMap.Add(node, container);
                        }
                    }
                }
            }
        }

        public static Vector2 GetLocation(List<BleNode> nodes, List<double> distances)
        {
            if (nodes.Count == 3 && distances.Count == 3)
            {
                Vector2 a = new Vector2((float)nodes[0].X, (float)nodes[0].Y);
                Vector2 b = new Vector2((float)nodes[1].X, (float)nodes[1].Y);
                Vector2 c = new Vector2((float)nodes[2].X, (float)nodes[2].Y);

                double normA = Math.Sqrt(Math.Pow((double)a.X, 2) + Math.Pow((double)a.Y, 2));

                Vector2 subtracAfB = b - a;

                Vector2 ex = new Vector2((float)(subtracAfB.X / normA), (float) (subtracAfB.Y / normA));

                Vector2 subtractCfA = c - a;

                float i = ex.X * subtractCfA.X + ex.Y * subtractCfA.Y;

                Vector2 p = (subtractCfA - new Vector2(ex.X * i, ex.Y * i));
                
                double normP = Math.Sqrt(Math.Pow((double)p.X, 2) + Math.Pow((double)p.Y, 2));

                Vector2 ey = new Vector2((float)(p.X / normP), (float)(p.Y / normP));

                double d = Math.Sqrt(Math.Pow((double)subtracAfB.X, 2) + Math.Pow((double)subtracAfB.Y, 2));

                double j = ey.X * subtractCfA.X + ey.Y * subtractCfA.Y;

                double x = Math.Pow(distances[0], 2) - Math.Pow(distances[1], 2) + Math.Pow(d, 2) / (2 * d);
                double y = (Math.Pow(distances[0], 2) - Math.Pow(distances[2], 2) + Math.Pow(i, 2) + Math.Pow(j, 2)) / (2 * j) - (i / j) * x;

                return new Vector2((float)x, (float)y);
            }
            return new Vector2();
        }

        private static double getDistance(BleNode node1, BleNode node2)
        {
            return Math.Sqrt(Math.Pow((node2.X - node1.X), 2) + Math.Pow((node2.Y - node1.Y), 2));
        }

        public static Vector2 trilaterate2DLinear(List<BleNode> nodes, List<double> distances)
        {
            if (nodes.Count == 3 && distances.Count == 3)
            {

                MathNet.Numerics.LinearAlgebra.Vector<double> vA = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new double[] { nodes[0].X, nodes[0].Y });
                
                double[] b = { 0, 0 };
                b[0] = 0.5 * (Math.Pow(distances[0], 2) - Math.Pow(distances[1], 2) + Math.Pow(getDistance(nodes[1], nodes[0]), 2));
                b[1] = 0.5 * (Math.Pow(distances[0], 2) - Math.Pow(distances[2], 2) + Math.Pow(getDistance(nodes[2], nodes[0]), 2));

                MathNet.Numerics.LinearAlgebra.Vector<double> vb = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(b);

                double[,] A = { { nodes[1].X - nodes[0].X, nodes[1].Y - nodes[0].Y }, { nodes[2].X - nodes[0].X, nodes[2].Y - nodes[0].Y } };

                MathNet.Numerics.LinearAlgebra.Matrix<double> mA = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseOfArray(A);

                MathNet.Numerics.LinearAlgebra.Matrix<double> mAT = mA.Transpose();

                MathNet.Numerics.LinearAlgebra.Vector<double> x = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(2);

                double det = mA.Multiply(mAT).Determinant();
                if (det > 0.1)
                {
                    x = (mA.Transpose() * mA).Inverse() * (mA.Transpose() * vb);
                }
                else
                {
                    x = (((mA.Multiply(mAT)).Inverse()).Multiply(mAT)).Multiply(vb);
                }
                
                x.Add(vA);

                double[] coordinates = x.ToArray();

                Vector2 vector = new Vector2((float)coordinates[0], (float)coordinates[1]);
                return vector;
            }

            return new Vector2();
        }
    }
}
