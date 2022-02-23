using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;

namespace TFGService
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service1 : IService1
    {
        
        //Variable para controlar el timer
        public static readonly long timeElapsed = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["timeElapsed"]);
        public static readonly int maxAccess = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxAccess"]);
        public static readonly int maxAccessTime = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxAccessTime"]);

        private static ConcurrentDictionary<string, InfoHash> ipHash = new ConcurrentDictionary<string, InfoHash>();
        public static Reader reader;
        public static HashSet<String> whiteList;
        public static HashSet<String> blackList;
        public static HashSet<String> vpnList;
        public static String registerFile = "C:\\inetpub\\ServicioIPControlWCF\\registro.txt";

        //Timer para controlar la escritura de las listas
        private static System.Timers.Timer timer;
        public Service1()
        {
            if (timer == null)
            {
                timer = new System.Timers.Timer(timeElapsed);
                timer.Elapsed += SetReader;
                timer.AutoReset = true;
                timer.Enabled = true;
                SetReader(null, null);
            }
        }

        private static void SetReader(object source, ElapsedEventArgs e)
        {
            reader = new Reader();
            whiteList = reader.WhiteList();
            blackList = reader.Blacklist();
            vpnList = reader.VPNList();
        }

        public void controlList(String ip, InfoHash info)
        {

            StreamWriter register = new StreamWriter(registerFile, true, System.Text.Encoding.Default);
            register.WriteLine(ip);
            register.Close();

            /*if (!ipHash.ContainsKey(ip))
            {
                ipHash.TryAdd(ip, new InfoHash("Servicio"));
                StreamWriter register = new StreamWriter(registerFile, true, System.Text.Encoding.Default);
                register.WriteLine(ip);
                register.Close();
            }*/

            //Prevalece la lista blanca
            if (whiteList.Contains(ip))
            {
                info.AllowAccess();
                return;
            }

            if (blackList.Contains(ip))
            {
                info.DenyAccess();
                return;
            }

            foreach (string line in vpnList)
            {
                if (ip.StartsWith(line))
                {
                    info.IsVPN();
                    return;
                }
            }

        }

        public void UpdateInfoHash(Access access, String ip, InfoHash info)
        {
            controlList(access.IP, info);
            info.AddAccess(access.Type);
        }


        public byte TryAccess(Access access)
        {
            try
            {
                InfoHash info = ipHash.GetOrAdd(access.IP, new InfoHash(access.Service));
                //Si hay algún problema se le da acceso al cliente
                new Task(() => UpdateInfoHash(access, access.IP, info)).Start();

                if (info.AccessAllowed())
                {
                    return 0;

                }
                else if (info.AccessDenied())
                {
                    return 1;

                }
                else if (info.VPN())
                {
                    return 2;
                }
                else
                {
                    if (info.NumAccess() > maxAccess)
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }
            }

            catch (Exception)
            {
                return 0;
            }
            finally
            {
                //if (hashIP.ContainsKey(ip)) hashIP[ip].Libre(true); // Se deja libre la IP por si se va a eliminar del HashIP.
            }

        }

    }
}
