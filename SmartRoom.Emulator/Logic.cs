using FluentNHibernate.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartRoom.Emulator
{
    public class Logic
    {
        private Dictionary<string, Tuple<Task, CancellationTokenSource>> _fading;

        public ObservableDictionary<string, byte> Pins { get; set; }
        public Dictionary<string, Tuple<bool, string>> IdValues { get; private set; }
        public int FadeDuration { get; set; }

        public Logic()
        {
            _fading = new Dictionary<string, Tuple<Task, CancellationTokenSource>>();
            FadeDuration = 500;
            Pins = new ObservableDictionary<string, byte>()
            {
                {"D0", 0},{"D1", 0},{"D2", 0},{"D3", 0},{"D4", 0},{"D5", 0},{"D6", 0},
                {"D7", 0},{"D8", 0},{"D9", 0},{"D10", 0},{"D11", 0},{"D12", 0},{"D13", 0},
                {"A0", 0},{"A1", 0},{"A2", 0},{"A3", 0},{"A4", 0},{"A5", 0},{"A6", 0},
                {"0", 0},{"1", 0},{"2", 0},{"3", 0},{"4", 0},{"5", 0},{"6", 0},{"7", 0},
                {"8", 0},{"9", 0},{"10", 0},{"11", 0},{"12", 0},{"13", 0},{"14", 0},{"15", 0}
            };
            IdValues = new Dictionary<string, Tuple<bool, string>>()
            {
                {"0", new Tuple<bool, string>(false, "255") },
                {"1", new Tuple<bool, string>(false, "0") },
                {"2", new Tuple<bool, string>(true, "128") },
                {"3", new Tuple<bool, string>(true, "Hi short") },
                {"4", new Tuple<bool, string>(true, "It's over 32 characters long!!! :)") }
            };
        }

        public bool IsFading(string pin)
        {
            if(_fading.TryGetValue(pin, out Tuple<Task, CancellationTokenSource> tuple))
                return tuple.Item1.IsCompleted;
            return false;
        }

        public bool WasFading(string pin) => _fading.ContainsKey(pin);

        public byte[] ProcessData(byte[] data, int count)
        {
            List<byte> output = new List<byte>();
            for (int i = 0; i < count; i++)
            {
                if (((data[i] & 0b10000000) >> 7) == 0) //Get
                {
                    byte action = (byte)((data[i] & 0b01100000) >> 5);
                    byte pin = (byte)(data[i] & 0b00011111);
                    if (action == 0) //Arduino
                    {
                        string pinName = pin >= 14 ? ("A" + (pin - 14)) : ("D" + pin);
                        output.AddRange(CreatePinPkg(pinName));
                    }
                    else if (action == 1) //TLC
                    {
                        output.AddRange(CreatePinPkg(pin.ToString()));
                    }
                    else if (action == 2) //ID
                    {
                        if(IdValues.TryGetValue(pin.ToString(), out Tuple<bool, string> v))
                        {
                            if (v.Item1 == true)
                                output.AddRange(CreateIdPkg(pin, v.Item2));
                            else
                                output.AddRange(CreateIdPkg(pin, byte.Parse(v.Item2)));
                        }
                    }
                }
                else
                {
                    string pinName = "";
                    byte pin = ((byte)(data[i] & 0b00011111));
                    if (((data[i] & 0b01000000) >> 6) == 0) //Arduino
                        pinName = (pin >= 14) ? ("A" + (pin - 14)) : ("D" + pin);
                    else //TLC
                        pinName = pin.ToString();

                    bool fade = ((data[i] & 0b00100000) >> 5) == 1;

                    i++;
                    if (_fading.ContainsKey(pinName))
                    {
                        _fading[pinName].Item2.Cancel();
                        _fading[pinName].Item2.Dispose();
                        _fading.Remove(pinName);
                    }
                    if (fade)
                        Fade(pinName, data[i]);
                    else
                        Pins[pinName] = data[i];
                }
            }
            output.Add(0b01100000); //END PKG
            return output.ToArray();
        }
        public void Fade(string pin, byte value)
        {
            var cts = new CancellationTokenSource();
            var task = Task.Run(() =>
            {
                try
                {
                    cts.Token.ThrowIfCancellationRequested();
                    var s = new Stopwatch();
                    var beg = Pins[pin];
                    int sub = (int)value - (int)beg;
                    s.Start();
                    while (s.ElapsedMilliseconds <= FadeDuration)
                    {
                        Pins[pin] = (byte)Math.Round(beg + (sub * ((float)s.ElapsedMilliseconds / (float)FadeDuration)));
                    }
                    Pins[pin] = value;
                }
                catch (Exception) { }
            }, cts.Token);
            _fading[pin] = new Tuple<Task, CancellationTokenSource>(task, cts);
        }

        public byte[] CreatePinPkg(string pin)
        {
            byte[] output = new byte[2] { 0, 0 };
            if (pin[0] == 'D')
            {
                byte p = byte.Parse(pin.Substring(1));
                output[0] = p;
                output[1] = Pins[pin];
            }
            else if (pin[0] == 'A')
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

        public byte[] CreateIdPkg(byte id, string text)
        {
            var output = new List<byte>();
            output.Add((byte)(0b11000000 + (id & 0b00001111)));

            var enc = Encoding.ASCII.GetBytes(text);
            output.AddRange(enc);
            output.Add(0b00000011); //End of Text char

            return output.ToArray();
        }

        public byte[] CreateIdPkg(byte id, byte value)
        {
            var output = new byte[2];

            output[0] = ((byte)(0b01000000 + (id & 0b00001111)));
            output[1] = value;

            return output;
        }
    }
}
