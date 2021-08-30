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
    public class ToggleSwitchModel : SwitchModel
    {
        private bool _toggle;
        private string _pin;

        [JsonIgnore]
        public bool Toggle
        {
            get => _toggle;
            set 
            {
                if (_toggle == value) return;
                _toggle = value;
                OnPropertyChanged("Toggle");  
            }
        }

        [JsonProperty(propertyName: "P")]
        public string Pin
        {
            get => _pin;
            set
            {
                if (_pin == value) return;
                _pin = value;
                OnPropertyChanged("Pin");
            }
        }

        protected override void OnPropertyChanged(String info)
        {
            base.OnPropertyChanged(info);
        }
    }
}