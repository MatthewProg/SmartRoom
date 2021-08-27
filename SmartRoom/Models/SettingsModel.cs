using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        private string _address;
        private ushort _port;
        private string _pins;

        public string Address
        {
            get { return _address; }
            set { if (_address == value) return; _address = value.Trim().ToLower(); OnPropertyChanged("Address"); }
        }

        public ushort Port
        {
            get { return _port; }
            set { if (_port == value) return; _port = value; OnPropertyChanged("Port"); }
        }

        public string Pins
        {
            get { return _pins; }
            set { if (_pins == value) return; _pins = new string(value.Trim().ToCharArray().Where(c => c != ' ').ToArray()).ToUpper(); OnPropertyChanged("Pins"); }
        }

        public SettingsModel()
        {
            _address = "192.168.4.1";
            _port = 23;
            _pins = "D13,D11,D10,D9,D3,A4,A5";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}