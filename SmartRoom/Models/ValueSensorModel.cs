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
    public class ValueSensorModel : SensorModel
    {
        public enum DisplayType
        {
            VALUE, PERCENT, STATE
        }

        private DisplayType _display;
        private byte? _value;

        public ValueSensorModel() : base()
        {
            ;
        }

        public ValueSensorModel(string title, string input, bool isId, DisplayType display = DisplayType.VALUE, TimeSpan? refresh = null) : base(title, input, isId, refresh)
        {
            _display = display;
        }

        protected ValueSensorModel(ValueSensorModel other) : base(other)
        {
            this.Value = other.Value;
        }

        [JsonProperty(propertyName: "D")]
        public DisplayType Display
        {
            get => _display;
            set
            {
                if (_display == value) return;

                _display = value;
                OnPropertyChanged(this, "Display");
            }
        }

        [JsonIgnore]
        public byte? Value
        {
            get => _value;
            set
            {
                if (_value == value) return;

                _value = value;
                OnPropertyChanged(this, "Value");
            }
        }

        protected override void OnPropertyChanged(SensorModel model, string info)
        {
            base.OnPropertyChanged(model, info);
        }

        public override bool Equals(SensorModel other)
        {
            if(other is ValueSensorModel m)
            {
                return (m.Value == this.Value &&
                       base.Equals(m) == true);
            }
            return false;
        }

        public override object Clone()
        {
            return new ValueSensorModel(this);
        }

        public override void UpdateListener(object sender, EventArgs e)
        {
            if(this.IsId == false && e is Events.PinValueEventArgs p)
            {
                if (this.Input == p.Pin)
                    this.Value = p.Value;
            }
            else if (this.IsId == true && e is Events.IdValuesEventArgs a)
            {
                if (a.Id == this.Input && a.IsText == false)
                    this.Value = (byte?)a.Value;
            }
        }
    }
}