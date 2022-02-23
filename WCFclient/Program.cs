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
            Access access1 = new Access() { IP = "0.0.0.1", ID = "1", Service = "Servicio1", Type = "button" };
            Access access2 = new Access() { IP = "0.0.0.8", ID = "2", Service = "Servicio1", Type = "url" };
            Access access3 = new Access() { IP = "0.0.0.15", ID = "1", Service = "Servicio2", Type = "lista" };
            Access access4 = new Access() { IP = "1.1.0.0", ID = "1", Service = "Servicio2", Type = "lista" };

            UserAccess(access1,2,0,false);
            UserAccess(access2,2,0,false);
            UserAccess(access3,3,0,false);
            UserAccess(access4,2,0,false);
            UserAccess(access3,5,0,true);

        }

        public static void UserAccess(Access access, int n, int ms, bool last)
        {
            byte result;
            using (ServiceReference1.Service1Client client = new ServiceReference1.Service1Client())
            {
                for (int i = 0; i<n; i++)
                {
                    result = client.TryAccess(access);
                    Console.WriteLine(access.IP + " intenta acceder...");
                    Thread.Sleep(500);
                    Console.WriteLine("Byte:" + result + " -> " + Result(result));
                    Console.WriteLine("");
                    Thread.Sleep(ms);
                }
                
                if (last)
                {
                    Console.WriteLine("\nPulsa <Enter> para terminar el cliente.");
                    Console.ReadLine();
                    client.Close();
                }

            }
        }

        public static string Result(byte x)
        {
            switch (x)
            {
                case 0:
                    return "El usuario pudo acceder (lista blanca)";
                case 1:
                    return "El usuario no pudo acceder (lista negra)";
                case 2:
                    return "El usuario no pudo acceder(VPN)";
                case 3:
                    return "El usuario superó el número de accesos permitidos";
                default:
                    return string.Format("El usuario pudo acceder");

            }
        }

    }
}
