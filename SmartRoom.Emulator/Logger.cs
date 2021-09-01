using System;
using System.Collections.Generic;
using System.Text;

namespace SmartRoom.Emulator
{
    public static class Logger
    {
        public static object ConsoleLock { get; private set; } = new object();
        public static void Processing(bool proc)
        {
            lock (ConsoleLock)
            {
                Console.SetCursorPosition(0, 9);
                Console.Write("Processing: " + (proc ? "YES" : "NO "));
            }
        }

        public static void Received(int bytes)
        {
            lock (ConsoleLock)
            {
                Console.SetCursorPosition(0, 10);
                Console.Write($"Received {bytes} bytes    ");
            }
        }

        public static void Send(int bytes)
        {
            lock (ConsoleLock)
            {
                Console.SetCursorPosition(0, 11);
                Console.Write($"Send {bytes} bytes     ");
            }
        }

        public static void Server(string msg)
        {
            lock (ConsoleLock)
            {
                Console.SetCursorPosition(0, 14);
                Console.Write(msg + new string(' ', 100));
            }
        }
    }
}
