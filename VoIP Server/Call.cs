using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace VoIP_Server
{
    class Client
    {
        public Queue buffer = new Queue();
    }
    class Call
    {
        public int id;
        public string login;
        UdpClient udpServer;
        Thread listenerThread;
        Thread senderThread;
        public Dictionary<SocketAddress, Client> users = new Dictionary<SocketAddress, Client>();
        public Dictionary<SocketAddress, string> usernames = new Dictionary<SocketAddress, string>();
        public SocketAddress[] socketAddresses = new SocketAddress[2];
        Client Client;
        public Call(int id, SocketAddress client1, SocketAddress client2)
        {
            socketAddresses[0] = client1;
            socketAddresses[1] = client2;
            this.id = id;
            udpServer = new UdpClient(8100 + id);

            listenerThread = new Thread(unused =>
                {
                    ReceiveUDP();
                });
            listenerThread.Start();

            senderThread = new Thread(unused =>
            {
                SendAudioBack();
            });
            senderThread.Start();
        }
        public void ReceiveUDP()
        {
            while (true)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    var data = udpServer.Receive(ref remoteEP);
                    //Console.WriteLine(remoteEP.ToString() + ": " + data.Length);
                    SocketAddress sa = remoteEP.Serialize();
                    //Console.WriteLine(remoteEP.Port);

                    var data2 = CommProtocol.DecryptUDP(data, CommProtocol.clientKeysUDP[sa].key, CommProtocol.clientKeysUDP[sa].iv);

                    lock (this)
                    {
                        if (!users.ContainsKey(sa))
                        {
                            Client client = new Client();
                            users.Add(sa, client);
                        }
                        else
                        {
                            users[sa].buffer.Enqueue(data2);
                        }
                    }

                }
                catch (Exception e)
                {

                }
            }
        }
        public void SendAudioBack()
        {
            while (true)
            {
                lock (this)
                {
                    var keys = users.Keys;
                    foreach (var key in keys)
                    {
                        SocketAddress rs = null;
                        foreach (var socket in socketAddresses) 
                        {
                            if (key != socket)
                            {
                                rs = socket;
                            }
                        }
                        byte[] data = new byte[320];
                        data= (byte[])users[key].buffer.Dequeue();
                        IPEndPoint ep = new IPEndPoint(0, 0);
                        ep = (IPEndPoint)ep.Create(rs);
                        var data2 = CommProtocol.EncryptUDP(data, CommProtocol.clientKeysUDP[key].key, CommProtocol.clientKeysUDP[key].iv);
                        udpServer.Send(data2, data2.Length, ep);
                    }
                }
            }
        }
    }
}