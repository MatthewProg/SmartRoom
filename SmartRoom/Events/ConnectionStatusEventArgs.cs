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
    public class ConnectionStatusEventArgs : EventArgs
    {
        public enum ConnectionStatus
        {
            DISCONNECTED, CONNECTING, CONNECTED, FAILED
        }

        public ConnectionStatus Status { get; set; }

        public ConnectionStatusEventArgs() : this(ConnectionStatus.DISCONNECTED)
        {
            ;
        }
        public ConnectionStatusEventArgs(ConnectionStatus status)
        {
            Status = status;
        }
    }
}