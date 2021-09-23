using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Adapters
{
    public class MacrosAdapter : BaseAdapter<Models.MacroModel>
    {
        private readonly Managers.MacrosManager _macrosManager;
        private readonly Activity _activity;
        public MacrosAdapter(Activity activity, Managers.MacrosManager macrosManager)
        {
            _activity = activity;
            _macrosManager = macrosManager;
            _macrosManager.Macros.CollectionChanged += Macros_CollectionChanged;
            foreach (Models.MacroModel m in _macrosManager.Macros)
                m.PropertyChanged += MacroPropChanged;
        }

        private void Macros_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
               e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Models.MacroModel m in e.OldItems)
                        m.PropertyChanged -= MacroPropChanged;
                }
                if (e.NewItems != null)
                {
                    foreach (Models.MacroModel m in e.NewItems)
                        m.PropertyChanged += MacroPropChanged;
                }
            }
        }

        private void MacroPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Running")
            {
                _activity.RunOnUiThread(NotifyDataSetChanged); //Change to notify item
            }
        }

        public override MacroModel this[int position] => _macrosManager.Macros[position];

        public override int Count => _macrosManager.Macros.Count;

        public override long GetItemId(int position) => position;

        private Android.Graphics.Color GetColor(int id)
        {
            var col = _activity.GetColor(id);
            var model = new Models.ColorModel();
            model.FromAndroid(col);
            var output = model.GetRGB();
            return Android.Graphics.Color.Rgb(output.R, output.G, output.B);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _macrosManager.Macros[position];
            View view = null;
            if (convertView?.Tag?.JavaCast<Java.Lang.Integer>().IntValue() == item.GetHashCode()) //If same, reuse
                view = convertView;

            bool isNew = false;
            if (view == null)
            {
                view = _activity.LayoutInflater.Inflate(Resource.Layout.list_item_macro, null);
                view.Tag = new Java.Lang.Integer(item.GetHashCode());
                isNew = true;
            }

            var expand = view.FindViewById<CheckBox>(Resource.Id.list_item_macro_expand);
            var title = view.FindViewById<TextView>(Resource.Id.list_item_macro_title);
            var delete = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_delete);
            var edit = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_edit);
            var repeat = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_repeat);
            var stop = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_stop);
            var playpause = view.FindViewById<CheckBox>(Resource.Id.list_item_macro_playpause);
            var items = view.FindViewById<RecyclerView>(Resource.Id.list_item_macro_items);

            if(isNew)
            {
                expand.Checked = false;
                expand.CheckedChange += delegate { Expand_CheckedChange(expand); };
                delete.Click += delegate { Delete_Click(item); };
                edit.Click += delegate { Edit_Click(item); };
                repeat.Click += delegate { Repeat_Click(item, repeat); };
                stop.Click += delegate { Stop_Click(item, stop); };
                playpause.CheckedChange += delegate { Playpause_CheckedChange(item, playpause.Checked); };
                //items.SetAdapter() When ready, add adapter
            }

            title.Text = (item.Name != string.Empty ? item.Name : view.Resources.GetString(Resource.String.text_untitled));
            repeat.SetColorFilter(item.Repeat ? GetColor(Resource.Color.colorAccent) : GetColor(Resource.Color.button_material_dark));
            playpause.Checked = item.Running;

            return view;
        }

        private void Playpause_CheckedChange(Models.MacroModel model, bool play)
        {  
            if (play)
                _macrosManager.StartMacro(model);
            else
                _macrosManager.PauseMacro(model);
        }

        private void Stop_Click(Models.MacroModel model, ImageButton sender)
        {
            _macrosManager.StopMacro(model);
        }

        private void Repeat_Click(Models.MacroModel model, ImageButton sender)
        {
            model.Repeat = !model.Repeat;
            sender.SetColorFilter(model.Repeat ? GetColor(Resource.Color.colorAccent) : GetColor(Resource.Color.button_material_dark));
        }

        private void Edit_Click(Models.MacroModel model)
        {
            throw new NotImplementedException();
        }

        private void Delete_Click(Models.MacroModel model)
        {
            _macrosManager.StopMacro(model);
            _macrosManager.Macros.Remove(model);
        }

        private void Expand_CheckedChange(CheckBox sender)
        {
            var expand = sender.RootView.FindViewById<RecyclerView>(Resource.Id.list_item_macro_items);
            var cardView = sender.RootView.FindViewById<AndroidX.CardView.Widget.CardView>(Resource.Id.list_item_macro_card);
            if(sender.Checked == true)
            {
                AndroidX.Transitions.TransitionManager.BeginDelayedTransition(cardView, new AndroidX.Transitions.AutoTransition());
                expand.Visibility = ViewStates.Visible;
            }
            else
            {
                AndroidX.Transitions.TransitionManager.BeginDelayedTransition(cardView, new AndroidX.Transitions.AutoTransition());
                expand.Visibility = ViewStates.Gone;
            }
        }
    }
}