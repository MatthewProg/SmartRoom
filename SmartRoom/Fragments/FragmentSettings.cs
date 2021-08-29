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
using System.Threading.Tasks;

namespace SmartRoom.Fragments
{
    public class FragmentSettings : Fragment
    {
        private ObservableCollection<Models.ListCellModel> _settings;
        private Task _settingsLoad;
        private FragmentPopupValue _popup;

        public FragmentSettings(Task settingsLoad, ObservableCollection<Models.ListCellModel> settings)
        {
            _settings = settings;
            _settingsLoad = settingsLoad;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.content_settings, container, false);

            if (_settingsLoad.IsCompleted)
                PopulateList(v);
            else
                _settingsLoad.ContinueWith(delegate {
                    Activity.RunOnUiThread(() =>
                    {
                        PopulateList(v);
                    });
                });
            
            return v;
        }

        private void PopulateList(View view)
        {
            view.FindViewById<RelativeLayout>(Resource.Id.settings_loading).Visibility = ViewStates.Gone;

            var listView = view.FindViewById<ListView>(Resource.Id.list_settings);
            listView.Adapter = new Adapters.ListCellAdapter(Activity, _settings);
            listView.ItemClick += ListView_ItemClick;
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var element = _settings[e.Position];
            _popup = new FragmentPopupValue(element);
            _popup.Show(Activity.SupportFragmentManager, "PopupSettings");
        }
    }
}