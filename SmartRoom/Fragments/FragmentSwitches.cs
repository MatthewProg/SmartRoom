using AndroidX.Fragment.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;

namespace SmartRoom.Fragments
{
    public class FragmentSwitches : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        private void Fab_Click(object sender, EventArgs e)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Add switches", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.content_switches, container, false);

            var fab = view.FindViewById<FloatingActionButton>(Resource.Id.fab_switches);
            if (fab != null)
                fab.Click += Fab_Click;

            return view;
        }
    }
}