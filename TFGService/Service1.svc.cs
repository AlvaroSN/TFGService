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
using System.Threading;

namespace TFGService
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service1 : IService1
    {
        //Varibales declaradas en el archivo de confirguración:
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
        //Variable que indica los segundos que tiene que estar una dirección sin acceder para que se borre del ipHash
        public static readonly long timeClean = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["timeClean"]);
        //Variable que indica cuantas IPs pueden acceder con el servidor, que empiecen por los mismos 3 números, antes de que se borren del ipHash
        public static readonly long maxMultiIP = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["maxMultiIP"]);
        //Variable para indicar que máximo de accesos periódicos que se van a permitir
        public static readonly int maxPeriodAccess = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["maxPeriodAccess"]);

        //Hash donde se van a almacenar los accesos al servidor
        private static ConcurrentDictionary<string, InfoHash> ipHash = new ConcurrentDictionary<string, InfoHash>();
        //Hash donde se van a almacenar el principio de las direcciones que accedan al servidor, para gestionar la multiIP
        //La clave son los 3 primeros números de la dirección IP, y el Hashset las ips que acceden y empiezan por la clave
        private static ConcurrentDictionary<string, HashSet<String>> multiIPHash = new ConcurrentDictionary<string, HashSet<String>>();
        //Hashsets donde se van a guardar las listas de direcciones
        public static HashSet<String> whiteList;    //Listas de direcciones permitidas
        public static HashSet<String> blackList;    //Listas de direcciones que tiene prohibido el acceso al servidor
        public static HashSet<String> vpnList;      //Lista de direcciones que son VPNs, que tampoco tienen acceso

        //Instacia de la clase Reader, que se encarga de la lectura de los ficheros para actualizar las siguientes listas:
        public static Reader reader;
        //Fichero de escritura del log de accesos al servidor
        public static String registerFile = "C:\\inetpub\\ServicioIPControlWCF\\registro.txt";

        public Service1()
        {
            //Cuando el timer aún no esté inicializado, se crea con la variable timeElapsed y se le añade la función SetReader
            if (timer == null)
            {
                timer = new System.Timers.Timer(timeElapsed);
                timer.Elapsed += SetReader;
                timer.Elapsed += CleanHash;
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

        private static void CleanHash(object source, ElapsedEventArgs e)
        {
            HashSet<String> aux = new HashSet<String>();
            InfoHash infoHash;

            //Recorrer el ipHash para comprobar que ips llevan sin acceder el tiempo establecido
            foreach (KeyValuePair<string, InfoHash> entry in ipHash)
            {
                if (entry.Value.TimeFromLastAccess() > timeClean * TimeSpan.TicksPerSecond)
                {
                    aux.Add(entry.Key);
                }
            }

            //Borrar IPs guardadas del ipHash
            foreach (var ip in aux)
            {
                lock (ipHash) ipHash.TryRemove(ip, out infoHash);
            }

            aux.Clear();
        }

        //Función para checkear las listas
        public void ControlList(String ip, InfoHash info)
        {

            //Escritura del registro de accesos en el log
            StreamWriter register = new StreamWriter(registerFile, true, System.Text.Encoding.Default);
            register.WriteLine(ip);
            register.Close();

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

            //Y en último lugar si está en la lista negra
            foreach (string line in blackList)
            {
                if (ip.StartsWith(line))
                {
                    info.DenyAccess();
                    return;
                }
            }

        }

        //Función donde se comprueban los casos concretos en cada acceso (puedo llamarlo desde controlList) 
        public void CheckAccess(Access access, InfoHash info)
        {
            //Si un usuario sigue accediendo aunque no tenga permiso, se le añade a la lista negra
            if (info.NumFailAccess() >= 10)
            {
                //Añadir a la lista negra
                reader.AddIpToBlackList(access.IP, info);
                return;
            }

            //Comprobar que no se accede periódicamente, un máximo número de veces, usando la desviación típica
            if (info.NumAccess() > maxPeriodAccess && info.PeriodCheck() <= 1)
            {
                //Añadir a la lista negra
                reader.AddIpToBlackList(access.IP, info);
                return;
            }

            //Comprobar si se está accediendo con muchas sesiones diferentes
            if (info.NumAccess() > 5 && info.NumAccess() - info.SessionIDs() < 2)
            {
                //Añadir a la lista negra
                reader.AddIpToBlackList(access.IP, info);
                return;
            }

            //Comprobar número máximo de accesos por tiempo
            if (info.NumAccess() > 10 && info.Timeout() < maxTime * TimeSpan.TicksPerSecond)
            {

                //No permitir seguir accediendo (castigo)
                info.Access(false);
                reader.AddIpToPunishmentFile(access.IP);
            }

            //Comprobar si la ip supera el número máximo de intentos permitidos
            if (info.NumAccess() > maxAccess)
            {
                //No permitir seguir accediendo (castigo)
                info.Access(false);
                reader.AddIpToPunishmentFile(access.IP);
            }

            //Comprobar multiIPs
            if (CheckMultiIP(access.IP, info))
            {
                //No permitir seguir accediendo (castigo)
                info.Access(false);
                reader.AddIpToPunishmentFile(access.IP);
            }

            //Comprobar si el número de accesos por URL supera al máximo permitido
            if (info.NumAccessURL() > maxAccessURL)
            {
                //No permitir acceder más por URL
                info.AccessURL(false);
            }

            //Comprobar si el número de accesos por lista supera al máximo permitido
            if (info.NumAccessList() > maxAccessURL)
            {
                //No permitir acceder más por lista
                info.AccessList(false);
            }

            
        }

        public bool CheckMultiIP(String ip, InfoHash info)
        {
            /*if (info.NumAccessURL() > maxAccessURL / 2 && info.NumAccessButton() < 5)
            {
                //Obtener 3 primeros números de la IP
                String ipBeginning = ip.Substring(0, ip.LastIndexOf('.') - 1);
                //Añadir IP al multiIPHash, o sumar uno a su valor
                int num = multiIPHash.AddOrUpdate(ipBeginning, new HashSet<String>, (key, oldValue) => oldValue.Add(ip));
                //Si el número de IPs supera el límite, devolver true
                if (num > maxMultiIP) return true;
            }*/  
            return false;
        }

        public void UpdateInfoHash(Access access, InfoHash info)
        {

            //Siempre se prueba en que listas está la dirección IP
            ControlList(access.IP, info);

            //Añadir el nuevo acceso al infoHash
            info.AddAccess(access.Type, access.ID);

            //Si la dirección IP está en la lista negra no se hace nada más
            //Controlar un mínimo de accesos para hacer chequeos
            if (!info.AccessDenied()) CheckAccess(access, info);            
        }


        public byte TryAccess(Access access)
        {
            InfoHash info;
            //Si hay algún problema se le da acceso al cliente
            try
            {
                lock (ipHash)
                {
                    info = ipHash.GetOrAdd(access.IP, new InfoHash());
                    Thread.Sleep(10000); //Para comprobar si los locks están funcionando bien
                }

                //Se lanza un nuevo con la función encargada de controlar el acceso de la dirección IP
                new Task(() => UpdateInfoHash(access, info)).Start();

                //Casos base de los accesos:
                if (info.AccessAllowed()) return 0;     //Está en la lista blanca
                if (info.VPN()) return 2;               //Es una VPN
                if (info.AccessDenied()) return 1;      //Está en la lista negra

                //Casos en los que ya no pueda acceder más por URL o por Lista
                if (!info.AccessURL()) return 4;
                if (!info.AccessList()) return 6;

                //Comprobar si tiene permiso de acceso
                if (info.Access()) {
                    return 0;
                } 
                else
                {
                    return 3;
                }
            }

            catch (Exception)
            {
                return 0;
            }

        }

    }
}
