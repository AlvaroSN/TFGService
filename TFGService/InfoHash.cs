using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TFGService
{
    public class InfoHash
    {
        //Varibales importadas del servicio
        private int maxAccessTime = Service1.maxAccessTime;
        private int maxPeriodAccess = Service1.maxPeriodAccess;

        private HashSet<String> sessionIDList;  //Hashset para guardar los diferentes IDs de sesión
        private int numAccess;                  //Número total de accesos
        private int numFailAccess;              //Número total de accesos, después de se le haya denegado el acceso
        private int numAccessURL;               //Número de accesos por URL
        private int numAccessButton;            //Número de accesos por botón
        private int numAccessList;              //Número de accesos por lista
        private bool access;                    //Variable para controlar el permiso de acceso
        private bool accessDenied;              //Acceso denegado permanente
        private bool accessAllowed;             //Acceso permito permanente
        private bool accessURL;                 //Control de acceso por URL
        private bool accessList;                //COntrol de acceso por lista
        private bool vpn;                       //Acceso denegado a VPNs
        private DateTime firstAccess;           //Primer acceso
        private DateTime lastAccess;            //Último acceso
        private Queue<long> timeouts;           //Cola para chequear los accesos por tiempo
        private Queue<long> periods;            //Cola para chequear los accesos periódicos

        public InfoHash()
        {
            sessionIDList = new HashSet<String>();
            numAccess = 0;
            numFailAccess = 0;
            numAccessButton = 0;        
            numAccessURL = 0;           
            numAccessList = 0;          
            access = true;              
            accessAllowed = false;      
            accessDenied = false;       
            accessURL = true;           
            accessList = true;          
            vpn = false;                
            firstAccess = DateTime.Now; 
            lastAccess = DateTime.Now;  
            timeouts = new Queue<long>(); 
            periods = new Queue<long>();
        }

        public void AddAccess(String type, String id)
        {
            numAccess++;
            if (!access) numFailAccess++;   //Se suma el número de accesos cuando no tienes permiso

            switch (type)
            {
                case "button":
                    numAccessButton ++;
                    break;
                case "url":
                    numAccessURL++;
                    break;
                case "list":
                    numAccessList++;
                    break;
                default:
                    break;
            }
            //Añadir ID de sesión al hashset si no ha accedido antes
            if (!sessionIDList.Contains(id)) sessionIDList.Add(id);
            SetTimeAtMaxAccess();       //Comprobar que no se supere un número máximo de accesos por tiempo
            SetPeriods();               //Comprobar que no se producen accesos periódicos
            lastAccess = DateTime.Now;  //Actualizar fecha del último acceso
        }

        //Gestión de la cola para controlar el máximo de accesos por tiempo
        public void SetTimeAtMaxAccess()
        {
            if (numAccess <= maxAccessTime)
            {
                timeouts.Enqueue(DateTime.Now.Ticks);
            } 
            else
            {
                timeouts.Dequeue();
                timeouts.Enqueue(DateTime.Now.Ticks);
            }
            
        }

        //Gestión de la cola para controlar los accesos periódicos
        public void SetPeriods()
        {
            if (numAccess <= maxPeriodAccess)
            {
                periods.Enqueue(TimeFromLastAccess() / TimeSpan.TicksPerSecond);
            }
            else
            {
                periods.Dequeue();
                periods.Enqueue(TimeFromLastAccess() / TimeSpan.TicksPerSecond);
            }

        }

        //Cáculo de la desviación típica de una cola
        public double Deviation(Queue<long> list)
        {
            double standardDeviation = 0;
            if (list.Any())
            {
                double avg = list.Average();  
                double sum = list.Sum(d => Math.Pow(d - avg, 2)); 
                standardDeviation = Math.Sqrt((sum) / (list.Count() - 1));
            }
            return standardDeviation;
        }

        public void AllowAccess()
        {
            accessAllowed = true;
            accessDenied = false;
            vpn = false;
            access = true;
        }

        public void DenyAccess()
        {
            accessDenied = true;
            accessAllowed = false;
            vpn = false;
            access = false;
        }

        public void IsVPN()
        {
            vpn = true;
            accessAllowed = false;
            accessDenied = false;
            access = false;
        }

        public void Access(bool perm)
        {
            access = perm;
        }

        public void AccessURL(bool perm)
        {
            accessURL = perm;
        }

        public void AccessList(bool perm)
        {
            accessList = perm;
        }

        public long TimeFromLastAccess()
        {
            return DateTime.Now.Ticks - lastAccess.Ticks;
        }

        public long TimeFromFirstAccess()
        {
            return DateTime.Now.Ticks - firstAccess.Ticks;
        }

        public long Timeout()
        {
            return timeouts.Last() - timeouts.First();
        }

        public double PeriodCheck()
        {
            return Deviation(periods);
        }

        public int SessionIDs()
        {
            return sessionIDList.Count;
        }

        //Getters
        public int NumAccess()
        {
            return numAccess;
        }

        public int NumFailAccess()
        {
            return numFailAccess;
        }

        public int NumAccessButton()
        {
            return numAccessButton;
        }

        public int NumAccessURL()
        {
            return numAccessURL;
        }

        public int NumAccessList()
        {
            return numAccessList;
        }

        public bool AccessDenied()
        {
            return accessDenied;
        }

        public bool Access()
        {
            return access;
        }

        public bool AccessURL()
        {
            return accessURL;
        }

        public bool AccessList()
        {
            return accessList;
        }

        public bool AccessAllowed()
        {
            return accessAllowed;
        }

        public bool VPN()
        {
            return vpn;
        }

        public DateTime FirstAccess()
        {
            return firstAccess;
        }

        public DateTime LastAccess()
        {
            return lastAccess;
        }

    }

}