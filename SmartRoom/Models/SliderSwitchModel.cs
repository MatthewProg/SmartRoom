using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SmartRoom.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class SliderSwitchModel : SwitchModel
    {
        private float _value;
        private string _pin;

        [JsonIgnore]
        public float Value
        {
            get => _value;
            set 
            {
                var v = Math.Clamp(value, 0f, 100f);
                if (_value == v) return;
                _value = v;
                OnPropertyChanged("Value");
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

        public SliderSwitchModel()
        {
            ;
        }

        protected SliderSwitchModel(SliderSwitchModel m) : base(m)
        {
            this.Pin = m.Pin;
            this.Value = m.Value;
        }

        public override object Clone()
        {
            return new SliderSwitchModel(this);
        }

        public override bool Equals(SwitchModel other)
        {
            if (other is SliderSwitchModel == false)
                return false;

            var obj = other as SliderSwitchModel;
            return (this.Value == obj.Value &&
                    this.Pin == obj.Pin &&
                    base.Equals(other) == true);
        }

        protected override void OnPropertyChanged(String info)
        {
            base.OnPropertyChanged(info);
        }
    }
}