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
    public class ObservableCollectionCallback<T> : ItemTouchHelper.SimpleCallback
    {
        private ObservableCollection<T> _items;
        public event EventHandler<Events.CallbackMoveEventArgs> Move;
        public ObservableCollectionCallback(ObservableCollection<T> items) : base(ItemTouchHelper.Up | ItemTouchHelper.Down | ItemTouchHelper.Start | ItemTouchHelper.End, 0)
        {
            _items = items;
        }
        public override bool OnMove(RecyclerView p0, RecyclerView.ViewHolder p1, RecyclerView.ViewHolder p2)
        {
            int from = p1.AdapterPosition;
            int to = p2.AdapterPosition;
            _items.Move(from, to);
            p0.GetAdapter().NotifyItemMoved(from, to);
            Move?.Invoke(this, new Events.CallbackMoveEventArgs(p0, p1, p2));
            return false;
        }

        public override void OnSwiped(RecyclerView.ViewHolder p0, int p1)
        {
            ; //Unused
        }
    }
}