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
        //Timer para controlar la creación de Reader
        private static System.Timers.Timer timer;
        //Variable para controlar el timer (nanosegundos)
        public static readonly long timeElapsed = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["timeElapsed"]);
        //Variable que indica el número máximo de veces total que puede acceder una dirección IP al servidor
        public static readonly int maxAccess = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxAccess"]);
        //Variable que indica el número máximo de veces total que puede acceder una dirección IP al servidor por URL
        public static readonly int maxAccessURL = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxAccessURL"]);
        //Variables que indica el número máximo de veces (maxAccessTime) que puede acceder una dirección IP al servidor en un periodo de tiempo en segundos (maxTime)
        public static readonly int maxAccessTime = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxAccessTime"]);
        public static readonly int maxTime = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxTime"]);
        //Variable que indica los segundos que tiene que estar una dirección sin acceder para que se restaure su número de consultas
        public static readonly long timeReset = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["timeReset"]);



        //Hash donde se van a almacenar los accesos al servidor
        private static ConcurrentDictionary<string, InfoHash> ipHash = new ConcurrentDictionary<string, InfoHash>();
        //Instacia de la clase Reader, que se encarga de la lectura de los ficheros para actualizar las siguientes listas:
        public static Reader reader;

        //Hashsets donde se van a guardar las listas de direcciones
        public static HashSet<String> whiteList;    //Listas de direcciones permitidas
        public static HashSet<String> blackList;    //Listas de direcciones que tiene prohibido el acceso al servidor
        public static HashSet<String> vpnList;      //Lista de direcciones que son VPNs, que tampoco tienen acceso
        //Fichero de escritura del log
        public static String registerFile = "C:\\inetpub\\ServicioIPControlWCF\\registro.txt";

        public Service1()
        {
            //Cuando el timer aún no esté inicializado, se crea con la variable timeElapsed y se le añade la función SetReader
            if (timer == null)
            {
                timer = new System.Timers.Timer(timeElapsed);
                timer.Elapsed += SetReader;
                timer.AutoReset = true;
                timer.Enabled = true;
                SetReader(null, null);
            }
        }

        //Función para crear la instancia de Reader y actualizar los hashes de la whitelist, blacklist y la lista de VPNs
        private static void SetReader(object source, ElapsedEventArgs e)
        {
            reader = new Reader();
            whiteList = reader.WhiteList();
            blackList = reader.Blacklist();
            vpnList = reader.VPNList();
        }

        //Función para checkear las listas
        public void ControlList(String ip, InfoHash info)
        {

            //Escritura del registro de accesos
            StreamWriter register = new StreamWriter(registerFile, true, System.Text.Encoding.Default);
            register.WriteLine(ip);
            register.Close();

            /*if (!ipHash.ContainsKey(ip))
            {
                ipHash.TryAdd(ip, new InfoHash("Servicio"));
            }*/

            //Prevalece la lista blanca
            if (whiteList.Contains(ip))
            {
                info.AllowAccess();
                return;
            }

            //En segundo lugar se chequea si está en la lista de direcciones de VPN
            foreach (string line in vpnList)
            {
                if (ip.StartsWith(line))
                {
                    info.IsVPN();
                    return;
                }
            }

            if (blackList.Contains(ip))
            {
                info.DenyAccess();
                return;
            }

        }

        //Función donde se comprueban los casos concretos en cada acceso (puedo llamarlo desde controlList) 
        public void CheckAccess(Access access, InfoHash info)
        {
            //Comprobar si la ip supera el número máximo de intentos permitidos
            if (info.NumAccess() > maxAccess)
            {
                info.Access(false);
            }

            if (info.NumAccessURL() > maxAccessURL)
            {
                info.AccessURL(false);
            }

            if (info.NumAccess() >= maxAccessTime && info.TimeFromFirstAccess() >= maxTime * TimeSpan.TicksPerSecond)
            {
                info.Access(false);
            }

            if (info.TimeFromLastAccess() < timeReset * TimeSpan.TicksPerSecond)
            {
                info.ResetAccesses();
            }

            
        }

        public void UpdateInfoHash(Access access, InfoHash info)
        {
            info.AddAccess(access.Type);
            CheckAccess(access, info);
            ControlList(access.IP, info);
        }


        public byte TryAccess(Access access)
        {
            //Si hay algún problema se le da acceso al cliente
            try
            {
                InfoHash info = ipHash.GetOrAdd(access.IP, new InfoHash(access.Service));
                CheckAccess(access, info);
                //Se lanza un nuevo con la función encargada de controlar el acceso de la dirección IP
                new Task(() => UpdateInfoHash(access, info)).Start();

                //Casos base de los accesos:
                if (info.AccessAllowed()) return 0;     //Está en la lista blanca
                if (info.VPN()) return 2;               //Es una VPN
                if (info.AccessDenied()) return 1;      //Está en la lista negra

                if (info.Access()) {
                    return 0;
                } 
                else
                {
                    return 1;
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
