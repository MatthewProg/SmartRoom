using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartRoom.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class DelayMacroItemModel : Interfaces.IMacroItemModel
    {
        private string _title;
        private bool _enabled;
        private int _delay;

        [JsonProperty(PropertyName = "T")]
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

        [JsonProperty(PropertyName = "D")]
        public int Delay
        {
            get => _delay;
            set
            {
                if (_delay == value) return;

                _delay = value;
                OnPropertyChanged("Delay");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DelayMacroItemModel()
        {
            ;
        }

        protected DelayMacroItemModel(DelayMacroItemModel m)
        {
            this.Delay = m.Delay;
            this.Enabled = m.Enabled;
            this.Title = m.Title;
        }

        public object Clone()
        {
            return new DelayMacroItemModel(this);
        }

        protected virtual void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public string MacroSerialize()
        {
            JObject o = JObject.FromObject(this);
            o.Add("E", Enabled);
            return o.ToString();
        }

        public IMacroItemModel MacroDeserialize(string json)
        {
            JObject o = JObject.Parse(json);
            this.Enabled = o.Value<bool>("E");
            o.Remove("E");
            var obj = JsonConvert.DeserializeObject<DelayMacroItemModel>(json);
            this.Delay = obj.Delay;
            this.Title = obj.Title;
            return this;
        }
    }
}