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
using System.Collections.ObjectModel;

namespace SmartRoom.Fragments
{
    public class FragmentSwitches : Fragment
    {
        private Extensions.Popup _popup;
        private ObservableCollection<Models.SwitchModel> _switches;
        private View _view;

        public FragmentSwitches(ObservableCollection<Models.SwitchModel> switches)
        {
            _popup = null;
            _view = null;
            _switches = switches;
            _switches.CollectionChanged += SwitchesChanged;
        }

        private void SwitchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTitleVisibility(_view);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = inflater.Inflate(Resource.Layout.content_switches, container, false);

            UpdateTitleVisibility(_view);

            var adapter = new Adapters.SwitchesAdapter(Activity, _switches);
            adapter.EditClickEvent += ListViewItemEdit;
            _view.FindViewById<ListView>(Resource.Id.switches_list).Adapter = adapter;

            var fabMenu = new Fragments.FragmentSwitchesFabMenu();
            fabMenu.ShowDialog += FabMenu_ShowDialog;

            var transaction = Activity.SupportFragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.frame_switches_fab, fabMenu, "FabMenu");
            transaction.Commit();

            return _view;
        }

        private void ListViewItemEdit(object sender, EventArgs e)
        {
            if (sender is Models.ToggleSwitchModel)
                _popup = new FragmentPopupSwitch(sender as Models.ToggleSwitchModel);
            else if (sender is Models.SliderSwitchModel)
                _popup = new FragmentPopupSlider(sender as Models.SliderSwitchModel);
            else if (sender is Models.ColorSwitchModel)
                _popup = new FragmentPopupRgb(sender as Models.ColorSwitchModel);

            _popup?.Show(Activity.SupportFragmentManager, "PopupEditSwitch");
        }

        private void UpdateTitleVisibility(View view)
        {
            if (_switches.Count > 0) 
                view.FindViewById<TextView>(Resource.Id.switches_title).Visibility = ViewStates.Gone;
            else
                view.FindViewById<TextView>(Resource.Id.switches_title).Visibility = ViewStates.Visible;
        }

        private void FabMenu_ShowDialog(object sender, EventArgs e)
        {
            _popup = sender as Extensions.Popup;
            _popup.PopupClose += PopupClose;
            _popup?.Show(Activity.SupportFragmentManager, "PopupAddSwitch");
        }

        private void PopupClose(object sender, Events.PopupEventArgs e)
        {
            if(e.HasResult)
            {
                _switches.Add(e.Result as Models.SwitchModel);
            }
        }
    }
}