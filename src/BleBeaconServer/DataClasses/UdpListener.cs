﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BleBeaconServer.DataClasses
{
    public class UdpListener
    {
        public delegate void BeaconEvent(BeaconPacket packet);
        public static event BeaconEvent BeaconPacketReceived;

        static UdpClient udp;
        static IPEndPoint udpEp;

        Thread dequeuePacketsThd;

        int udpReceiveBufferSize = 524288;

        private bool shouldStop = false;

        public void RequestStop()
        {
            shouldStop = true;
        }

        Queue<UdpReceiveResult> receivedUdpPackets;
        //Queue<byte[]> receivedUdpPackets;

        public UdpListener(int port, int udpReceiveBufferSize)
        {
            receivedUdpPackets = new Queue<UdpReceiveResult>(1000);

            UDPListener(port);
            
            dequeuePacketsThd = new Thread(dequeuePackets);
            dequeuePacketsThd.Start();

            //ss.ReceiveReady += onReceiveReady;

            //this.poller = new NetMQPoller();
            //this.poller.Add(this.ss);
            //this.poller.RunAsync();
        }

        private void dequeuePackets()
        {
            UdpReceiveResult result;
            int count = 0;
            while (!shouldStop)
            {
                try
                {
                    //byte[] data = ss.ReceiveFrameBytes();

                    lock (receivedUdpPackets)
                    {
                        count = receivedUdpPackets.Count;
                        if (count > 0)
                            result = receivedUdpPackets.Dequeue();
                    }

                    if (count == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    else
                    {
                        if (result != null && result.Buffer.Length > 0)
                        {
                            string data = System.Text.Encoding.UTF8.GetString(result.Buffer);

                            //byte[] data = (byte[])result.Buffer.Clone();

                            //Console.WriteLine(data);

                            if (result.Buffer.Length >= 2)
                            {
                                /*
                                int index = 0;
                                BeaconPacket packet = new BeaconPacket();
                                packet.SenderLength = BitConverter.ToUInt16(result.Buffer, 0);
                                index += 2;

                                if(result.Buffer.Length < 2 + index)
                                {
                                    Console.WriteLine("[udplistener] Error at parsing packet: Buffer length too short!");
                                }

                                packet.Sender = System.Text.Encoding.UTF8.GetString(result.Buffer, index, packet.SenderLength);
                                index += packet.SenderLength;
                                if(packet.Sender == null || packet.Sender == "" || packet.Sender.Length != 17)
                                {
                                    Console.WriteLine("[udplistener] Error at parsing packet: Sender not valid!");
                                    return;
                                }

                                packet.DataLength = BitConverter.ToUInt16(result.Buffer, index);
                                index += 2;

                                packet.Data = new byte[packet.DataLength];


                                if(packet.Data.Length >= result.Buffer.Length - index)
                                {
                                    for(int i = 0; i < packet.DataLength; i++)
                                    {
                                        packet.Data[i] = result.Buffer[index++];
                                    }
                                }
                                */
                                    //result.Buffer. CopyTo(packet.Data, index);

                                BeaconPacket packet = JsonConvert.DeserializeObject<BeaconPacket>(data);
                                
                                if (BeaconPacketReceived != null)
                                    BeaconPacketReceived(packet);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Program.DebugWriter.WriteLine("[nflistener] Catched exception in loop dequeuePackets " + ex.Message + "\r\n\r\n" + ex.StackTrace);
                    Console.WriteLine("[udplistener] Catched exception in loop dequeuePackets " + ex.Message + "\r\n\r\n" + ex.StackTrace);
                }
            }
        }

        private void UDPListener(int port)
        {
            udpEp = new IPEndPoint(IPAddress.Any, port);
            Task.Run(async () =>
            {
                using (var udpClient = new UdpClient(udpEp))
                {
                    udpClient.Client.ReceiveBufferSize = udpReceiveBufferSize;
                    //string loggingEvent = "";
                    while (true)
                    {
                        UdpReceiveResult receivedResults = await udpClient.ReceiveAsync();

                        //loggingEvent += Encoding.ASCII.GetString(receivedResults.Buffer);
                        //Console.Write(Encoding.ASCII.GetString(receivedResults.Buffer));

                        lock (receivedUdpPackets)
                        {
                            receivedUdpPackets.Enqueue(receivedResults);
                        }
                    }
                }
            });
        }


    }
}
