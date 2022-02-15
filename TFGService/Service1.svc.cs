using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace TFGService
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service1 : IService1
    {
        private static System.Timers.Timer timer;
        private static ConcurrentDictionary<string, InfoHash> ipHash = new ConcurrentDictionary<string, InfoHash>();
        public static HashSet<String> whiteList = new HashSet<String>();
        public static HashSet<String> blackList = new HashSet<String>();
        public static String registerFile = "C:\\inetpub\\ServicioIPControlWCF\\registro.txt";

        public static readonly int maxAccess = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxAccess"]);

        public Service1()
        {
            setBlacklist("C:\\inetpub\\ServicioIPControlWCF\\listanegra.txt");
            setWhitelist("C:\\inetpub\\ServicioIPControlWCF\\listablanca.txt");
        }

        public void setBlacklist(string url)
        {
            StreamReader blacklistFile = null;
            try
            {
                blacklistFile = new StreamReader(url);
                string line;
                while ((line = blacklistFile.ReadLine()) != null)
                {
                    lock(blackList) blackList.Add(line);
                }
            }

            catch (Exception)
            {
            }
            finally
            {
                if (blacklistFile != null) blacklistFile.Close();
            }
        }

        public void setWhitelist(string url)
        {
            StreamReader whitelistFile = null;
            try
            {
                whitelistFile = new StreamReader(url);
                string line;
                while ((line = whitelistFile.ReadLine()) != null)
                {
                    lock (whiteList) whiteList.Add(line);
                }
            }

            catch (Exception)
            {
            }
            finally
            {
                if (whitelistFile != null) whitelistFile.Close();
            }
        }

        public void controlList(String ip, InfoHash info)
        {
            if (!ipHash.ContainsKey(ip))
            {
                ipHash.TryAdd(ip, new InfoHash("Servicio"));
                StreamWriter register = new StreamWriter(registerFile, true, System.Text.Encoding.Default);
                register.WriteLine(ip);
                register.Close();
            }

            if (blackList.Contains(ip))
            {
                info.DenyAccess();
                return;
            }

            if(whiteList.Contains(ip))
            {
                info.AllowAccess();
                return;
            }
        }


        public byte TryAccess(Access access)
        {
            string ip = access.IP;
            InfoHash info = ipHash.GetOrAdd(ip, new InfoHash("Por defecto"));
            controlList(ip, info);
            info.AddAccess(access.Type);

            if (info.AccessAllowed())
            {
                return 0;

            } else if(info.AccessDenied())
            {
                return 1;

            } else if (info.VPN())
            {
                return 2;
            } else
            {
                if (info.NumAccess() > maxAccess)
                {
                    return 3;
                } else
                {
                    return 4;
                }
            }
        }

        public string Result(byte x)
        {
            switch (x)
            {
                case 0:
                    return "El usuario pudo acceder (lista blanca)";
                case 1:
                    return "El usuario no pudo acceder (lista negra)";
                case 2:
                    return "El usuario no pudo acceder(VPN)";
                case 4:
                    return "El usuario superó el número de accesos permitidos";
                default:
                    return string.Format("El usuario pudo acceder");

            }
        }

    }
}
