using System;
using captive_portal_demo.Utils;

namespace captive_portal_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var lastOutput = string.Empty;
            Console.WriteLine("Hi! I am captive portal demo application!");

            Console.WriteLine("Will install prerequisites now...");
            LinuxManager.InstallPrerequisites();
            Console.WriteLine("Prerequisites installed. Proceeding...");

            Console.WriteLine("1. Need to setup open WiFi Network.");
            Console.WriteLine("1.1 Setting up DNS and DHCP via dnsmasq.");
            LinuxManager.SetupDNSMASQ();
            Console.WriteLine("1.2 Setting up interfaces configuration.");
            LinuxManager.SetupOSInterfaces();
            Console.WriteLine("1.3 Setting up ipv4 redirect capability.");
            LinuxManager.SetupSysCtlRedirect();
            Console.WriteLine("1.4 Setting up hostapd.");
            LinuxManager.SetupHostapd();
            Console.WriteLine("1.4 Setting up iptables rules.");
            LinuxManager.SetupIPTables();

            Console.WriteLine("2. Need to start captive portal web UI.");
            
        }

    }
}
