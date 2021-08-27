using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Adapters
{
    public class ListCellAdapter : BaseAdapter<Models.ListCellModel>
    {
        ObservableCollection<Models.ListCellModel> _items;
        Activity _context;

        public ListCellAdapter(Activity context, ObservableCollection<Models.ListCellModel> items)
        {
            _context = context;
            _items = items;
        }

        public override ListCellModel this[int position] => _items[position];
        public override int Count => _items.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = _context.LayoutInflater.Inflate(Resource.Layout.list_item_setting, null);
            view.FindViewById<TextView>(Resource.Id.title).Text = _context.GetString(item.Title);
            view.FindViewById<TextView>(Resource.Id.subtitle).Text = item.Value;
            return view;
        }
    }
}