using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SmartRoom.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class MacroModel : INotifyPropertyChanged
    {
        private string _name;
        private bool _repeat;
        private bool _enabled;
        private bool _running;

        public MacroModel()
        {
            Items = new ObservableCollection<IMacroItemModel>();
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public bool Repeat
        {
            get => _repeat;
            set
            {
                if (_repeat == value) return;

                _repeat = value;
                OnPropertyChanged("Repeat");
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;

                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public bool Running 
        { 
            get => _running;
            set
            {
                if (_running == value) return;

                _running = value;
                OnPropertyChanged("Running");
            }
        }

        public ObservableCollection<IMacroItemModel> Items { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}