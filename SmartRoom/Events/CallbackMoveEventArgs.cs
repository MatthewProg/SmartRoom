using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Events
{
    public class CallbackMoveEventArgs : EventArgs
    {
        public RecyclerView View { get; private set; }
        public RecyclerView.ViewHolder From { get; private set; }
        public RecyclerView.ViewHolder To { get; private set; }

        public CallbackMoveEventArgs(RecyclerView view, RecyclerView.ViewHolder from, RecyclerView.ViewHolder to)
        {
            View = view;
            From = from;
            To = to;
        }
    }
}