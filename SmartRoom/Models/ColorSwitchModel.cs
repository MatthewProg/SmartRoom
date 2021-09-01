using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class ColorSwitchModel : SwitchModel
    { 
        private ColorModel _color;
        private string _redPin;
        private string _greenPin;
        private string _bluePin;

        public ColorSwitchModel()
        {
            Color = new ColorModel();
        }

        [JsonIgnore]
        public ColorModel Color
        {
            get => _color;
            set
            {
                _color = value;
                _color.PropertyChanged += Color_PropertyChanged;
                OnPropertyChanged("Color");
            }
        }

        private void Color_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Color");
        }

        [JsonProperty(propertyName: "R")]
        public string RedPin
        {
            get => _redPin;
            set
            {
                if (_redPin == value) return;
                _redPin = value;
                OnPropertyChanged("RedPin");
            }
        }

        [JsonProperty(propertyName: "G")]
        public string GreenPin
        {
            get => _greenPin;
            set
            {
                if (_greenPin == value) return;
                _greenPin = value;
                OnPropertyChanged("GreenPin");
            }
        }

        [JsonProperty(propertyName: "B")]
        public string BluePin
        {
            get => _bluePin;
            set
            {
                if (_bluePin == value) return;
                _bluePin = value;
                OnPropertyChanged("BluePin");
            }
        }

        protected override void OnPropertyChanged(String info)
        {
            base.OnPropertyChanged(info);
        }
    }
}