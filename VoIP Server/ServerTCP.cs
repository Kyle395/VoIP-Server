using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoIP_Server
{
    class ServerTCP
    {
        private TcpListener _server;
        private Boolean _isRunning;

        private List<string> loggedUsers = new List<string>();

        public ServerTCP(int port)
        {
            _server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            _server.Start();
            _isRunning = true;
        }

        public void LoopClients()
        {
            while (_isRunning)
            {
                // wait for client connection;
                TcpClient newClient = _server.AcceptTcpClient();
                //vs[0] = newClient;
                //vs[1] = rooms;
                // client found.
                // create a thread to handle communication

                Thread t = new Thread(unused =>
                {
                    try
                    {
                        HandleClient(newClient);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Client has disconnected due to error");
                        newClient.Close();
                    }
                });
                t.Start();
            }
        }

        public void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            bool clientConnected = true;

            while (clientConnected)
            {
                string userID;
                string sData = "";

                try
                {
                    sData = CommProtocol.Read(stream);
                }
                catch (Exception e)
                {
                    sData = "exit";
                }
                string[] logData = CommProtocol.CheckMessage(sData);


                if (sData == "exit")
                {
                    clientConnected = false;
                }
                else if (logData[0] == "user")
                {
                    userID = logData[1];
                    lock (loggedUsers)
                    {
                        loggedUsers.Add(userID);
                    }
                    CommProtocol.Write(stream, "ok");
                }
                else if (logData[0] == "ref")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(loggedUsers.Count);
                    foreach (var user in loggedUsers)
                    {
                        //sb.Append(room.Encode());
                    }
                    CommProtocol.Write(stream, sb.ToString());
                }
                else if (logData[0] == "call")
                {

                }
                else if (logData[0] == "hangup")
                {

                }
                else if (logData[0] == "pickup")
                {

                }
                else if (logData[0] == "reject")

                }
            }
        }
    }
}
