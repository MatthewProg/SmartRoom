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
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public abstract class SwitchModel : IMacroItemModel, IEquatable<SwitchModel>
    {
        private string _title;
        private bool _fade;
        private bool _enabled;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected SwitchModel(SwitchModel model)
        {
            this.Fade = model.Fade;
            this.Enabled = model.Enabled;
            this.Title = model.Title;
        }

        protected SwitchModel()
        {
            ;
        }

        protected SwitchModel(string title, bool fade = false, bool enabled = false)
        {
            _title = title;
            _fade = fade;
            _enabled = enabled;
        }

        public virtual bool Equals(SwitchModel other)
        {
            return (this.Fade == other.Fade &&
                    this.Enabled == other.Enabled &&
                    this.Title == other.Title);
        }

        protected virtual void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public abstract object Clone();

        public abstract IEnumerable<Tuple<string, byte>> GetPinsValue();

        public abstract void PinUpdateListener(object sender, Events.PinValueEventArgs e);

        public abstract string MacroSerialize();

        public abstract IMacroItemModel MacroDeserialize(string json);
    }
}