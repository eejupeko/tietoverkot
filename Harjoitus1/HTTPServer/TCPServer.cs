using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TCPServer
{
    class TCPServer
    {
        static void Main(string[] args)
        {
            Socket palvelin = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Loopback, 12345);
            palvelin.Bind(ipep);
            palvelin.Listen(0);

            Socket asiakas = palvelin.Accept();

            IPEndPoint ipepa = (IPEndPoint)asiakas.RemoteEndPoint;

            Console.WriteLine("Yhteys osoitteesta {0} portista {1}", ipepa.Address, ipepa.Port);

            NetworkStream ns = new NetworkStream(asiakas);
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            String str = sr.ReadLine();
            sw.WriteLine("Eetun palvelin;" + str);
            sw.Flush();

            sw.Close();
            sr.Close();
            ns.Close();
            palvelin.Close();
            asiakas.Close();
            Console.WriteLine("Palvelin suljettu. Paina jotain nappia.");
            Console.ReadKey();
        }
    }
}
