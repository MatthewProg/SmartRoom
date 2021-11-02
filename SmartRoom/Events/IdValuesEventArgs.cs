using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Events
{
    public class IdValuesEventArgs : EventArgs
    {
        public string Id { get; set; }
        public bool IsText { get; set; }
        public object Value { get; set; }

        public IdValuesEventArgs()
        {
            ;
        }

        public IdValuesEventArgs(string id, bool isText, object value)
        {
            Id = id;
            IsText = isText;
            Value = value;
        }
    }
}