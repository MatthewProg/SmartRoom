using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Callbacks
{
    public class SwitchesCallback : ItemTouchHelper.SimpleCallback
    {
        private ObservableCollection<Models.SwitchModel> _switches;
        public SwitchesCallback(ObservableCollection<Models.SwitchModel> switches) : base(ItemTouchHelper.Up | ItemTouchHelper.Down | ItemTouchHelper.Start | ItemTouchHelper.End, 0)
        {
            _switches = switches;
        }
        public override bool OnMove(RecyclerView p0, RecyclerView.ViewHolder p1, RecyclerView.ViewHolder p2)
        {
            int from = p1.AdapterPosition;
            int to = p2.AdapterPosition;
            _switches.Move(from, to);
            p0.GetAdapter().NotifyItemMoved(from, to);
            return false;
        }

        public override void OnSwiped(RecyclerView.ViewHolder p0, int p1)
        {
            ; //Unused
        }
    }
}