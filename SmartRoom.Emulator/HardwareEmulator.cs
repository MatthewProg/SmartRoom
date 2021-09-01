using FluentNHibernate.Utils;
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
        private Dictionary<string, Task> _fading;
        private TcpListener _server;
        private bool _run;

        public ObservableDictionary<string, byte> Pins { get; set; }
        public int Delay { get; set; }
        public int FadeDuration { get; set; }

        public HardwareEmulator(string ipAddress, int port = 23)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            _fading = new Dictionary<string, Task>();
            _run = true;

            Delay = 200;
            FadeDuration = 500;
            Pins = new ObservableDictionary<string, byte>()
            {
                {"D0", 0},{"D1", 0},{"D2", 0},{"D3", 0},{"D4", 0},{"D5", 0},{"D6", 0},
                {"D7", 0},{"D8", 0},{"D9", 0},{"D10", 0},{"D11", 0},{"D12", 0},{"D13", 0},
                {"A0", 0},{"A1", 0},{"A2", 0},{"A3", 0},{"A4", 0},{"A5", 0},{"A6", 0},
                {"0", 0},{"1", 0},{"2", 0},{"3", 0},{"4", 0},{"5", 0},{"6", 0},{"7", 0},
                {"8", 0},{"9", 0},{"10", 0},{"11", 0},{"12", 0},{"13", 0},{"14", 0},{"15", 0}
            };
            Pins.CollectionChanged += PinValueChanged;

            Console.CursorVisible = false;
            Logger.Processing(false);
            var list = new List<KeyValuePair<string, byte>>();
            foreach (var p in Pins)
                list.Add(p);
            PrintPins(list);
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

                    var response = ProcessData(buffer, i);
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

        private byte[] ProcessData(byte[] data, int count)
        {
            List<byte> output = new List<byte>();
            for(int i = 0;i<count;i++)
            {
                if(((data[i] & 0b10000000) >> 7) == 0) //Get
                {
                    byte action = (byte)((data[i] & 0b01100000) >> 5);
                    byte pin = (byte)(data[i] & 0b00011111);
                    if (action == 0) //Arduino
                    {
                        string pinName = pin >= 14 ? ("A" + pin) : ("D" + pin);
                        output.AddRange(CreatePinPkg(pinName));
                    }
                    else if (action == 1) //TLC
                    {
                        output.AddRange(CreatePinPkg(pin.ToString()));
                    }
                    else if(action == 2) //ID
                    {
                        //output.AddRange(CreateIdPkg(pin));
                    }
                }
                else
                {
                    string pinName = "";
                    byte pin = ((byte)(data[i] & 0b00011111));
                    if (((data[i] & 0b01000000) >> 6) == 0) //Arduino
                        pinName = (pin >= 14) ? ("A" + pin) : ("D" + pin);
                    else //TLC
                        pinName = pin.ToString();

                    bool fade = ((data[i] & 0b00100000) >> 5) == 1;

                    i++;
                    if (fade)
                        Fade(pinName, data[i]);
                    else
                    {
                        if (_fading.ContainsKey(pinName))
                        {
                            _fading[pinName].Dispose();
                            _fading.Remove(pinName);
                        }
                        Pins[pinName] = data[i];
                    }
                }
            }
            output.Add(0b01100000); //END PKG
            return output.ToArray();
        }

        private void Fade(string pin, byte value)
        {
            _fading[pin] = Task.Run(() =>
            {
                var s = new Stopwatch();
                var beg = Pins[pin];
                int sub = (int)value - (int)beg;
                s.Start();
                while(s.ElapsedMilliseconds <= FadeDuration)
                {
                    Pins[pin] = (byte)Math.Round(beg + (sub * ((float)s.ElapsedMilliseconds / (float)FadeDuration)));
                }
                Pins[pin] = value;
            });
        }

        private byte[] CreatePinPkg(string pin)
        {
            byte[] output = new byte[2] { 0, 0 };
            if (pin[0] == 'D')
            {
                byte p = byte.Parse(pin.Substring(1));
                output[0] = p;
                output[1] = Pins[pin];
            }
            else if(pin[0] == 'A')
            {
                byte p = byte.Parse(pin.Substring(1));
                output[0] = (byte)(p + 14);
                output[1] = Pins[pin];
            }
            else 
            {
                byte p = byte.Parse(pin);
                output[0] = (byte)(0b00100000 + p);
                output[1] = Pins[pin];
            }
            return output;
        }
    }
}
