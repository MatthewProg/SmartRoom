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
    public class MacroColorViewHolder : RecyclerView.ViewHolder, IViewHolder<IMacroItemModel>
    {
        public ImageView CurrentIndicator { get; set; }
        public JaredRummler.Android.ColorPicker.ColorPanelView Picker { get; set; }
        public TextView ColorHex { get; set; }
        public CheckBox Fade { get; set; }
        public TextView Title { get; set; }
        public ImageButton Edit { get; set; }
        public ImageButton Delete { get; set; }
        public IMacroItemModel Model { get; set; }

        public MacroColorViewHolder(View itemView) : base(itemView)
        {
            CurrentIndicator = itemView.FindViewById<ImageView>(Resource.Id.list_item_macro_rgb_current);
            Picker = itemView.FindViewById<JaredRummler.Android.ColorPicker.ColorPanelView>(Resource.Id.list_item_macro_rgb_picker);
            ColorHex = itemView.FindViewById<TextView>(Resource.Id.list_item_macro_rgb_color);
            Fade = itemView.FindViewById<CheckBox>(Resource.Id.list_item_macro_rgb_fade);
            Title = itemView.FindViewById<TextView>(Resource.Id.list_item_macro_rgb_title);
            Edit = itemView.FindViewById<ImageButton>(Resource.Id.list_item_macro_rgb_edit);
            Delete = itemView.FindViewById<ImageButton>(Resource.Id.list_item_macro_rgb_delete);
        }
    }
}