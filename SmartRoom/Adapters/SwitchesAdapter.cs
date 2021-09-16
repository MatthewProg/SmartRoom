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

        public event EventHandler EditClickEvent;

        public SwitchesAdapter(ObservableCollection<Models.SwitchModel> items)
        {
            _switches = items;
            _switches.CollectionChanged += SwitchesChanged;
        }

        private void SwitchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
               e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                NotifyDataSetChanged();
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
                var v = holder as ViewHolders.ToggleSwitchViewHolder;

                v.Model = e;
                v.Title.Text = (e.Title != string.Empty ? e.Title : holder.ItemView.Resources.GetString(Resource.String.text_untitled));
                v.Toggle.Checked = e.Toggle;
                v.Toggle.Enabled = e.Enabled;
                v.Fade.Checked = e.Fade;             
            }
            else if (obj is Models.SliderSwitchModel) 
            {
                var e = obj as Models.SliderSwitchModel;
                var v = holder as ViewHolders.SliderSwitchViewHolder;

                v.Model = e;
                v.Title.Text = (e.Title != string.Empty ? e.Title : holder.ItemView.Resources.GetString(Resource.String.text_untitled));
                v.Slider.Enabled = e.Enabled;
                v.Slider.Progress = (int)Math.Round(e.Value * 100f);         
                v.Fade.Checked = e.Fade;
            }
            else if (obj is Models.ColorSwitchModel)
            {
                var e = obj as Models.ColorSwitchModel;
                var v = holder as ViewHolders.ColorSwitchViewHolder;
                var hsv = e.Color.GetHSV();

                v.Model = e;
                v.Title.Text = (e.Title != string.Empty ? e.Title : holder.ItemView.Resources.GetString(Resource.String.text_untitled));
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
                var v = new ViewHolders.ToggleSwitchViewHolder(view);
                v.Toggle.CheckedChange += delegate { Toggle_CheckedChange(v, new CompoundButton.CheckedChangeEventArgs(v.Toggle.Checked)); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, new CompoundButton.CheckedChangeEventArgs(v.Fade.Checked)); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else if (viewType == 1)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_switches_slider, parent, false);
                var v = new ViewHolders.SliderSwitchViewHolder(view);
                v.Slider.ProgressChanged += delegate { Slider_ProgressChanged(v, new SeekBar.ProgressChangedEventArgs(v.Slider, v.Slider.Progress, true)); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, new CompoundButton.CheckedChangeEventArgs(v.Fade.Checked)); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else if (viewType == 2)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_switches_rgb, parent, false);
                var v = new ViewHolders.ColorSwitchViewHolder(view);
                v.Slider.SetColorSeeds(Resource.Array.hueColors);
                v.Slider.ColorChange += delegate { SliderColorChange(v, v.Slider); };
                v.Fade.CheckedChange += delegate { FadeChanged(v, new CompoundButton.CheckedChangeEventArgs(v.Fade.Checked)); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else
                throw new ArgumentException("Unable to find correct switch view");
        }

        private void FadeChanged(Interfaces.ISwitchViewHolder model, CompoundButton.CheckedChangeEventArgs e)
        {
            model.Model.Fade = e.IsChecked;
        }

        private void Toggle_CheckedChange(Interfaces.ISwitchViewHolder model, CompoundButton.CheckedChangeEventArgs e)
        {
            var m = model.Model as Models.ToggleSwitchModel;
            m.Toggle = e.IsChecked;
        }

        private void Slider_ProgressChanged(Interfaces.ISwitchViewHolder model, SeekBar.ProgressChangedEventArgs e)
        {
            var m = model.Model as Models.SliderSwitchModel;
            m.Value = (float)e.Progress / 100f;
        }

        private void SliderColorChange(Interfaces.ISwitchViewHolder model, Rtugeek.ColorSeekBarLib.ColorSeekBar slider)
        {
            var m = model.Model as Models.ColorSwitchModel;
            m.Color.FromHSV(slider.ColorBarPosition, 1f, ((float)Math.Abs(slider.AlphaBarPosition - 100)) / 100f);
        }

        private void DeleteClick(Interfaces.ISwitchViewHolder sender, EventArgs e)
        {
            _switches.Remove(sender.Model);
        }

        private void EditClick(Interfaces.ISwitchViewHolder sender, EventArgs e)
        {
            EditClickEvent?.Invoke(sender.Model, null);
        }
    }
}