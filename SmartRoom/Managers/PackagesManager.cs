using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SmartRoom.Connectors;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoom.Managers
{
    public class PackagesManager
    {
        public TcpConnector Connection { get; private set; }

        private ObservableCollection<Models.SwitchModel> _switches;
        private Dictionary<string, Tuple<bool, byte>> _pinValue;
        private Models.SettingsModel _settings;

        public PackagesManager(ObservableCollection<SwitchModel> switches, SettingsModel settings)
        {
            _switches = switches;
            _settings = settings;
            _pinValue = new Dictionary<string, Tuple<bool, byte>>();

            _switches.CollectionChanged += SwitchesCollectionChanged;

            Connection = new TcpConnector(_settings.Address, _settings.Port);
            Connection.DataReceivedEvent += DataReceived;
        }

        public async Task UpdateAllPins()
        {
            if (Connection.IsConnected == false)
                return;

            var data = GetPinsGetPackages(_switches);
            Connection.Send(data.ToArray());

            bool wait = true;
            void Received(object sender, EventArgs e) => wait = false;
            Connection.DataReceivedEvent += Received;
            while (wait)
                await Task.Delay(10);
            Connection.DataReceivedEvent -= Received;
        }

        private List<byte> GetPinsGetPackages(IEnumerable<Models.SwitchModel> switches)
        {
            //Enabled = false & get all pins
            var pins = new HashSet<string>();
            foreach (var s in switches)
            {
                s.Enabled = false;
                if (s is Models.ToggleSwitchModel)
                    pins.Add((s as Models.ToggleSwitchModel).Pin);
                else if (s is Models.SliderSwitchModel)
                    pins.Add((s as Models.SliderSwitchModel).Pin);
                else if (s is Models.ColorSwitchModel)
                {
                    var model = s as Models.ColorSwitchModel;
                    pins.Add(model.RedPin);
                    pins.Add(model.GreenPin);
                    pins.Add(model.BluePin);
                }
            }

            //Create pkg data
            var data = new List<byte>();
            foreach (var p in pins)
                data.Add(Adapters.PackageAdapter.CreateGetPackage(p));

            return data;
        }

        private void SwitchesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Models.SwitchModel m in e.OldItems)
                    m.PropertyChanged -= SwitchPropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (Models.SwitchModel m in e.NewItems)
                    m.PropertyChanged += SwitchPropertyChanged;
            }

            //When new switch is created, get values from hardware
            if (Connection.IsConnected == false)
                return;

            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                var data = GetPinsGetPackages(e.NewItems.Cast<Models.SwitchModel>());
                Connection.Send(data.ToArray());
            }
        }

        private void SwitchPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Toggle" || e.PropertyName == "Value" || e.PropertyName == "Color")
            {
                if (sender is Models.ToggleSwitchModel)
                {
                    var model = sender as Models.ToggleSwitchModel;
                    _pinValue[model.Pin] = new Tuple<bool, byte>(model.Fade, (byte)(model.Toggle ? 255 : 0));
                }
                else if (sender is Models.SliderSwitchModel)
                {
                    var model = sender as Models.SliderSwitchModel;
                    _pinValue[model.Pin] = new Tuple<bool, byte>(model.Fade, (byte)Math.Round(model.Value * 255));
                }
                else if (sender is Models.ColorSwitchModel)
                {
                    var model = sender as Models.ColorSwitchModel;
                    var col = model.Color.GetRGB();

                    _pinValue[model.RedPin] =   new Tuple<bool, byte>(model.Fade, col.R);
                    _pinValue[model.GreenPin] = new Tuple<bool, byte>(model.Fade, col.G);
                    _pinValue[model.BluePin] =  new Tuple<bool, byte>(model.Fade, col.B);
                }

                if (Connection.IsReady)
                    SendWaiting();
            }
        }

        private void DataReceived(object sender, EventArgs e)
        {
            var data = sender as List<byte>;
            if (data.Count != 0)
            {
                var pkgs = Adapters.PackageAdapter.DeserializePackages(data.ToArray());
                data.Clear();
                ProcessPackages(pkgs);
            }

            if(Connection.IsConnected)
                SendWaiting();
        }

        private void SendWaiting()
        {
            if (_pinValue.Count == 0)
                return;

            var data = new List<byte>();
            var items = _pinValue.ToArray();
            foreach (var p in items)
            {
                data.AddRange(Adapters.PackageAdapter.CreateSetPackage(p.Key, p.Value.Item1, p.Value.Item2));
                _pinValue.Remove(p.Key);
            }
            Connection.Send(data.ToArray());
        }
          
        private void ProcessPackages(List<Models.PackageModel> packages) //Very intense process, try to optimize in future, updates only first with pin, not all
        {
            foreach (var s in _switches)
            {
                if (s is Models.ToggleSwitchModel)
                {
                    var model = s as Models.ToggleSwitchModel;
                    var pkg = packages.TakeWhile(x => x is Models.PackageValueModel)
                                      .Select(x => x as Models.PackageValueModel)
                                      .Where(x => x.PinId == model.Pin && x.IsID == false)
                                      .FirstOrDefault();
                    if (pkg == null) continue;
                    model.Toggle = (pkg.Value != 0);
                    model.Enabled = true;
                }
                else if (s is Models.SliderSwitchModel)
                {
                    var model = s as Models.SliderSwitchModel;
                    var pkg = packages.TakeWhile(x => x is Models.PackageValueModel)
                                      .Select(x => x as Models.PackageValueModel)
                                      .Where(x => x.PinId == model.Pin && x.IsID == false)
                                      .FirstOrDefault();

                    if (pkg == null) continue;
                    model.Value = (float)pkg.Value / 255f;
                    model.Enabled = true;
                }
                else if(s is Models.ColorSwitchModel)
                {
                    var model = s as Models.ColorSwitchModel;
                    var pkg = packages.TakeWhile(x => x is Models.PackageValueModel)
                                      .Select(x => x as Models.PackageValueModel)
                                      .Where(x => x.IsID == false && (x.PinId == model.RedPin || x.PinId == model.GreenPin || x.PinId == model.BluePin))
                                      .ToList();

                    if (pkg.Count == 0) continue;
                    var color = model.Color.GetRGB();
                    foreach (var p in pkg)
                        if (p.PinId == model.RedPin) color.R = p.Value;
                        else if (p.PinId == model.GreenPin) color.G = p.Value;
                        else if (p.PinId == model.BluePin) color.B = p.Value;
                    model.Color.FromRGB(color);
                    model.Enabled = true;
                }
            }
        }
    }
}