using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PeliPalvelin
{
    class PeliPalvelin
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
        static string[] Vastaanota(Socket s, ref EndPoint ep)
        {
            byte[] rec = new byte[256];
            int koko = s.ReceiveFrom(rec, ref ep);
            return Encoding.ASCII.GetString(rec, 0, koko).Split(' ');
        }

        static int Flip(int n)
        {
            if (n == 1) return 0;
            return 1;
        }

        static Boolean OnkoNumero(String str)
        {
            int tulos;
            return int.TryParse(str, out tulos);
        }
        static void Main(string[] args)
        {
            Socket palvelin;
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, 25100);
            try
            {
                palvelin = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                palvelin.Bind(iep);
            }
            catch
            {
                return;
            }

            String tila = "WAIT";
            Boolean on = true;
            int vuoro = -1;
            int pelaajat = 0;
            int quit_ACKp1 = 0;
            int quit_ACKp2 = 0;
            int luku = -1;
            int max_luku = 10;
            EndPoint[] pelaaja = new EndPoint[2];
            String[] nimi = new String[2];

            while (on)
            {
                IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)client;
                String[] kehys = Vastaanota(palvelin, ref remote);
                if (kehys.Length < 2) { Laheta(palvelin, remote, "ACK 404 Väärä kehysrakenne"); break; }
                switch (tila)
                {
                    case "WAIT":
                        switch (kehys[0])
                        {
                            case "JOIN":
                                if (pelaajat == 2) 
                                { 
                                    Laheta(palvelin, remote, "ACK 401 Pelissä on maksimimäärä pelaajia"); 
                                    break; 
                                }
                                pelaaja[pelaajat] = remote;
                                nimi[pelaajat] = kehys[1];
                                pelaajat++;
                                if (pelaajat == 1) Laheta(palvelin, remote, "ACK 201 JOIN OK");
                                else if (pelaajat == 2)
                                {
                                    // Arvotaan aloittaja
                                    // Arvotaan oikea luku
                                    Random rand = new Random();
                                    int aloittaja = rand.Next(0, 1);                                  
                                    vuoro = aloittaja;
                                    luku = rand.Next(0, max_luku);
                                    Console.WriteLine("Arvattava numero: " + luku);
                                    Laheta(palvelin, pelaaja[aloittaja], "ACK 202 " + nimi[Flip(vuoro)]);
                                    Laheta(palvelin, pelaaja[Flip(aloittaja)], "ACK 203 "+ nimi[aloittaja]);
                                    tila = "GAME";
                                }
                            
                                break;
                            default:
                                // virheenkäsittely
                                break;
                        } // WAIT switch kehys[0]
                        break;

                    case "GAME":
                        switch (kehys[0])
                        {
                            case "DATA":
                                if (!remote.Equals(pelaaja[vuoro]))
                                {
                                    Laheta(palvelin, remote, "ACK 402 Väärä vuoro");
                                    break;
                                }
                                else
                                {
                                    if (OnkoNumero(kehys[1]))
                                    {
                                        int arvaus = Convert.ToInt16(kehys[1]);
                                        if (arvaus == luku)
                                        {
                                            Laheta(palvelin, pelaaja[vuoro], "QUIT 501");
                                            Laheta(palvelin, pelaaja[Flip(vuoro)], "QUIT 502 " + luku);
                                            tila = "END";
                                        }
                                        else
                                        {
                                            Laheta(palvelin, pelaaja[vuoro], "ACK 300 DATA OK");
                                            Laheta(palvelin, pelaaja[Flip(vuoro)], "DATA " + arvaus);
                                            vuoro = Flip(vuoro);
                                            tila = "WAIT_ACK";
                                        }
                                    }
                                    else
                                    {
                                        Laheta(palvelin, pelaaja[vuoro], "ACK 407 Arvaus ei ollu numero");
                                        break;
                                    }
                                }
                                break;
                            default:
                                //virheenkäsittely
                                break;
                        } // GAME switch kehys[0]
                        break;

                    case "WAIT_ACK":
                        switch (kehys[0])
                        {
                            case "DATA":
                                if (remote.Equals(pelaaja[vuoro]))
                                { 
                                    Laheta(palvelin, remote, "ACK 400 Odotetaan kuittausviestiä");
                                    break;
                                }
                                Laheta(palvelin, remote, "ACK 402 Väärä vuoro");
                                break;
                            case "ACK":
                                if (!remote.Equals(pelaaja[vuoro]))
                                {
                                    Laheta(palvelin, remote, "ACK 402 Väärä vuoro");
                                    break;
                                }
                                if (kehys[1] != "300")
                                {
                                    Laheta(palvelin, remote, "ACK 403 Väärä kuittausviesti");
                                    break;
                                }
                                if (kehys[1] == "300")
                                {
                                    tila = "GAME";
                                }
                                break;
                            default:
                                if (remote.Equals(pelaaja[vuoro]))
                                { 
                                    Laheta(palvelin, remote, "ACK 400 Odotetaan kuittausviestiä");
                                    break;
                                }
                                Laheta(palvelin, remote, "ACK 402 Väärä vuoro");
                                break;
                        } // WAIT_ACK switch kehys[0]
                        break;

                    case "END":
                        switch (kehys[0])
                        {
                            case "ACK":
                                if (kehys[1] == "500")
                                {
                                    if (remote.Equals(pelaaja[vuoro]))
                                    {
                                        if (quit_ACKp1 == 0)
                                            if (quit_ACKp2 != 0) on = false;
                                            else quit_ACKp1 = 1;
                                        else Laheta(palvelin, remote, "ACK 400 Peli on loppunut ja lopetusviesti kuitattu.");
                                    }
                                    else
                                    {
                                        if (quit_ACKp2 == 0)
                                            if (quit_ACKp1 != 0) on = false;
                                            else quit_ACKp2 = 1;
                                        else Laheta(palvelin, remote, "ACK 400 Peli on loppunut ja lopetusviesti kuitattu.");
                                    }
                                }
                                break;
                            default:
                                Laheta(palvelin, remote, "ACK 400 Odotetaan kuittausviestiä");
                                break;
                        } // END swtich kehys[0]
                        break;

                    default:
                        Console.WriteLine("Virhe: Tila on " + tila);
                        on = false;
                        break;
                } // switch tila
            } // while on
        }
    }
}
