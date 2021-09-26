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
    public class MacroDelayViewHolder : RecyclerView.ViewHolder, IViewHolder<IMacroItemModel>
    {
        public ImageView CurrentIndicator { get; set; }
        public ImageButton Delete { get; set; }
        public EditText Value { get; set; }
        public  IMacroItemModel Model { get; set; }

        public MacroDelayViewHolder(View itemView) : base(itemView)
        {
            CurrentIndicator = itemView.FindViewById<ImageView>(Resource.Id.list_item_macro_delay_current);
            Delete = itemView.FindViewById<ImageButton>(Resource.Id.list_item_macro_delay_delete);
            Value = itemView.FindViewById<EditText>(Resource.Id.list_item_macro_delay_value);
        }
    }
}