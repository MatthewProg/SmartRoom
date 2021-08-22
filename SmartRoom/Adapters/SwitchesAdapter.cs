using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Adapters
{
    class SwitchesAdapter : BaseAdapter<Models.SwitchModel>
    {
        private ObservableCollection<Models.SwitchModel> _switches;
        private Activity _context;

        public event EventHandler EditClickEvent;

        public SwitchesAdapter(Activity context, ObservableCollection<Models.SwitchModel> items)
        {
            _context = context;
            _switches = items;
            _switches.CollectionChanged += SwitchesChanged;
        }

        private void SwitchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override Models.SwitchModel this[int position] => _switches[position];
        public override int Count => _switches.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _switches[position];
            View view = convertView; //Can't re-use old view, buttons hold wrong events, maybe fix in future
            if (item is Models.ToggleSwitchModel)
            {
                var e = item as Models.ToggleSwitchModel;
                view = _context.LayoutInflater.Inflate(Resource.Layout.list_item_switches_toggle, null);
                view.FindViewById<TextView>(Resource.Id.list_item_switches_toggle_title).Text = e.Title;
                view.FindViewById<AndroidX.AppCompat.Widget.SwitchCompat>(Resource.Id.list_item_switches_toggle_switch).Checked = e.Toggle;
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_toggle_edit).Click += delegate { EditClick(e, null); };
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_toggle_delete).Click += delegate { DeleteClick(e, null); };
            }
            return view;
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            _switches.Remove(sender as Models.SwitchModel);
        }

        private void EditClick(object sender, EventArgs e)
        {
            EditClickEvent?.Invoke(sender, null);
        }
    }
}