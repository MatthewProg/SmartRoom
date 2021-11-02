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
    public class TextSensorModel : SensorModel
    {
        private string _text;

        public TextSensorModel() : base()
        {
            ;
        }

        public TextSensorModel(string title, string input, bool isId, TimeSpan? refresh = null) : base(title, input, isId, refresh)
        {
        }

        protected TextSensorModel(TextSensorModel other) : base(other)
        {
            this.Text = other.Text;
        }

        [JsonIgnore]
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;

                _text = value;
                OnPropertyChanged(this, "Text");
            }
        }

        protected override void OnPropertyChanged(SensorModel model, string info)
        {
            base.OnPropertyChanged(model, info);
        }

        public override bool Equals(SensorModel other)
        {
            if(other is TextSensorModel m)
            {
                return (m.Text == this.Text &&
                       base.Equals(m) == true);
            }
            return false;
        }

        public override object Clone()
        {
            return new TextSensorModel(this);
        }

        public override void UpdateListener(object sender, EventArgs e)
        {
            if (this.IsId == true && e is Events.IdValuesEventArgs a)
            {
                if (a.Id == this.Input && a.IsText == true)
                    this.Text = a.Value as string;
            }
        }
    }
}