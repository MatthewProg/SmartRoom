using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Adapters
{
    class SwitchesAdapter : BaseAdapter<Models.SwitchModel>
    {
        private ObservableCollection<Models.SwitchModel> _switches;
        private Activity _context;

        public event EventHandler EditClickEvent;

        public SwitchesAdapter(Activity context, ObservableCollection<Models.SwitchModel> items)
        {
            _context = context;
            _switches = items;
            _switches.CollectionChanged += SwitchesChanged;
        }

        private void SwitchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override Models.SwitchModel this[int position] => _switches[position];
        public override int Count => _switches.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _switches[position];
            View view = convertView; //Can't re-use old view, buttons hold wrong events, maybe fix in future
            if (item is Models.ToggleSwitchModel)
            {
                var e = item as Models.ToggleSwitchModel;

                view = _context.LayoutInflater.Inflate(Resource.Layout.list_item_switches_toggle, null);
                var toggle = view.FindViewById<AndroidX.AppCompat.Widget.SwitchCompat>(Resource.Id.list_item_switches_toggle_switch);
                
                view.FindViewById<TextView>(Resource.Id.list_item_switches_toggle_title).Text = (e.Title != string.Empty ? e.Title : view.Resources.GetString(Resource.String.text_untitled));
                toggle.Checked = e.Toggle;
                toggle.CheckedChange += delegate { Toggle_CheckedChange(e, new CompoundButton.CheckedChangeEventArgs(toggle.Checked)); };
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_toggle_edit).Click += delegate { EditClick(e, null); };
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_toggle_delete).Click += delegate { DeleteClick(e, null); };
            }
            else if(item is Models.SliderSwitchModel)
            {
                var e = item as Models.SliderSwitchModel;

                view = _context.LayoutInflater.Inflate(Resource.Layout.list_item_switches_slider, null);
                var slider = view.FindViewById<SeekBar>(Resource.Id.list_item_switches_slider_value);
                view.FindViewById<TextView>(Resource.Id.list_item_switches_slider_title).Text = (e.Title != string.Empty ? e.Title : view.Resources.GetString(Resource.String.text_untitled));
                slider.Progress = (int)Math.Round(e.Value * 100f);
                slider.ProgressChanged += delegate { Slider_ProgressChanged(e, new SeekBar.ProgressChangedEventArgs(slider, slider.Progress, true)); };
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_slider_edit).Click += delegate { EditClick(e, null); };
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_slider_delete).Click += delegate { DeleteClick(e, null); };
            }
            else if(item is Models.ColorSwitchModel)
            {
                var e = item as Models.ColorSwitchModel;
                var hsv = e.Color.GetHSV();

                view = _context.LayoutInflater.Inflate(Resource.Layout.list_item_switches_rgb, null);
                var slider = view.FindViewById<Rtugeek.ColorSeekBarLib.ColorSeekBar>(Resource.Id.list_item_switches_rgb_slider);

                slider.SetColorSeeds(Resource.Array.hueColors);
                slider.ColorBarPosition = (int)Math.Round(hsv.H);
                slider.AlphaMaxPosition = 100;
                slider.AlphaBarPosition = 100 - (int)Math.Round(hsv.V * 100f);
                slider.ColorChange += delegate { SliderColorChange(e, slider); };
                view.FindViewById<TextView>(Resource.Id.list_item_switches_rgb_title).Text = (e.Title != string.Empty ? e.Title : view.Resources.GetString(Resource.String.text_untitled));
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_rgb_edit).Click += delegate { EditClick(e, null); };
                view.FindViewById<ImageButton>(Resource.Id.list_item_switches_rgb_delete).Click += delegate { DeleteClick(e, null); };

            }
            return view;
        }

        private void Toggle_CheckedChange(Models.ToggleSwitchModel model, CompoundButton.CheckedChangeEventArgs e)
        {
            model.Toggle = e.IsChecked;
        }

        private void Slider_ProgressChanged(Models.SliderSwitchModel model, SeekBar.ProgressChangedEventArgs e)
        {
            model.Value = (float)e.Progress / 100f;
        }


        private void SliderColorChange(Models.ColorSwitchModel model, Rtugeek.ColorSeekBarLib.ColorSeekBar slider)
        {
            model.Color.FromHSV(slider.ColorBarPosition, 1f, ((float)Math.Abs(slider.AlphaBarPosition - 100)) / 100f);
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            _switches.Remove(sender as Models.SwitchModel);
        }

        private void EditClick(object sender, EventArgs e)
        {
            EditClickEvent?.Invoke(sender, null);
        }
    }
}