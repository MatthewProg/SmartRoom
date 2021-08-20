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
        private DialogFragment _popup;

        public FragmentSwitches()
        {
            _popup = null;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.content_switches, container, false);

            var fabMenu = new Fragments.FragmentSwitchesFabMenu();
            fabMenu.ShowDialog += FabMenu_ShowDialog;

            var transaction = Activity.SupportFragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.frame_switches_fab, fabMenu, "FabMenu");
            transaction.Commit();

            return view;
        }

        private void FabMenu_ShowDialog(object sender, EventArgs e)
        {
            _popup = sender as DialogFragment;
            _popup?.Show(Activity.SupportFragmentManager, "PopupAddSwitch");
        }
    }
}