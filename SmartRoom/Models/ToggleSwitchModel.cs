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

        public ToggleSwitchModel()
        {
            ;
        }

        protected ToggleSwitchModel(ToggleSwitchModel m) : base(m)
        {
            this.Pin = m.Pin;
            this.Toggle = m.Toggle;
        }

        public ToggleSwitchModel(string title, string pin, bool toggle, bool fade = false, bool enabled = false) : base(title, fade, enabled)
        {
            _pin = pin;
            _toggle = toggle;
        }

        public override object Clone()
        {
            return new ToggleSwitchModel(this);
        }

        public override bool Equals(SwitchModel other)
        {
            if (other is ToggleSwitchModel == false)
                return false;

            var obj = other as ToggleSwitchModel;
            return (this.Toggle == obj.Toggle &&
                    this.Pin == obj.Pin &&      
                    base.Equals(other) == true);
        }

        protected override void OnPropertyChanged(String info)
        {
            base.OnPropertyChanged(info);
        }

        public override string MacroSerialize()
        {
            JObject o = JObject.FromObject(this);
            o.Add("F", Fade);
            o.Add("Tg", Toggle);
            o.Add("E", Enabled);
            return o.ToString();
        }

        public override IMacroItemModel MacroDeserialize(string json)
        {
            JObject o = JObject.Parse(json);
            this.Fade = o.Value<bool>("F");
            this.Toggle = o.Value<bool>("Tg");
            this.Enabled = o.Value<bool>("E");
            o.Remove("F");
            o.Remove("Tg");
            o.Remove("E");
            var obj = JsonConvert.DeserializeObject<ToggleSwitchModel>(json);
            this.Pin = obj.Pin;
            this.Title = obj.Title;
            return this;
        }

        public override IEnumerable<Tuple<string, byte>> GetPinsValue()
            => new List<Tuple<string, byte>>()
            {
                new Tuple<string, byte>(this.Pin, (byte)(this.Toggle ? 255 : 0))
            };

        public override void PinUpdateListener(object sender, PinValueEventArgs e)
        {
            if(this.Pin == e.Pin)
            {
                this.Enabled = true;
                this.Toggle = (e.Value == 255);
            }
        }
    }
}