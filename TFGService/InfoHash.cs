using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFGService
{
    public class InfoHash
    {
        private string service;
        private string sesionID;
        private int numAccess;
        private int numAccessURL;
        private int numAccessButton;
        private int numAccessList;
        private bool accessDenied;
        private bool accessAllowed;
        private bool vpn;

        public InfoHash(String action)
        {
            numAccess = 0;
            numAccessButton = 0;
            numAccessURL = 0;
            service = action;
            accessAllowed = false;
            accessDenied = false;
            vpn = false;
        }

        public void AddAccess(string type)
        {
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
        }

        public void AllowAccess()
        {
            accessAllowed = true;
            accessDenied = false;
            vpn = false;
        }

        public void DenyAccess()
        {
            accessDenied = true;
            accessAllowed = false;
        }

        public void IsVPN()
        {
            vpn = true;
            accessAllowed = false;
        }

        public string Service()
        {
            return service;
        }

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

        public bool AccessDenied ()
        {
            return accessDenied;
        }

        public bool AccessAllowed()
        {
            return accessAllowed;
        }

        public bool VPN()
        {
            return vpn;
        }

    }

}