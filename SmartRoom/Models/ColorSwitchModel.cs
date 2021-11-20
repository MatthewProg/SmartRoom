using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartRoom.Events;
using SmartRoom.Interfaces;
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

        public ColorSwitchModel() : base()
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

        protected ColorSwitchModel(ColorSwitchModel m) : base(m)
        {
            this.RedPin = m.RedPin;
            this.GreenPin = m.GreenPin;
            this.BluePin = m.BluePin;
            this.Color = (ColorModel)m.Color.Clone();
        }

        public ColorSwitchModel(string title, string redPin, string greenPin, string bluePin, ColorModel color, bool fade = false, bool enabled = false) : base(title, fade, enabled)
        {
            RedPin = redPin;
            GreenPin = greenPin;
            BluePin = bluePin;
            Color = color;
        }

        public override object Clone()
        {
            return new ColorSwitchModel(this);
        }

        public override bool Equals(SwitchModel other)
        {
            if (other is ColorSwitchModel == false)
                return false;

            var obj = other as ColorSwitchModel;
            return (this.RedPin == obj.RedPin &&
                    this.GreenPin == obj.GreenPin &&
                    this.BluePin == obj.BluePin &&
                    this.Color.GetRGB().Equals(obj.Color.GetRGB()) == true &&
                    base.Equals(other) == true);
        }

        protected override void OnPropertyChanged(String info)
        {
            base.OnPropertyChanged(info);
        }

        public override string MacroSerialize()
        {
            var obj = JToken.FromObject(this);
            JObject o = (JObject)obj;
            JProperty color = new JProperty("C", Color.GetHex());
            JProperty enabled = new JProperty("E", Enabled);
            o.AddFirst(color);
            o.AddFirst(enabled);
            o.Add("F", Fade);
            return o.ToString();
        }

        public override IMacroItemModel MacroDeserialize(string json)
        {
            var o = JObject.Parse(json);
            Color.FromHex(o.Value<string>("C"));
            this.Fade = o.Value<bool>("F");
            this.Enabled = o.Value<bool>("E");
            o.Remove("E");
            o.Remove("C");
            o.Remove("F");
            var obj = o.ToObject<ColorSwitchModel>();
            this.RedPin = obj.RedPin;
            this.GreenPin = obj.GreenPin;
            this.BluePin = obj.BluePin;
            this.Title = obj.Title;
            return this;
        }

        public override IEnumerable<Tuple<string, byte>> GetPinsValue()
            => new List<Tuple<string, byte>>()
            {
                new Tuple<string, byte>(this.RedPin,   _color.GetRGB().R),
                new Tuple<string, byte>(this.GreenPin, _color.GetRGB().G),
                new Tuple<string, byte>(this.BluePin,  _color.GetRGB().B)
            };

        public override void PinUpdateListener(object sender, PinValueEventArgs e)
        {
            if(this.RedPin == e.Pin || this.GreenPin == e.Pin || this.BluePin == e.Pin)
            {
                this.Enabled = true;
                var col = this.Color.GetRGB();

                if (this.RedPin == e.Pin) col.R = e.Value;
                else if (this.GreenPin == e.Pin) col.G = e.Value;
                else col.B = e.Value;
                    
                this.Color.FromRGB(col);
            }
        }
    }
}