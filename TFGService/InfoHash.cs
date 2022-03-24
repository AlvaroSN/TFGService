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
        private int maxAccessTime = Service1.maxAccessTime;
        private int maxPeriodAccess = Service1.maxPeriodAccess;
        //private string target;
        private int sessionIDs;             //Número de ids de sesión con los que se ha accedido
        private String lastSessionsID;      //Último id de sesión con el que se ha intentado acceder 
        private List<String> sessionIDList;
        private int numAccess;              //Número total de accesos
        private int numFailAccess;          //Número total de accesos, después de se le haya denegado el acceso
        private int numAccessURL;           //Número de accesos por URL
        private int numAccessButton;        //Número de accesos por botón
        private int numAccessList;          //Número de accesos por lista
        private bool access;                //Variable para controlar el permiso de acceso
        private bool accessDenied;          //Acceso denegado permanente
        private bool accessAllowed;         //Acceso permito permanente
        private bool accessURL;             //Control de acceso por URL
        private bool accessList;            //COntrol de acceso por lista
        private bool vpn;                   //Acceso denegado a VPNs
        private DateTime firstAccess;       //Primer acceso
        private DateTime lastAccess;        //Último acceso
        private List<long> timeouts;        //Tiempos de espera para acceder
        private long timeout;               //Tiempo en los últimos accesos
        private List<long> periods;
        private double periodCheck;

        public InfoHash()
        {
            sessionIDs = 0;             
            lastSessionsID = null;
            sessionIDList = new List<String>();
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
            timeouts = new List<long>(); 
            timeout = 0;
            periods = new List<long>();
            periodCheck = 0;
        }

        public void AddAccess(String type, String id)
        {
            lastAccess = DateTime.Now;
            numAccess++;
            if (!access) numFailAccess++;

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
            lastSessionsID = id;
            if (!sessionIDList.Contains(id)) sessionIDList.Add(id);
            sessionIDs = sessionIDList.Count;
            SetTimeAtMaxAccess();
            SetPeriods();
        }

        public void ResetAccesses()
        {
            firstAccess = DateTime.Now;
            lastAccess = DateTime.Now;
            numAccess = 0;
            numFailAccess = 0;
            numAccessButton = 0;
            numAccessURL = 0;
            numAccessList = 0;
            sessionIDs = 0;
            lastSessionsID = null;
            timeout = 0;
            timeouts.Clear();
            periodCheck = 0;
            periods.Clear();
            sessionIDList.Clear();
        }

        public void SetTimeAtMaxAccess()
        {
            if (numAccess <= maxAccessTime)
            {
                timeouts.Add(DateTime.Now.Ticks);
            } 
            else
            {
                timeouts.RemoveAt(0);
                timeouts.Add(DateTime.Now.Ticks);
                timeout = timeouts.Last() - timeouts.First();
            }
            
        }

        public void SetPeriods()
        {
            if (numAccess <= maxPeriodAccess)
            {
                periods.Add(TimeFromLastAccess());
            }
            else
            {
                periods.RemoveAt(0);
                periods.Add(TimeFromLastAccess());
                periodCheck = Deviation(periods);
            }

        }

        //Cáculo de la desviación típica
        public double Deviation(List<long> list)
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

        //Getters
        /*public string Service()
        {
            return target;
        }*/

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

        public long Timeout()
        {
            return timeouts.Last() - timeouts.First();
        }

        public double PeriodCheck()
        {
            return periodCheck;
        }

        public string LastSessionsID()
        {
            return lastSessionsID;
        }

        public int SessionIDs ()
        {
            return sessionIDList.Count;
        }

    }

}