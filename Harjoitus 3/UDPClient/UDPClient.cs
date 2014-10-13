using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace UDPClient
{
    class UDPClient
    {
        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            int port = 25000;

            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, port);
            byte[] rec = new byte[256];

            EndPoint ep = (EndPoint)iep;
            s.ReceiveTimeout = 1000;
            String msg;
            Boolean on = true;

            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint palvelinep = (EndPoint)remote;

            String rec_string;
            String[] palat;
            char[] delim = { ';' };
            do
            {
                Console.WriteLine(">");
                msg = Console.ReadLine();
                if (msg.Equals(""))
                {
                    on = false;
                }
                else
                {
                    s.SendTo(Encoding.ASCII.GetBytes(msg),ep);
                    while (!Console.KeyAvailable)
                    {
                       
                        int paljon = 0;
                        try
                        {
                            paljon = s.ReceiveFrom(rec, ref palvelinep);
                            rec_string = Encoding.ASCII.GetString(rec, 0, paljon);
                            palat = rec_string.Split(delim, 2);
                            Console.WriteLine("{0}: {1}", palat[0], palat[1]);
                        }
                        catch { /* timeout */ }
                    }
                }
            } while (on);
            Console.ReadKey();
            s.Close();
        }
    }
}
