using System;
using System.Net;

namespace SmartRoom.Emulator
{
    class Program
    {
        static void Main(string[] args)
        {
            //Vars
            int port = 23;
            string serverIp = "192.168.2.193";
            bool autoIp = false;

            //Auto get local IP
            if(autoIp)
            {
                var addr = Dns.GetHostAddresses(Dns.GetHostName());
                foreach(var ip in addr)
                    if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        serverIp = ip.ToString();
                        break;
                    }
            }

            //Init
            HardwareEmulator emu = new HardwareEmulator(serverIp, port);
            emu.Start();
            while(emu.IsRunning) 
            {
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    emu.Stop();
            }
        }
    }
}
