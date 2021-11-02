using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using JaredRummler.Android.ColorPicker;
using SmartRoom.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Adapters
{
    class MacroItemsAdapter : RecyclerView.Adapter
    {
        private readonly FragmentActivity _activity;
        private readonly Managers.MacrosManager _macrosManager;
        private Fragments.FragmentPopupColorPicker _popup;
        public Models.MacroModel Macro { get; private set; }

        public MacroItemsAdapter(FragmentActivity activity, Managers.MacrosManager macrosManager, Models.MacroModel macro)
        {
            _popup = null;
            _activity = activity;
            _macrosManager = macrosManager;
            Macro = macro;
            Macro.Items.CollectionChanged += ItemsChanged;
            foreach (Interfaces.IMacroItemModel m in Macro.Items)
                m.PropertyChanged += ItemPropChanged;
        }

        private void ItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _macrosManager.StopMacro(Macro); //If sth happens with collection, stop executing

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
               e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Interfaces.IMacroItemModel m in e.OldItems)
                        m.PropertyChanged -= ItemPropChanged;
                    _activity.RunOnUiThread(() => NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count));
                }
                if (e.NewItems != null)
                {
                    foreach (Interfaces.IMacroItemModel m in e.NewItems)
                        m.PropertyChanged += ItemPropChanged;
                    _activity.RunOnUiThread(() => NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count));
                }
            }
        }

        private void ItemPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled" || e.PropertyName == "Title" || e.PropertyName == "Color")
            {
                var index = Macro.Items.IndexOf(sender as Interfaces.IMacroItemModel);
                _activity.RunOnUiThread(() => NotifyItemChanged(index));
            }
        }

        public override int ItemCount => Macro.Items.Count;

        public override int GetItemViewType(int position)
        {
            var obj = Macro.Items[position];
            if (obj is Models.ToggleSwitchModel) return 0;
            else if (obj is Models.SliderSwitchModel) return 1;
            else if (obj is Models.ColorSwitchModel) return 2;
            else if (obj is Models.DelayMacroItemModel) return 3;

            return -1;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = null;
            if (viewType == 0)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_macro_toggle, parent, false);
                var v = new ViewHolders.MacroToggleViewHolder(view);
                v.Toggle.CheckedChange += delegate { Toggle_CheckedChange(v, v.Toggle.Checked); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, v.Fade.Checked); };
                v.Edit.Click += delegate { EditClick(v); };
                v.Delete.Click += delegate { DeleteClick(v); };
                return v;
            }
            else if (viewType == 1)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_macro_slider, parent, false);
                var v = new ViewHolders.MacroSliderViewHolder(view);
                v.Slider.ProgressChanged += delegate { Slider_ProgressChanged(v, v.Slider.Progress); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, v.Fade.Checked); };
                v.Edit.Click += delegate { EditClick(v); };
                v.Delete.Click += delegate { DeleteClick(v); };
                return v;
            }
            else if (viewType == 2)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_macro_rgb, parent, false);
                var v = new ViewHolders.MacroColorViewHolder(view);
                v.Picker.Click += delegate { Picker_Click(v, v.Picker.Color); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, v.Fade.Checked); };
                v.Edit.Click += delegate { EditClick(v); };
                v.Delete.Click += delegate { DeleteClick(v); };
                return v;
            }
            else if(viewType == 3)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_macro_delay, parent, false);
                var v = new ViewHolders.MacroDelayViewHolder(view);
                v.Value.AfterTextChanged += delegate { ValueChanged(v, v.Value.Text); };
                v.Delete.Click += delegate { DeleteClick(v); };
                return v;
            }
            else
                throw new ArgumentException("Unable to find correct macro item view");
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var obj = Macro.Items[position];
            if (obj is Models.ToggleSwitchModel)
            {
                var e = obj as Models.ToggleSwitchModel;
                var v = holder as ViewHolders.MacroToggleViewHolder;

                v.Model = e;
                v.Title.Text = (string.IsNullOrWhiteSpace(e.Title) ? holder.ItemView.Resources.GetString(Resource.String.text_untitled) : e.Title);
                v.Toggle.Checked = e.Toggle;
                v.Toggle.Enabled = e.Enabled;
                v.Fade.Checked = e.Fade;
            }
            else if (obj is Models.SliderSwitchModel)
            {
                var e = obj as Models.SliderSwitchModel;
                var v = holder as ViewHolders.MacroSliderViewHolder;

                v.Model = e;
                v.Title.Text = (string.IsNullOrWhiteSpace(e.Title) ? holder.ItemView.Resources.GetString(Resource.String.text_untitled) : e.Title);
                v.Slider.Enabled = e.Enabled;
                v.Slider.Progress = (int)Math.Round(e.Value * 100f);
                v.Fade.Checked = e.Fade;
            }
            else if (obj is Models.ColorSwitchModel)
            {
                var e = obj as Models.ColorSwitchModel;
                var v = holder as ViewHolders.MacroColorViewHolder;

                v.Model = e;
                v.Title.Text = (string.IsNullOrWhiteSpace(e.Title) ? holder.ItemView.Resources.GetString(Resource.String.text_untitled) : e.Title);
                v.Picker.Enabled = e.Enabled;
                v.Picker.Color = e.Color.GetAndroid();
                v.ColorHex.Text = e.Color.GetHex();
                v.Fade.Checked = e.Fade;
            }
            else if(obj is Models.DelayMacroItemModel)
            {
                var e = obj as Models.DelayMacroItemModel;
                var v = holder as ViewHolders.MacroDelayViewHolder;

                v.Model = e;
                v.Value.Enabled = e.Enabled;
                v.Value.Text = e.Delay.ToString();
            }
        }

        private void ValueChanged(IViewHolder<IMacroItemModel> v, string text)
        {
            int value = 0;
            if (string.IsNullOrWhiteSpace(text) == false)
                value = int.Parse(text);
            (v.Model as Models.DelayMacroItemModel).Delay = value;
        }

        private void Picker_Click(IViewHolder<IMacroItemModel> v, int color)
        {
            _popup = new Fragments.FragmentPopupColorPicker((v.Model as Models.ColorSwitchModel).Color, color);
            _popup.Show(_activity.SupportFragmentManager, "PopupColorPicker");
        }

        private void Slider_ProgressChanged(IViewHolder<IMacroItemModel> v, int progress)
        {
            (v.Model as Models.SliderSwitchModel).Value = (float)progress / 100f;
        }

        private void DeleteClick(IViewHolder<IMacroItemModel> v)
        {
            Macro.Items.Remove(v.Model);
        }

        private void EditClick(IViewHolder<IMacroItemModel> v)
        {
            Extensions.Popup popup = null;
            if (v.Model is Models.ToggleSwitchModel) popup = new Fragments.FragmentPopupSwitch(v.Model as Models.ToggleSwitchModel);
            else if (v.Model is Models.SliderSwitchModel) popup = new Fragments.FragmentPopupSlider(v.Model as Models.SliderSwitchModel);
            else if (v.Model is Models.ColorSwitchModel) popup = new Fragments.FragmentPopupRgb(v.Model as Models.ColorSwitchModel);

            popup?.Show(_activity.SupportFragmentManager, "PopupEditMacro");
        }

        private void Toggle_CheckedChange(IViewHolder<IMacroItemModel> v, bool check)
        {
            (v.Model as Models.ToggleSwitchModel).Toggle = check;
        }
        private void FadeChanged(IViewHolder<IMacroItemModel> v, bool check)
        {
            (v.Model as Models.SwitchModel).Fade = check;
        }
    }
}