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
        //Hashes donde se van a almacenar las direcciones de los ficheros
        public static HashSet<String> whiteList = new HashSet<String>();
        public static HashSet<String> blackList = new HashSet<String>();
        public static HashSet<String> vpnList = new HashSet<String>();
        public static string blackListFile = "C:\\inetpub\\ServicioIPControlWCF\\listanegra.txt";
        public static string whiteListFile = "C:\\inetpub\\ServicioIPControlWCF\\listablanca.txt";
        public static string vpnListFile = "C:\\inetpub\\ServicioIPControlWCF\\VPN.txt";
        public Reader()
        {
            //LLamada a las funciones encargadas de leer los archivos e insertar las direcciones en los hashes
            ReadBlacklist(blackListFile);
            ReadWhitelist(whiteListFile);
            ReadVPN(vpnListFile);
        }

        private static void ReadBlacklist(string url)
        {
            StreamReader blacklistFile = null;
            blackList.Clear();
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
            whiteList.Clear();
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
            vpnList.Clear();
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

        public void AddIpToBlackList(string ip, InfoHash info)
        {
            info.DenyAccess();
            StreamReader sr = new StreamReader(blackListFile);
            string content = sr.ReadToEnd();
            sr.Close();
            if (!content.Contains(ip))
            {
                //Corregir escritura concurrente
                StreamWriter list = new StreamWriter(blackListFile, true, System.Text.Encoding.Default);
                list.WriteLine(ip);
                list.Close();
            }
            

            /*foreach (string line in System.IO.File.ReadLines(@"C:\\inetpub\\ServicioIPControlWCF\\listanegra.txt"))
            {
                if (line == ip)
                {
                    flag = false;
                }
            }*/
            
        }

        //Getters
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