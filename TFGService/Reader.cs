using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Web;

namespace TFGService
{
    public class Reader
    {
        public static HashSet<String> whiteList = new HashSet<String>();
        public static HashSet<String> blackList = new HashSet<String>();
        public static HashSet<String> vpnList = new HashSet<String>();
        public Reader()
        {
            ReadBlacklist("C:\\inetpub\\ServicioIPControlWCF\\listanegra.txt");
            ReadWhitelist("C:\\inetpub\\ServicioIPControlWCF\\listablanca.txt");
            ReadVPN("C:\\inetpub\\ServicioIPControlWCF\\VPN.txt");
        }

        private static void ReadBlacklist(string url)
        {
            StreamReader blacklistFile = null;
            try
            {
                blacklistFile = new StreamReader(url);
                string line;
                while ((line = blacklistFile.ReadLine()) != null)
                {
                    blackList.Add(line);
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

        private static void ReadWhitelist(string url)
        {
            StreamReader whitelistFile = null;
            try
            {
                whitelistFile = new StreamReader(url);
                string line;
                while ((line = whitelistFile.ReadLine()) != null)
                {
                    whiteList.Add(line);
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

        private static void ReadVPN(string url)
        {
            StreamReader vpnFile = null;
            try
            {
                vpnFile = new StreamReader(url);
                string line;
                while ((line = vpnFile.ReadLine()) != null)
                {
                    vpnList.Add(line);
                }
            }

            catch (Exception)
            {
            }
            finally
            {
                if (vpnFile != null) vpnFile.Close();
            }
        }

        public HashSet<String> Blacklist()
        {
            return blackList;
        }

        public HashSet<String> WhiteList()
        {
            return whiteList;
        }

        public HashSet<String> VPNList()
        {
            return vpnList;
        }

    }
}