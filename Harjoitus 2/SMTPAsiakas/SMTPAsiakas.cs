using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace SMTPAsiakas
{
    class SMTPAsiakas
    {

        static int ParseVastaus(String s)
        {
            return Convert.ToInt16(s.Substring(0, 3));
        }

        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect("localhost", 25000);
            }
            catch (Exception ex)
            {
                Console.Write("Virhe: " + ex.Message);
                Console.ReadKey();
                return;
            }
            NetworkStream ns = new NetworkStream(s);
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            String from = "eetu.j.p.korhonen@student.jyu.fi";
            String to = "ekorhonen@gmail.com";
            String posti = "Testi posti";

            Boolean on = true;
            String viesti;
            String[] status;
            while (on)
            {
                viesti = sr.ReadLine();
                status = viesti.Split(' ');
                Console.WriteLine(viesti);
                switch (status[0])
                {
                    case "250":
                        switch(status[1]) 
                        {
                            case "2.0.0":
                                sw.WriteLine("QUIT");
                                break;
                            case "2.1.0":
                                sw.WriteLine("RCPT TO: " + to);
                                break;
                            case "2.1.5":
                                sw.WriteLine("DATA");
                                break;
                            default:
                                sw.WriteLine("MAIL FROM: "+ from);
                                break;
                        }
                        break;
                    case "220":
                        sw.WriteLine("HELO");
                        break;
                    case "221":
                        on = false;
                        break;
                    case "354":
                        sw.WriteLine(posti);
                        sw.WriteLine(".");
                        break;
                    default:
                        Console.WriteLine("Virhe (koodi {0})",status[0]);
                        sw.WriteLine("QUIT");
                        break;
                } // switch
                sw.Flush();
            } // while

            Console.Write("Ohjelma sulkeutuu. Paina jotain nappia.");
            Console.ReadKey();
            sw.Close();
            sr.Close();
            ns.Close();
            s.Close();
        }
    }
}
