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
    public class SwitchSliderViewHolder : RecyclerView.ViewHolder, IViewHolder<Models.SwitchModel>
    {
        public SeekBar Slider { get; set; }
        public CheckBox Fade { get; set; }
        public TextView Title { get; set; }
        public ImageButton Edit { get; set; }
        public ImageButton Delete { get; set; }
        public Models.SwitchModel Model { get; set; }

        public SwitchSliderViewHolder(View itemView) : base(itemView)
        {
            Slider = itemView.FindViewById<SeekBar>(Resource.Id.list_item_switches_slider_value);
            Fade = itemView.FindViewById<CheckBox>(Resource.Id.list_item_switches_slider_fade);
            Title = itemView.FindViewById<TextView>(Resource.Id.list_item_switches_slider_title);
            Edit = itemView.FindViewById<ImageButton>(Resource.Id.list_item_switches_slider_edit);
            Delete = itemView.FindViewById<ImageButton>(Resource.Id.list_item_switches_slider_delete);
        }
    }
}