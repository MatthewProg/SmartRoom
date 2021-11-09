using Android.App;
using Android.Content;
using Android.Content.Res;
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

namespace SmartRoom.Adapters
{
    public class SwitchesAdapter : RecyclerView.Adapter
    {
        private ObservableCollection<Models.SwitchModel> _switches;
        private readonly Activity _activity;

        public event EventHandler EditClickEvent;

        public SwitchesAdapter(Activity activity, ObservableCollection<Models.SwitchModel> items)
        {
            _activity = activity;
            _switches = items;
            _switches.CollectionChanged += SwitchesChanged;
            foreach (Models.SwitchModel m in _switches)
                m.PropertyChanged += SwitchPropChanged;
        }

        private void SwitchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
               e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Models.SwitchModel m in e.OldItems)
                        m.PropertyChanged -= SwitchPropChanged;
                    _activity?.RunOnUiThread(() => NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count));
                }
                if (e.NewItems != null)
                {
                    foreach (Models.SwitchModel m in e.NewItems)
                        m.PropertyChanged += SwitchPropChanged;
                    _activity?.RunOnUiThread(() => NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count));
                }
            }
        }

        private void SwitchPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled" || e.PropertyName == "Title") //So as not to break animation
            {
                var index = _switches.IndexOf(sender as Models.SwitchModel);
                _activity?.RunOnUiThread(() => NotifyItemChanged(index));
            }
        }

        public override int ItemCount => _switches.Count;

        public override int GetItemViewType(int position)
        {
            var obj = _switches[position];
            if (obj is Models.ToggleSwitchModel) return 0;
            else if (obj is Models.SliderSwitchModel) return 1;
            else if (obj is Models.ColorSwitchModel) return 2;

            return -1;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var obj = _switches[position];
            if (obj is Models.ToggleSwitchModel)
            {
                var e = obj as Models.ToggleSwitchModel;
                var v = holder as ViewHolders.SwitchToggleViewHolder;

                v.Model = e;
                v.Title.Text = (string.IsNullOrWhiteSpace(e.Title) ? holder.ItemView.Resources.GetString(Resource.String.text_untitled) : e.Title);
                v.Toggle.Checked = e.Toggle;
                v.Toggle.Enabled = e.Enabled;
                v.Fade.Checked = e.Fade;             
            }
            else if (obj is Models.SliderSwitchModel) 
            {
                var e = obj as Models.SliderSwitchModel;
                var v = holder as ViewHolders.SwitchSliderViewHolder;

                v.Model = e;
                v.Title.Text = (string.IsNullOrWhiteSpace(e.Title) ? holder.ItemView.Resources.GetString(Resource.String.text_untitled) : e.Title);
                v.Slider.Enabled = e.Enabled;
                v.Slider.Progress = (int)Math.Round(e.Value * 100f);         
                v.Fade.Checked = e.Fade;
            }
            else if (obj is Models.ColorSwitchModel)
            {
                var e = obj as Models.ColorSwitchModel;
                var v = holder as ViewHolders.SwitchColorViewHolder;
                var hsv = e.Color.GetHSV();

                v.Model = e;
                v.Title.Text = (string.IsNullOrWhiteSpace(e.Title) ? holder.ItemView.Resources.GetString(Resource.String.text_untitled) : e.Title);
                v.Slider.Enabled = e.Enabled;
                v.Slider.ColorBarPosition = (int)Math.Round(hsv.H);
                v.Slider.AlphaMaxPosition = 100;
                v.Slider.AlphaBarPosition = 100 - (int)Math.Round(hsv.V * 100f);
                v.Fade.Checked = e.Fade;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = null;
            if (viewType == 0)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_switches_toggle, parent, false);
                var v = new ViewHolders.SwitchToggleViewHolder(view);
                v.Toggle.CheckedChange += delegate { Toggle_CheckedChange(v, new CompoundButton.CheckedChangeEventArgs(v.Toggle.Checked)); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, new CompoundButton.CheckedChangeEventArgs(v.Fade.Checked)); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else if (viewType == 1)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_switches_slider, parent, false);
                var v = new ViewHolders.SwitchSliderViewHolder(view);
                v.Slider.ProgressChanged += delegate { Slider_ProgressChanged(v, new SeekBar.ProgressChangedEventArgs(v.Slider, v.Slider.Progress, true)); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, new CompoundButton.CheckedChangeEventArgs(v.Fade.Checked)); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else if (viewType == 2)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_switches_rgb, parent, false);
                var v = new ViewHolders.SwitchColorViewHolder(view);
                v.Slider.SetColorSeeds(Resource.Array.hueColors);
                v.Slider.ColorChange += delegate { SliderColorChange(v, v.Slider); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, new CompoundButton.CheckedChangeEventArgs(v.Fade.Checked)); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else
                throw new ArgumentException("Unable to find correct switch view holder");
        }

        private void FadeChanged(Interfaces.IViewHolder<Models.SwitchModel> model, CompoundButton.CheckedChangeEventArgs e)
        {
            model.Model.Fade = e.IsChecked;
        }

        private void Toggle_CheckedChange(Interfaces.IViewHolder<Models.SwitchModel> model, CompoundButton.CheckedChangeEventArgs e)
        {
            var m = model.Model as Models.ToggleSwitchModel;
            m.Toggle = e.IsChecked;
        }

        private void Slider_ProgressChanged(Interfaces.IViewHolder<Models.SwitchModel> model, SeekBar.ProgressChangedEventArgs e)
        {
            var m = model.Model as Models.SliderSwitchModel;
            m.Value = (float)e.Progress / 100f;
        }

        private void SliderColorChange(Interfaces.IViewHolder<Models.SwitchModel> model, Rtugeek.ColorSeekBarLib.ColorSeekBar slider)
        {
            var m = model.Model as Models.ColorSwitchModel;
            m.Color.FromHSV(slider.ColorBarPosition, 1f, ((float)Math.Abs(slider.AlphaBarPosition - 100)) / 100f);
        }

        private void DeleteClick(Interfaces.IViewHolder<Models.SwitchModel> sender, EventArgs e)
        {
            _switches.Remove(sender.Model);
        }

        private void EditClick(Interfaces.IViewHolder<Models.SwitchModel> sender, EventArgs e)
        {
            EditClickEvent?.Invoke(sender.Model, null);
        }
    }
}