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
    public class MacroSliderViewHolder : RecyclerView.ViewHolder, IViewHolder<IMacroItemModel>
    {
        public ImageView CurrentIndicator { get; set; }
        public SeekBar Slider { get; set; }
        public CheckBox Fade { get; set; }
        public TextView Title { get; set; }
        public ImageButton Edit { get; set; }
        public ImageButton Delete { get; set; }
        public  IMacroItemModel Model { get; set; }

        public MacroSliderViewHolder(View itemView) : base(itemView)
        {
            CurrentIndicator = itemView.FindViewById<ImageView>(Resource.Id.list_item_macro_slider_current);
            Slider = itemView.FindViewById<SeekBar>(Resource.Id.list_item_macro_slider_value);
            Fade = itemView.FindViewById<CheckBox>(Resource.Id.list_item_macro_slider_fade);
            Title = itemView.FindViewById<TextView>(Resource.Id.list_item_macro_slider_title);
            Edit = itemView.FindViewById<ImageButton>(Resource.Id.list_item_macro_slider_edit);
            Delete = itemView.FindViewById<ImageButton>(Resource.Id.list_item_macro_slider_delete);
        }
    }
}