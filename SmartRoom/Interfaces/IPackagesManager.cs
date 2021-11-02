using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Interfaces
{
    public interface IPackagesManager
    {
        public event EventHandler<Events.PinValueEventArgs> PinValuesUpdated;
        public event EventHandler<Events.IdValuesEventArgs> IdValuesReceived;

        public ITcpConnector Connection { get; }

        public void SetValue(IEnumerable<Models.SwitchModel> models);
        public void SetValue(Models.SwitchModel model);
        public void SetValue(string pin, byte value, bool fade);

        public void GetValue(IEnumerable<Models.SwitchModel> models);
        public void GetValue(Models.SwitchModel model);
        public void GetValue(string pin);

        //public void GetId(IEnumerable<Models.SensorModel> models);
        //public void GetId(Models.SensorModel model);
        public void GetId(IEnumerable<string> ids);
        public void GetId(string id);
    }
}