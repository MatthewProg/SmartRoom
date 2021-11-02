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
    public abstract class SensorModel : INotifyPropertyChanged, IEquatable<SensorModel>, ICloneable
    {
        private string _title;
        private string _input;
        private bool _isId;
        private TimeSpan _refresh;

        [JsonProperty(propertyName: "T")]
        public string Title
        {
            get => _title;
            set 
            {
                if (_title == value) return;

                _title = value;
                OnPropertyChanged(this, "Title");
            }
        }

        [JsonProperty(propertyName: "I")]
        public string Input
        {
            get => _input;
            set
            {
                if (_input == value) return;

                _input = value;
                OnPropertyChanged(this, "Input");
            }
        }

        [JsonProperty(propertyName: "D")]
        public bool IsId
        {
            get => _isId;
            set
            {
                if (_isId == value) return;

                _isId = value;
                OnPropertyChanged(this, "IsId");
            }
        }

        [JsonProperty(propertyName: "R")]
        public TimeSpan Refresh
        {
            get => _refresh;
            set
            {
                if (_refresh == value) return;

                _refresh = value;
                OnPropertyChanged(this, "Refresh");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SensorModel()
        {
            ;
        }
        public SensorModel(string title, string input, bool isId, TimeSpan? refresh = null)
        {
            Title = title;
            Input = input;
            IsId = isId;
            Refresh = (TimeSpan)((refresh is null) ? TimeSpan.FromSeconds(10) : refresh);
        }

        protected SensorModel(SensorModel other)
        {
            this.Title = other.Title;
            this.Input = other.Input;
            this.IsId = other.IsId;
            this.Refresh = other.Refresh;
        }


        protected virtual void OnPropertyChanged(SensorModel model, string info)
            => PropertyChanged?.Invoke(model, new PropertyChangedEventArgs(info));

        public virtual bool Equals(SensorModel other)
        {
            return (other.Title == this.Title &&
                    other.Input == this.Input &&
                    other.IsId == this.IsId &&
                    other.Refresh == this.Refresh);
        }

        public abstract object Clone();

        public abstract void UpdateListener(object sender, EventArgs e);
    }
}