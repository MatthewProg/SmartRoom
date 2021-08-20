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
        private FragmentPopupValue _popup;

        public FragmentSettings(ObservableCollection<Models.ListCellModel> settings)
        {
            _settings = settings;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.content_settings, container, false);
            var listView = v.FindViewById<ListView>(Resource.Id.list_settings);

            listView.Adapter = new Adapters.ListCellAdapter(Activity, _settings);
            listView.ItemClick += ListView_ItemClick;
            return v;
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var element = _settings[e.Position];
            _popup = new FragmentPopupValue(element);
            _popup.Show(Activity.SupportFragmentManager, "PopupSettings");
        }
    }
}