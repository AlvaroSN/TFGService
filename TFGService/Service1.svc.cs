using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;

namespace TFGService
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service1 : IService1
    {
        private static System.Timers.Timer timer;
        public static HashSet<String> whiteList = new HashSet<String>();
        public static HashSet<String> blackList = new HashSet<String>();
        public static HashSet<String> ipHash = new HashSet<String>();
        public static String registerFile = "C:\\inetpub\\ServicioIPControlWCF\\registro.txt";

        public static readonly int maAccess = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxAccess"]);

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

        public void controlList(string ip)
        {
            if (!ipHash.Contains(ip))
            {
                ipHash.Add(ip);
                StreamWriter register = new StreamWriter(registerFile, true, System.Text.Encoding.Default);
                register.WriteLine(ip);
                register.Close();
            }
        }


        public byte TryAccess(Access access)
        {
            string ip = access.IP;
            controlList(ip);

            if (blackList.Contains(ip))
            {
                return 0;

            } else if(whiteList.Contains(ip))
            {
                return 1;

            } else
            {
                return 2;
            }
        }

        public string Result(byte x)
        {
            switch (x)
            {
                case 0:
                    return string.Format("El usuario está en la lista negra");
                case 1:
                    return string.Format("El usuario está en la lista blanca");
                default:
                    return string.Format("El usuario no está en ninguna lista");

            }
        }

    }
}
