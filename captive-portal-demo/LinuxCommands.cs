using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using captive_portal_demo.Utils;

namespace captive_portal_demo
{
    public static class LinuxCommands
    {
        public static string PREPARE_IPTABLES_PRESISTENT_INSTALLv4 =
            "echo iptables-persistent iptables-persistent/autosave_v4 boolean true | debconf-set-selections";

        public static string PREPARE_IPTABLES_PRESISTENT_INSTALLv6 =
            "echo iptables-persistent iptables-persistent/autosave_v6 boolean true | debconf-set-selections";

        public static string PURGE_IPTABLES_PERSISTENT_CONFIGS =
            "echo PURGE | debconf-communicate iptables-persistent";

        public static string INSTALL_IPTABLES_PERSISTENT = "apt-get -y install iptables-persistent";
        public static string INSTALL_CONNTRACK = "apt-get install -y conntrack";
        public static string INSTALL_DNSMASQ = "apt-get install -y dnsmasq";
        public static string INSTALL_HOSTAPD = "apt-get install -y hostapd";

        public static string SAVE_ORIG_DNSMASQ_CONFIG = "mv -n /etc/dnsmasq.conf /etc/dnsmasq.conf.orig";
        public static string MOVE_NEW_DNSMASQ_CONFIG_TEMPLATE = "cp {0} /etc/";

        public static string SAVE_ORIG_INTERFACES_CONFIG = "mv -n /etc/network/interfaces /etc/network/interfaces.orig";
        public static string MOVE_NEW_INTERFACES_CONFIG_TEMPLATE = "cp {0} /etc/network/";
        public static string MOVE_NEW_HOSTAPD_CONFIG_TEMPLATE = "cp {0} /etc/hostapd/";

        public static string IPTABLES_FLUSH = "iptables -F";
        public static string IPTABLES_DELETE_CHAINS = "iptables -X";
        public static string IPTABLES_RESTORE_TEMPLATE = "iptables-restore {0}";
    }

    public static class LinuxManager
    {
        public static void InstallPrerequisites()
        {
            //Install iptables-persistent

            LinuxCommands.PREPARE_IPTABLES_PRESISTENT_INSTALLv4.Shell();
            LinuxCommands.PREPARE_IPTABLES_PRESISTENT_INSTALLv6.Shell();
            LinuxCommands.INSTALL_IPTABLES_PERSISTENT.Shell();
            LinuxCommands.PURGE_IPTABLES_PERSISTENT_CONFIGS.Shell();

            //Install conntrack
            LinuxCommands.INSTALL_CONNTRACK.Shell();

            //Install dnsmasq
            LinuxCommands.INSTALL_DNSMASQ.Shell();

            //Install hostapd
            LinuxCommands.INSTALL_HOSTAPD.Shell();

        }

        public static void SetupDNSMASQ()
        {
            LinuxCommands.SAVE_ORIG_INTERFACES_CONFIG.Shell();

            var currentDirectory = Directory.GetCurrentDirectory();

            var dnsmasqConfPath = currentDirectory + "/ConfigFiles/dnsmasq.conf";
            var dnsmasqConfCopyCommand =
                String.Format(LinuxCommands.MOVE_NEW_DNSMASQ_CONFIG_TEMPLATE, dnsmasqConfPath);
            dnsmasqConfCopyCommand.Shell();
        }

        public static void SetupOSInterfaces()
        {
            LinuxCommands.SAVE_ORIG_DNSMASQ_CONFIG.Shell();

            var currentDirectory = Directory.GetCurrentDirectory();

            var interfacesConfPath = currentDirectory + "/ConfigFiles/interfaces";
            var interfacesConfCopyCommand =
                String.Format(LinuxCommands.MOVE_NEW_INTERFACES_CONFIG_TEMPLATE, interfacesConfPath);
            interfacesConfCopyCommand.Shell();
        }

        public static void SetupSysCtlRedirect()
        {
            var writer = new StringWriter();
            writer.WriteLine();
            writer.WriteLine("net.ipv4.ip_forward=1");
            writer.WriteLine();
            var sysCtlLines = writer.ToString().Split(Environment.NewLine);

            File.AppendAllLines(@"/etc/sysctl.conf", sysCtlLines);
        }

        public static void SetupHostapd()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var hostapdConfPath = currentDirectory + "/ConfigFiles/hostapd.conf";
            var hostapdConfCopyCommand =
                String.Format(LinuxCommands.MOVE_NEW_HOSTAPD_CONFIG_TEMPLATE, hostapdConfPath);
            hostapdConfCopyCommand.Shell();

            var defaultsPath = "/etc/default/hostapd";
            string hostapdDefaults = File.ReadAllText(defaultsPath);
            hostapdDefaults = hostapdDefaults.Replace("#DAEMON_CONF=\"\"", "DAEMON_CONF=\"/etc/ hostapd/hostapd.conf\"");
            File.WriteAllText(defaultsPath, hostapdDefaults);
        }

        public static void SetupIPTables()
        {
            LinuxCommands.IPTABLES_FLUSH.Shell();
            LinuxCommands.IPTABLES_DELETE_CHAINS.Shell();

            var currentDirectory = Directory.GetCurrentDirectory();
            var iptablesConfigPath = currentDirectory + "/ConfigFiles/iptables.dat";
            var iptablesRestoreCommand =
                String.Format(LinuxCommands.IPTABLES_RESTORE_TEMPLATE, iptablesConfigPath);
            iptablesRestoreCommand.Shell();
        }
    }
}
