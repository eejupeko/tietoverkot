using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace PeliAsiakas
{
    class PeliAsiakas
    {
        /// <summary>
        /// Lähettää palvelimelle viestin
        /// </summary>
        /// <param name="s">Palvelimen socket</param>
        /// <param name="ep">Palvelimen end point</param>
        /// <param name="str">Lähetettävä viesti</param>
        static void Laheta(Socket s, EndPoint ep, String str)
        {
            s.SendTo(Encoding.ASCII.GetBytes(str), ep);
        }

        /// <summary>
        /// Otetaan vastaan palvelimelta viesti
        /// </summary>
        /// <param name="s">Palvelimen socket</param>
        /// <returns>Vastaanotettu viesti taulukossa välilyönneistä jaettuna</returns>
        static string[] Vastaanota(Socket s)
        {
            byte[] rec = new byte[256];
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint palvelinep = (EndPoint)remote;
            int koko = s.ReceiveFrom(rec, ref palvelinep);
            return Encoding.ASCII.GetString(rec,0,koko).Split(' ');
        }
        static void Main(string[] args)
        {
            Socket palvelin = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            int port = 25100;

            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, port);
            

            EndPoint ep = (EndPoint)iep;
            Boolean on = true;

            Console.WriteLine("Anna nimi");
            String nimi = Console.ReadLine();
            Laheta(palvelin, ep, "JOIN "+nimi);
            String luku; 

            String tila = "JOIN";

            String[] status;
            do
            {
                status = Vastaanota(palvelin);
                if (status.Length < 2) { Console.WriteLine("Virheellinen viesti palvelimelta"); break; }
                switch (tila)
                {
                    case "JOIN":
                        switch (status[0])
                        {
                            case "ACK":
                                switch (status[1])
                                {
                                    case "201":
                                        Console.WriteLine("Odotetaan toista pelaajaa.");
                                        break;
                                    case "202":
                                        Console.WriteLine("Vastustajasi on {0}. Sinä aloitat.", status[2]);
                                        Console.WriteLine("Anna numero");
                                        luku = Console.ReadLine();
                                        Laheta(palvelin, ep, "DATA " + luku);
                                        tila = "GAME";
                                        break;
                                    case "203":
                                        Console.WriteLine("Vastustaja {0} saa aloittaa", status[2]);
                                        tila = "GAME";
                                        break;
                                    case "401":
                                        Console.WriteLine("Peliin liittyminen epäonnistui");
                                        on = false;
                                        break;
                                    default:
                                        Console.WriteLine("Virhe " + status[1]);
                                        break;
                                } // ACK switch status[1]
                                break;
                            default:
                                Console.WriteLine("Virhe " + status[0]);
                                break;
                        } // JOIN switch status[0]
                        break;
                    case "GAME":
                        switch (status[0]) {
                            case "ACK":
                                switch (status[1]) {
                                    case "300":
                                        Console.WriteLine("Vastaus oli väärin. Vastustajan vuoro arvata.");
                                        break;
                                    case "400":
                                        Console.WriteLine("Määrittelemätön virhe.");
                                        on = false;
                                        break;
                                    case "403":
                                        Console.WriteLine("Virhe kommunikaatiossa.");
                                        on = false;
                                        break;
                                    case "407":
                                        Console.WriteLine("Virheellinen arvaus. Arvaa uusi numero");
                                        luku = Console.ReadLine();
                                        Laheta(palvelin, ep, "DATA " + luku);
                                        break;
                                    case "402":
                                        Console.WriteLine("Ei ole sinun vuoro arvata.");
                                        break;
                                    default:
                                        Console.WriteLine("Virhe " + status[1]);
                                        break;
                                } // ACK switch status[1]
                                break;
                            case "DATA":
                                Laheta(palvelin, ep, "ACK 300");
                                Console.WriteLine("Vastustaja arvasi {0}. Vastaus on väärin. Sinun vuoro arvata." , status[1]);
                                Console.WriteLine("Anna numero");
                                luku = Console.ReadLine();
                                Laheta(palvelin, ep, "DATA " + luku);
                                break;
                            case "QUIT":
                                switch (status[1])
                                {
                                    case "501":
                                        Console.WriteLine("Arvasit oikein! Voitit!");
                                        on = false;
                                        Console.WriteLine("Ohjelma sammuu. Paina mitä tahansa nappia.");
                                        Console.ReadKey();         
                                        Laheta(palvelin, ep, "ACK 500");

                                        break;
                                    case "502":
                                        if (status.Length > 2) Console.WriteLine("Vastustaja arvasi oikein. Oikea luku oli " + status[2]);
                                        else Console.WriteLine("Vastustaja arvasi oikein. Hävisit.");
                                        on = false;
                                        Console.WriteLine("Ohjelma sammuu. Paina mitä tahansa nappia.");
                                        Console.ReadKey();         
                                        Laheta(palvelin, ep, "ACK 500");
                                        break;
                                    default:
                                        Console.WriteLine("Virhe " + status[1]);
                                        break;
                                }
                                break;
                        } // GAME switch status[0]
                        break;
                    default:
                        Console.WriteLine("Virheellinen tilanne. Suljetaan ohjelma");
                        on = false;
                        break;
                } // switch tila
            } while (on);
 
            palvelin.Close();
        }
    }
}
