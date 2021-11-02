using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.ImageView;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Adapters
{
    public class ListItemIconAdapter : BaseAdapter<Models.ListItemIconModel>
    {
        private readonly FragmentActivity _activity;
        private readonly IEnumerable<Models.ListItemIconModel> _items;

        public ListItemIconAdapter(FragmentActivity activity, IEnumerable<ListItemIconModel> items)
        {
            _activity = activity;
            _items = items;
        }

        public override ListItemIconModel this[int position] => _items.ElementAt(position);

        public override int Count => _items.Count();

        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = this[position];
            View view = convertView;

            if (view == null)
                view = _activity.LayoutInflater.Inflate(Resource.Layout.list_item_icon, null, false);

            var title = view.FindViewById<TextView>(Resource.Id.list_item_icon_title);
            var subtitle = view.FindViewById<TextView>(Resource.Id.list_item_icon_subtitle);
            var icon = view.FindViewById<ShapeableImageView>(Resource.Id.list_item_icon_icon);

            title.Text = (string.IsNullOrWhiteSpace(item.Title) ? view.Resources.GetString(Resource.String.text_untitled) : item.Title);
            subtitle.Text = (string.IsNullOrWhiteSpace(item.Subtitle) == false ? item.Subtitle : view.Resources.GetString(Resource.String.text_untitled));
            subtitle.Visibility = (string.IsNullOrWhiteSpace(item.Subtitle) == false ? ViewStates.Visible : ViewStates.Gone);
            icon.SetImageResource(item.Icon);

            return view;
        }
    }
}