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
using System.Collections.ObjectModel;

namespace SmartRoom.Fragments
{
    public class FragmentSettings : Fragment
    {
        private ObservableCollection<Models.ListCellModel> _settings;
        private FragmentValuePopup _popup;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _settings = new ObservableCollection<Models.ListCellModel>
            {
                new Models.ListCellModel("IP Address", "Server address", "192.168.4.1"),
                new Models.ListCellModel("Port", "Listener port", "23"),
                new Models.ListCellModel("Restricted pins", "Pins used by hardware", "A0, A1, D13")
            };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.content_settings, container, false);
            var listView = v.FindViewById<ListView>(Resource.Id.list_settings);

            listView.Adapter = new Adapters.SettingsAdapter(Activity, _settings);
            listView.ItemClick += ListView_ItemClick;
            return v;
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var element = _settings[e.Position];
            _popup = new FragmentValuePopup(element);
            _popup.Show(Activity.SupportFragmentManager, "PopupSettings");
        }
    }
}