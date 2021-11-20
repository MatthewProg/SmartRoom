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
    public class DelayMacroItemModel : IMacroItemModel
    {
        private string _title;
        private bool _enabled;
        private int _delay;
        private bool _executing;

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

        [JsonIgnore]
        public bool Executing
        {
            get => _executing;
            set
            {
                if(_executing == value) return;

                _executing = value;
                OnPropertyChanged("Executing");
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
            this.Executing = m.Executing;
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