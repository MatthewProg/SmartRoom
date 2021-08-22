using AndroidX.Fragment.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Extensions
{
    public abstract class Popup : DialogFragment
    {
        public event EventHandler<Events.PopupEventArgs> PopupClose;

        protected virtual void OnPopupClose(object sender, Events.PopupEventArgs e)
        {
            PopupClose?.Invoke(sender, e);
        }
        public abstract override void OnDismiss(IDialogInterface dialog);
    }
}