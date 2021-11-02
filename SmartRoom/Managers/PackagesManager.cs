using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SmartRoom.Connectors;
using SmartRoom.Events;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoom.Managers
{
    public class PackagesManager : Interfaces.IPackagesManager
    {
        public event EventHandler<PinValueEventArgs> PinValuesUpdated;
        public event EventHandler<IdValuesEventArgs> IdValuesReceived;

        public Interfaces.ITcpConnector Connection { get; private set; }

        private Dictionary<string, Tuple<bool, byte>> _setQueue;
        private HashSet<Tuple<string, bool>> _getQueue;

        public PackagesManager(Interfaces.ITcpConnector connector)
        {
            _setQueue = new Dictionary<string, Tuple<bool, byte>>();
            _getQueue = new HashSet<Tuple<string, bool>>();

            Connection = connector;
            Connection.DataReceivedEvent += DataReceived;
        }

        private void DataReceived(object sender, Events.ObjectEventArgs e)
        {
            var data = e.Object as List<byte>;
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
            if (_setQueue.Count == 0 && _getQueue.Count == 0)
                return;

            var data = new List<byte>();
            foreach (var s in _setQueue)
                data.AddRange(Adapters.PackageAdapter.CreateSetPackage(s.Key, s.Value.Item1, s.Value.Item2));
            foreach (var g in _getQueue)
                data.Add(Adapters.PackageAdapter.CreateGetPackage(g.Item1, g.Item2));

            _setQueue.Clear();
            _getQueue.Clear();

            Connection.Send(data.ToArray());
        }
          
        private void ProcessPackages(List<Models.PackageModel> packages)
        {
            foreach (var pkg in packages)
            {
                if(pkg.IsID == false)
                {
                    if (pkg is PackageValueModel m)
                        PinValuesUpdated?.Invoke(this, new PinValueEventArgs(m.PinId, m.Value));
                }
                else
                {
                    if (pkg is PackageValueModel v)
                        IdValuesReceived?.Invoke(this, new IdValuesEventArgs(v.PinId, false, v.Value));
                    else if (pkg is PackageTextModel t)
                        IdValuesReceived?.Invoke(this, new IdValuesEventArgs(t.PinId, true, t.Text));
                }
            }
        }

        public void SetValue(IEnumerable<SwitchModel> models)
        {
            foreach (var m in models)
                foreach (var v in m.GetPinsValue())
                    _setQueue[v.Item1] = new Tuple<bool, byte>(m.Fade, v.Item2);

            if (Connection.IsReady)
                SendWaiting();
        }

        public void SetValue(SwitchModel model)
        {
            foreach (var v in model.GetPinsValue())
                _setQueue[v.Item1] = new Tuple<bool, byte>(model.Fade, v.Item2);

            if (Connection.IsReady)
                SendWaiting();
        }

        public void SetValue(string pin, byte value, bool fade)
        {
            _setQueue[pin] = new Tuple<bool, byte>(fade, value);

            if (Connection.IsReady)
                SendWaiting();
        }

        public void GetValue(IEnumerable<SwitchModel> models)
        {
            foreach (var m in models)
                foreach (var p in m.GetPinsValue())
                    _getQueue.Add(new Tuple<string, bool>(p.Item1, false));

            if (Connection.IsReady)
                SendWaiting();
        }

        public void GetValue(SwitchModel model)
        {
            foreach (var p in model.GetPinsValue())
                _getQueue.Add(new Tuple<string, bool>(p.Item1, false));

            if (Connection.IsReady)
                SendWaiting();
        }

        public void GetValue(string pin)
        {
            _getQueue.Add(new Tuple<string, bool>(pin, false));

            if (Connection.IsReady)
                SendWaiting();
        }

        public void GetId(IEnumerable<string> ids)
        {
            foreach (var id in ids)
                _getQueue.Add(new Tuple<string, bool>(id, true));

            if (Connection.IsReady)
                SendWaiting();
        }

        public void GetId(string id)
        {
            _getQueue.Add(new Tuple<string, bool>(id, true));

            if (Connection.IsReady)
                SendWaiting();
        }
    }
}