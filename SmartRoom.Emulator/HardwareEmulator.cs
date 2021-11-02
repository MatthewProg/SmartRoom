using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoom.Emulator
{
    public class HardwareEmulator
    {
        private TcpListener _server;
        private Logic _logic;
        private bool _run;

        public int Delay { get; set; }

        public HardwareEmulator(string ipAddress, int port = 23)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            _logic = new Logic();
            _run = true;

            Delay = 200;

            Console.CursorVisible = false;
            Logger.Processing(false);
            var list = new List<KeyValuePair<string, byte>>();
            foreach (var p in _logic.Pins)
                list.Add(p);
            PrintPins(list);
            _logic.Pins.CollectionChanged += PinValueChanged;
        }

        private void PrintPins(IList<KeyValuePair<string, byte>> list)
        {
            foreach (var i in list)
            {
                int index = 0;
                string key = "";
                if (i.Key[0] == 'D')
                {
                    index = int.Parse(i.Key.Substring(1));
                    if (index < 10) key = "D0" + index;
                    else key = i.Key;
                }
                else if (i.Key[0] == 'A')
                {
                    index = 14 + int.Parse(i.Key.Substring(1));
                    if (index - 14 < 10) key = "A0" + (index - 14);
                    else key = i.Key;
                }
                else
                {
                    index = int.Parse(i.Key) + 21;
                    if (index - 21 < 10) key = "00" + (index - 21);
                    else key = "0" + (index - 21);
                }

                int x = index / 7;
                int y = index % 7;
                lock (Logger.ConsoleLock)
                {
                    Console.SetCursorPosition(x * 10, y);
                    Console.Write($"{key}: {i.Value}  ");
                }
            }
        }

        private void PinValueChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PrintPins(e.NewItems.Cast<KeyValuePair<string, byte>>().ToList());
        }

        public void Start()
        {
            _server.Start();
            Console.SetCursorPosition(0, 13);
            Console.WriteLine($"Server is listening at {_server.LocalEndpoint}");
            Task.Run(async () => await Loop());
        }

        public bool IsRunning => _run;

        public void Stop()
        {
            _server.Stop();
            _run = false;
        }

        private async Task Loop()
        {
            while(_run)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                Logger.Server($"Client connected from {client.Client.RemoteEndPoint}");

                var stream = client.GetStream();
                while(client.Connected)
                {
                    byte[] buffer = new byte[32];
                    int i = 0;
                    do
                    {
                        if (i == buffer.Length) break;

                        buffer[i] = (byte)stream.ReadByte();
                        i++;
                    } while (stream.DataAvailable);

                    Logger.Received(i);

                    var response = _logic.ProcessData(buffer, i);
                    i = 0;
                    while(i<response.Length)
                    {
                        if (i % 32 == 0)
                        {
                            Logger.Processing(true);
                            await Task.Delay(Delay); //Simulate processing
                            Logger.Processing(false);
                        }

                        stream.WriteByte(response[i]);
                        i++;
                    }

                    Logger.Send(i);
                }
                Logger.Server("Client disconnected!");
            }
            Logger.Server("Server stopped!");
        }
    }
}
