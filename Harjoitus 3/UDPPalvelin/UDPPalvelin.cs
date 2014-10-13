using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace UDPPalvelin
{
    class UDPPalvelin
    {
        static void Main(string[] args)
        {
            Socket s = null;
            int port = 25000;
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, port);
            

            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Bind(iep);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Virhe: " + ex.Message);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Odotetaan asiakasta.");
            List<EndPoint> asiakkaat = new List<EndPoint>();
            byte[] rec = new byte[256];
            IPEndPoint asiakas = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)asiakas;
            String rec_string;
            String[] palat;
            char[] delim = { ';' };

            while (!Console.KeyAvailable)
            {
                int received = s.ReceiveFrom(rec, ref remote);
                rec_string = Encoding.ASCII.GetString(rec,0,received);
                palat = rec_string.Split(delim,2);
                if (palat.Length < 2) { }
                else
                {
                    if (!asiakkaat.Contains(remote))
                    {
                        asiakkaat.Add(remote);
                        Console.WriteLine("Uusi asiakas: {0}:{1}", ((IPEndPoint)remote).Address, ((IPEndPoint)remote).Port);
                    }
                    Console.WriteLine("{0}: {1}", palat[0], palat[1]);
                    foreach (EndPoint ep in asiakkaat)
                    {
                        s.SendTo(Encoding.ASCII.GetBytes(rec_string), ep);
                    }
                }

            }

            Console.ReadKey();
            s.Close();
        }
    }
}
