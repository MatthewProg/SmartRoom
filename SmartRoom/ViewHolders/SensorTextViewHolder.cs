using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using SmartRoom.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.ViewHolders
{
    public class SensorTextViewHolder : RecyclerView.ViewHolder, IViewHolder<Models.SensorModel>
    {
        public TextView Text { get; set; }
        public TextView Title { get; set; }
        public ImageButton Edit { get; set; }
        public ImageButton Delete { get; set; }
        public ImageButton Refresh { get; set; }
        public Models.SensorModel Model { get; set; }

        public SensorTextViewHolder(View itemView) : base(itemView)
        {
            Text = itemView.FindViewById<TextView>(Resource.Id.list_item_sensors_text_text);
            Title = itemView.FindViewById<TextView>(Resource.Id.list_item_sensors_text_title);
            Edit = itemView.FindViewById<ImageButton>(Resource.Id.list_item_sensors_text_edit);
            Delete = itemView.FindViewById<ImageButton>(Resource.Id.list_item_sensors_text_delete);
            Refresh = itemView.FindViewById<ImageButton>(Resource.Id.list_item_sensors_text_refresh);
        }
    }
}