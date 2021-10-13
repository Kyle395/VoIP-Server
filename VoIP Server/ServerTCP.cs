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

            LoopClients();
        }

        public void LoopClients()
        {
            while (_isRunning)
            {
                TcpClient newClient = _server.AcceptTcpClient();

                Thread t = new Thread(unused =>
                {
                    try
                    {
                        HandleClient(newClient);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Client has disconnected due to error");
                    }
                });
                t.Start();
            }
        }

        static string Hash(string password)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            CommProtocol.setAes(stream);

            bool clientConnected = true;
            DatabaseConn dc = new DatabaseConn();
            bool logged = false;
            string playerID = "";


            while (clientConnected)
            {
                do
                {
                    string sData = CommProtocol.Read(stream);
                    Console.WriteLine(sData);
                    string[] logData = CommProtocol.CheckMessage(sData);
                    if (logData[0] == "log")
                    {
                        if (!loggedUsers.Contains(logData[1]))
                        {
                            if (dc.checkUserData(logData[1], Hash(logData[1] + logData[2])))
                            {

                                Console.WriteLine("user logged");
                                CommProtocol.Write(stream, "log ok");
                                logged = true;
                                lock (loggedUsers)
                                {
                                    loggedUsers.Add(playerID);
                                }
                            }
                            else
                            {
                                CommProtocol.Write(stream, "error wrong_credentials");
                                Console.WriteLine("wrong login data");
                            }
                        }
                        else CommProtocol.Write(stream, "error already_logged_in");
                    }
                    else if (logData[0] == "reg")
                    {
                        if (dc.registerUser(logData[1], Hash(logData[1] + logData[2])))
                        {
                            CommProtocol.Write(stream, "reg ok");
                        }
                        else CommProtocol.Write(stream, "error login_already_used");
                    }
                    else Console.WriteLine("wrong command");
                } while (!logged);

                while (logged)
                {
                    string sData = "";
                    try
                    {
                        sData = CommProtocol.Read(stream);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Logging out player " + playerID + " due to error");
                        sData = "logout";
                    }
                    Console.WriteLine(sData);
                    string[] logData = CommProtocol.CheckMessage(sData);

                    if (sData == "logout")
                    {
                        logged = false;
                        clientConnected = false;
                        lock (loggedUsers)
                        {
                            loggedUsers.Remove(playerID);
                        }
                    }
                    else if (logData[0] == "ref")
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("ref ");
                        CommProtocol.Write(stream, sb.ToString());
                    }
                    else if (logData[0] == "chngpass")
                    {
                        dc.editUserPassword(logData[1], Hash(logData[1] + logData[2]));
                        CommProtocol.Write(stream, "ok");
                    }
                    else if (logData[0] == "delacc")
                    {
                        dc.deleteUser(logData[1], Hash(logData[1] + logData[2]));
                        CommProtocol.Write(stream, "ok");
                    }
                    else if (logData[0] == "call")
                    {

                    }
                }
            }
        }
    }
}