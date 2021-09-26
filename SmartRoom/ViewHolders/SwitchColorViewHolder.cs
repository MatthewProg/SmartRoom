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
    public class SwitchColorViewHolder : RecyclerView.ViewHolder, IViewHolder<Models.SwitchModel>
    {
        public Rtugeek.ColorSeekBarLib.ColorSeekBar Slider { get; set; }
        public CheckBox Fade { get; set; }
        public TextView Title { get; set; }
        public ImageButton Edit { get; set; }
        public ImageButton Delete { get; set; }
        public Models.SwitchModel Model { get; set; }

        public SwitchColorViewHolder(View itemView) : base(itemView)
        {
            Slider = itemView.FindViewById<Rtugeek.ColorSeekBarLib.ColorSeekBar>(Resource.Id.list_item_switches_rgb_slider);
            Fade = itemView.FindViewById<CheckBox>(Resource.Id.list_item_switches_rgb_fade);
            Title = itemView.FindViewById<TextView>(Resource.Id.list_item_switches_rgb_title);
            Edit = itemView.FindViewById<ImageButton>(Resource.Id.list_item_switches_rgb_edit);
            Delete = itemView.FindViewById<ImageButton>(Resource.Id.list_item_switches_rgb_delete);
        }
    }
}