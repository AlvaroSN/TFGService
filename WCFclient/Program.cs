using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TFGService;
using WCFclient.ServiceReference1;

namespace WCFclient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Comienzo de servicio");
            //Acceso con IP en lista negra
            Access access1 = new Access() { IP = "0.0.0.1", ID = "1", App = "Servicio1", Type = "button" };
            //Acceso con IP en lista blanca
            Access access2 = new Access() { IP = "0.0.0.8", ID = "2", App = "Servicio1", Type = "url" };
            //Acceso con VPN
            Access access3 = new Access() { IP = "1.1.0.0", ID = "1", App = "Servicio2", Type = "list" };
            //Accesos libres
            Access access4 = new Access() { IP = "0.0.0.15", ID = "1", App= "Servicio2", Type = "button" };
            Access access5 = new Access() { IP = "0.0.0.16", ID = "1", App = "Servicio2", Type = "button" };
            Access access6 = new Access() { IP = "0.0.0.17", ID = "1", App = "Servicio2", Type = "url" };
            Access access7 = new Access() { IP = "2.0.0.0", ID = "1", App = "Servicio2", Type = "button" };
            Access access8 = new Access() { IP = "0.0.0.18", ID = "1", App = "Servicio2", Type = "button" };
            Access access91 = new Access() { IP = "0.0.0.19", ID = "1", App = "Servicio2", Type = "button" };
            Access access92 = new Access() { IP = "0.0.0.19", ID = "2", App = "Servicio2", Type = "button" };
            Access access93 = new Access() { IP = "0.0.0.19", ID = "3", App = "Servicio2", Type = "button" };
            Access access94 = new Access() { IP = "0.0.0.19", ID = "4", App = "Servicio2", Type = "button" };
            Access access95 = new Access() { IP = "0.0.0.19", ID = "5", App = "Servicio2", Type = "button" };
            Access access96 = new Access() { IP = "0.0.0.19", ID = "6", App = "Servicio2", Type = "button" };

            using (ServiceReference1.Service1Client client = new ServiceReference1.Service1Client())
            {
                /*//Pruebas listas
                UserAccess(client,access1,3,500,false);
                UserAccess(client,access2,3,0,false);
                UserAccess(client,access3,3,500,true);

                //Prueba máximo total de accesos
                UserAccess(client,access4,25,200,true);

                //Prueba reseteo de accesos
                UserAccess(client, access5, 12, 500, false);
                UserAccess(client, access5, 1, 8000, false);
                UserAccess(client, access5, 12, 500, true);

                //Prueba timer
                UserAccess(client, access7, 5, 100, true);

                //Prueba accesos por URL
                UserAccess(client, access6, 6, 100, true);

                //Prueba máximo accesos por tiempo
                UserAccess(client, access8, 13, 0, true);*/

                //Prueba de periodicidad
                //UserAccess(client, access9, 5, 200, false);
                //UserAccess(client, access9, 5, 300, true);

                UserAccess(client, access8,15,50,true);
            }

        }

        public static void UserAccess(ServiceReference1.Service1Client client, Access access, int n, int ms, bool end)
        {
            var rand = new Random();
            int x = ms;
            byte result;
            for (int i = 0; i<n; i++)
            {
                result = client.TryAccess(access);
                Console.WriteLine(access.IP + " intenta acceder por " + (i+1) + "ª vez");
                Console.WriteLine("Byte:" + result + " -> " + Result(result));
                Console.WriteLine("");
                if (ms < 0) x = rand.Next(1001);
                Thread.Sleep(x);
            }
            Console.WriteLine("--------------------------------------------------------------------");
            
            if(end)
            {
                Console.WriteLine("\nPulsa <Enter> para continuar.");
                Console.ReadLine();
            }
            
        }

        public static string Result(byte x)
        {
            switch (x)
            {
                case 0:
                    return "El usuario pudo acceder";
                case 1:
                    return "El usuario no pudo acceder (lista negra)";
                case 2:
                    return "El usuario no pudo acceder(VPN)";
                case 3:
                    return "El usuario no puede acceder";
                case 4:
                    return "El usuario no puede acceder por URL";
                case 5:
                    return "El usuario acceso periódicamente";
                case 6:
                    return "El usuario no puede acceder por lista";
                default:
                    return "El usuario pudo acceder";

            }
        }

    }
}
