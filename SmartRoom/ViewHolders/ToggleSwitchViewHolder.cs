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
    public class ToggleSwitchViewHolder : RecyclerView.ViewHolder, ISwitchViewHolder
    {
        public AndroidX.AppCompat.Widget.SwitchCompat Toggle { get; set; }
        public CheckBox Fade { get; set; }
        public TextView Title { get; set; }
        public ImageButton Edit { get; set; }
        public ImageButton Delete { get; set; }
        public Models.SwitchModel Model { get; set; }

        public ToggleSwitchViewHolder(View itemView) : base(itemView)
        {
            Toggle = itemView.FindViewById<AndroidX.AppCompat.Widget.SwitchCompat>(Resource.Id.list_item_switches_toggle_switch);
            Fade = itemView.FindViewById<CheckBox>(Resource.Id.list_item_switches_toggle_fade);
            Title = itemView.FindViewById<TextView>(Resource.Id.list_item_switches_toggle_title);
            Edit = itemView.FindViewById<ImageButton>(Resource.Id.list_item_switches_toggle_edit);
            Delete = itemView.FindViewById<ImageButton>(Resource.Id.list_item_switches_toggle_delete);
        }
    }
}