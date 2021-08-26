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

        public FragmentSwitches()
        {
            _popup = null;
            _switches = new ObservableCollection<Models.SwitchModel>() //TMP, ONLY FOR TESTS
            {
                new Models.ToggleSwitchModel()
                {
                    Title = "Test toggle",
                    Pin = "D10",
                    Toggle = false
                },
                new Models.SliderSwitchModel()
                {
                    Title = "Test slider",
                    Pin = "D6",
                    Value = 0.5f
                },
                new Models.ColorSwitchModel()
                {
                    Title = "Test RGB",
                    RedPin = "D1",
                    GreenPin = "D2",
                    BluePin = "D3",
                    Color = new Models.ColorModel(255,0,0)
                }
            };
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.content_switches, container, false);

            UpdateTitleVisibility(view);

            var adapter = new Adapters.SwitchesAdapter(Activity, _switches);
            adapter.EditClickEvent += ListViewItemEdit;
            view.FindViewById<ListView>(Resource.Id.switches_list).Adapter = adapter;

            var fabMenu = new Fragments.FragmentSwitchesFabMenu();
            fabMenu.ShowDialog += FabMenu_ShowDialog;

            var transaction = Activity.SupportFragmentManager.BeginTransaction();
            transaction.Add(Resource.Id.frame_switches_fab, fabMenu, "FabMenu");
            transaction.Commit();

            return view;
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