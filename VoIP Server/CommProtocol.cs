﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VoIP_Server
{


    class CommProtocol
    {
        public static string Read(NetworkStream stream)
        {
            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8, false, 1024, true))
            {
                string str = sr.ReadLine();
                return str;
            }
        }

        public static void Write(NetworkStream stream, string msg)
        {

            using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                try
                {
                    sw.WriteLine(msg);
                }
                catch (Exception e)
                { }
            }
        }
        public static string[] CheckMessage(string sData)
        {
            return sData.Split(' ');
        }
    }
}
