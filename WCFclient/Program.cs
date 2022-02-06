using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Access access1 = new Access() { IP = "0.0.0.1", ID = "1", Service = "Servicio1", Button = true };
            Access access2 = new Access() { IP = "0.0.0.8", ID = "2", Service = "Servicio1", Button = true };
            Access access3 = new Access() { IP = "0.0.0.15", ID = "1", Service = "Servicio2", Button = false };
            byte result;

            using (ServiceReference1.Service1Client client = new ServiceReference1.Service1Client())
            {
                result = client.TryAccess(access1);
                Console.WriteLine(access1.IP + ": " + client.Result(result));
                result = client.TryAccess(access2);
                Console.WriteLine(access2.IP + ": "+ client.Result(result));
                result = client.TryAccess(access3);
                Console.WriteLine(access3.IP + ": " + client.Result(result));
                result = client.TryAccess(access1);
                Console.WriteLine(access1.IP + ": " + client.Result(result));

                Console.WriteLine("\nPulsa <Enter> para terminar el cliente.");
                Console.ReadLine();
                client.Close();
            }
            
        }
    }
}
