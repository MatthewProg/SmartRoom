using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class SwitchModel : INotifyPropertyChanged
    {
        private string _title;
        private bool _fade;

        [JsonProperty(propertyName: "T")]
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;

                _title = value;
                OnPropertyChanged("Title");
            }
        }

        [JsonIgnore]
        public bool Fade
        {
            get => _fade;
            set
            {
                if (_fade == value) return;

                _fade = value;
                OnPropertyChanged("Fade");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}