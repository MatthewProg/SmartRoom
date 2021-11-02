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
    public class ObjectEventArgs : EventArgs
    {
        public object Object { get; set; }
        public ObjectEventArgs() : this(null)
        {
            ;
        }
        public ObjectEventArgs(object obj)
        {
            Object = obj;
        }
    }
}