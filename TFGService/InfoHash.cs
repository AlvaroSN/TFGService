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
        //private string target;
        private ConcurrentDictionary<String, int> sessionIDs;
        private int numAccess;
        private int numAccessURL;
        private int numAccessButton;
        private int numAccessList;
        private bool accessDenied;
        private bool accessAllowed;
        private bool access;
        private bool accessURL;
        private bool vpn;
        private DateTime firstAccess;
        private DateTime lastAccess;
        private DateTime interval;
        private ConcurrentDictionary<long, int> times;
        public InfoHash()
        {
            numAccess = 0;
            numAccessButton = 0;
            numAccessURL = 0;
            numAccessList = 0;
            sessionIDs = new ConcurrentDictionary<String, int>(); //Número de veces que se accede con ese ID de sesión
            accessAllowed = false;
            accessDenied = false;
            access = true;
            accessURL = true;
            vpn = false;
            firstAccess = DateTime.Now;
            lastAccess = DateTime.Now;
            times = new ConcurrentDictionary<long, int>(); //Tiempos de espera para acceder
        }

        public void AddAccess(String type, String id)
        {
            GetPeriodNumber();
            lastAccess = DateTime.Now;
            numAccess ++;
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

            if (sessionIDs.ContainsKey(id))
            {
                sessionIDs[id]++;
            }
            else
            {
                sessionIDs.TryAdd(id, 1);
            }
            //sessionIDs.GetOrAdd(id,0);
        }

        public int GetPeriodNumber()
        {
            long period = TimeFromLastAccess() / TimeSpan.TicksPerSecond;
            times.GetOrAdd(period, 0);
            return times[period]++;
        }

        public void ResetAccesses()
        {
            firstAccess = DateTime.Now;
            lastAccess = DateTime.Now;
            numAccess = 1;
            numAccessButton = 0;
            numAccessURL = 0;
            numAccessList = 0;
            sessionIDs.Clear();
            times.Clear();
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
            //accessAllowed = false;
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

        public ConcurrentDictionary<long, int> Times()
        {
            return times;
        }

    }

}