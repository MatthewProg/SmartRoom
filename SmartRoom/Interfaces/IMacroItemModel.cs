using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Interfaces
{
    public interface IMacroItemModel : INotifyPropertyChanged, ICloneable
    {
        public string Title { get; set; }
        public bool Enabled { get; set; }
        public bool Executing { get; set; }

        public string MacroSerialize();
        public IMacroItemModel MacroDeserialize(string json);
    }
}