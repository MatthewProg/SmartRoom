using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
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
        private readonly ObservableCollection<Models.SwitchModel> _switches;
        private readonly Managers.MacrosManager _macrosManager;
        private readonly FragmentActivity _activity;
        private Extensions.Popup _popup;

        public MacrosAdapter(FragmentActivity activity, Managers.MacrosManager macrosManager, ObservableCollection<Models.SwitchModel> switches)
        {
            _activity = activity;
            _switches = switches;
            _macrosManager = macrosManager;
            _macrosManager.MacrosViewModel.Macros.CollectionChanged += Macros_CollectionChanged;
            foreach (Models.MacroModel m in _macrosManager.MacrosViewModel.Macros)
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
                _activity?.RunOnUiThread(NotifyDataSetChanged); //Change to notify item
            }
        }

        private void MacroPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Running")
            {
                _activity?.RunOnUiThread(NotifyDataSetChanged); //Change to notify item
            }
        }

        public override MacroModel this[int position] => _macrosManager.MacrosViewModel.Macros[position];

        public override int Count => _macrosManager.MacrosViewModel.Macros.Count;

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
            var item = _macrosManager.MacrosViewModel.Macros[position];
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
            var expandLayout = view.FindViewById<LinearLayout>(Resource.Id.list_item_macro_expand_list);
            var cardView = view.FindViewById<AndroidX.CardView.Widget.CardView>(Resource.Id.list_item_macro_card);
            var title = view.FindViewById<TextView>(Resource.Id.list_item_macro_title);
            var delete = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_delete);
            var edit = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_edit);
            var repeat = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_repeat);
            var stop = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_stop);
            var playpause = view.FindViewById<CheckBox>(Resource.Id.list_item_macro_playpause);
            var items = view.FindViewById<RecyclerView>(Resource.Id.list_item_macro_items);
            var add = view.FindViewById<ImageButton>(Resource.Id.list_item_macro_add);

            if (isNew)
            {
                expand.Checked = false;
                expand.CheckedChange += delegate { Expand_CheckedChange(expandLayout, cardView, expand.Checked); };
                delete.Click += delegate { Delete_Click(item); };
                edit.Click += delegate { Edit_Click(item); };
                repeat.Click += delegate { Repeat_Click(item, repeat); };
                stop.Click += delegate { Stop_Click(item, stop); };
                playpause.CheckedChange += delegate { Playpause_CheckedChange(item, playpause.Checked); };
                add.Click += delegate { Add_Click(item); };
                var decor = new DividerItemDecoration(_activity, DividerItemDecoration.Vertical);
                var callback = new Callbacks.ObservableCollectionCallback<Interfaces.IMacroItemModel>(item.Items);
                var ith = new ItemTouchHelper(callback);
                items.SetAdapter(new MacroItemsAdapter(_activity, _macrosManager, item));
                items.SetLayoutManager(new LinearLayoutManager(_activity));
                items.AddItemDecoration(decor);
                ith.AttachToRecyclerView(items);
            }

            title.Text = (string.IsNullOrWhiteSpace(item.Name) ? view.Resources.GetString(Resource.String.text_untitled) : item.Name);
            repeat.SetColorFilter(item.Repeat ? GetColor(Resource.Color.colorAccent) : GetColor(Resource.Color.button_material_dark));
            playpause.Checked = item.Running;

            return view;
        }

        private void Add_Click(Models.MacroModel model)
        {
            _popup = new Fragments.FragmentPopupMacroAddSelector(_macrosManager.MacrosViewModel.Macros, _switches);
            _popup.PopupClose += (o, e) =>
            {
                if (e.HasResult)
                    model.Items.Add(e.Result as Interfaces.IMacroItemModel);
            };
            _popup?.Show(_activity.SupportFragmentManager, "PopupAddMacro");
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
            _popup = new Fragments.FragmentPopupValue(new Models.ListCellModel(null,
                                                Resource.String.popup_title_title,
                                                Resource.String.popup_description_title,
                                                model.Name, 
                                                Android.Text.InputTypes.TextFlagAutoCorrect, "", 0));
            _popup.PopupClose += (o, e) =>
            {
                if (e.HasResult == true)
                    model.Name = (e.Result as ListCellModel).Value;
            };
            _popup.Show(_activity.SupportFragmentManager, "PopupEditMacro");
        }

        private void Delete_Click(Models.MacroModel model)
        {
            _macrosManager.StopMacro(model);
            _macrosManager.MacrosViewModel.Macros.Remove(model);
        }

        private void Expand_CheckedChange(LinearLayout expand, AndroidX.CardView.Widget.CardView cardView, bool check)
        {
            if(check == true)
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