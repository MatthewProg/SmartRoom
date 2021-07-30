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

namespace SmartRoom.Models
{
    public class ListCellModel
    {
        public ListCellModel(string title, string description, string value)
        {
            Title = title;
            Description = description;
            Value = value;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
    }
}