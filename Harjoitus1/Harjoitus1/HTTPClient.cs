using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace HTTPClient
{
    class HTTPClient
    {
        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            s.Connect("localhost", 25000);

            String viesti = "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n";
            byte[] buffer = Encoding.ASCII.GetBytes(viesti);
            s.Send(buffer);

            int n = 0;
            String tuloste;
            byte[] vastaanotettu = new byte[1024];
     /*       do
            {
                n = s.Receive(vastaanotettu);

                tuloste = Encoding.ASCII.GetString(vastaanotettu, 0, n);

            } while (n != 0 && !tuloste.Equals("\r\n") && tuloste.IndexOf("\r\n\r\n") != -1); */

            do
            {
                n = s.Receive(vastaanotettu);
                

                tuloste = Encoding.ASCII.GetString(vastaanotettu, 0, n);

                Console.Write(tuloste);
            } while (n != 0);

            Console.ReadKey();
            s.Close();

        }
    }
}
