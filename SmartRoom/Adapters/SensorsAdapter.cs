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
    public class SensorsAdapter : RecyclerView.Adapter
    {
        private ObservableCollection<Models.SensorModel> _sensors;
        private readonly Activity _activity;

        public event EventHandler EditClickEvent;
        public event EventHandler RefreshClickEvent;

        public SensorsAdapter(Activity activity, ObservableCollection<Models.SensorModel> items)
        {
            _activity = activity;
            _sensors = items;
            _sensors.CollectionChanged += SensorsCollectionChanged; ;
            foreach (Models.SensorModel m in _sensors)
                m.PropertyChanged += SensorPropertyChanged; ;
        }

        private void SensorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
               e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Models.SensorModel m in e.OldItems)
                        m.PropertyChanged -= SensorPropertyChanged;
                    _activity.RunOnUiThread(() => NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count));
                }
                if (e.NewItems != null)
                {
                    foreach (Models.SensorModel m in e.NewItems)
                        m.PropertyChanged += SensorPropertyChanged;
                    _activity.RunOnUiThread(() => NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count));
                }
            }
        }
        private void SensorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Title" || e.PropertyName == "Value" || 
                e.PropertyName == "Text" || e.PropertyName == "Display")
            {
                var index = _sensors.IndexOf(sender as Models.SensorModel);
                _activity.RunOnUiThread(() => NotifyItemChanged(index));
            }
        }

        public override int ItemCount => _sensors.Count;

        public override int GetItemViewType(int position)
        {
            var obj = _sensors[position];
            if (obj is Models.ValueSensorModel) return 0;
            else if (obj is Models.TextSensorModel) return 1;

            return -1;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var obj = _sensors[position];
            var r = holder.ItemView.Resources;
            if (obj is Models.ValueSensorModel v)
            {
                var vh = holder as ViewHolders.SensorValueViewHolder;

                vh.Model = v;
                vh.Title.Text = (string.IsNullOrWhiteSpace(v.Title) ? r.GetString(Resource.String.text_untitled) : v.Title);
                if (v.Value != null)
                {
                    vh.Value.Text = v.Display switch
                    {
                        Models.ValueSensorModel.DisplayType.VALUE => v.Value.ToString(),
                        Models.ValueSensorModel.DisplayType.PERCENT => (Math.Round((decimal)(v.Value / 255f)*100)).ToString() + "%",
                        Models.ValueSensorModel.DisplayType.STATE => (v.Value == 255) ? r.GetString(Resource.String.text_state_on) : r.GetString(Resource.String.text_state_off),
                        _ => r.GetString(Resource.String.text_no_data)
                    };
                }
                else
                    vh.Value.Text = r.GetString(Resource.String.text_no_data);
                vh.Text.Text = v.Display switch
                {
                    Models.ValueSensorModel.DisplayType.STATE => r.GetString(Resource.String.text_state),
                    _ => r.GetString(Resource.String.text_value)
                };
            }
            else if (obj is Models.TextSensorModel t)
            {
                var vh = holder as ViewHolders.SensorTextViewHolder;

                vh.Model = t;
                vh.Title.Text = (string.IsNullOrWhiteSpace(t.Title) ? r.GetString(Resource.String.text_untitled) : t.Title);
                vh.Text.Text = (string.IsNullOrWhiteSpace(t.Text) ? r.GetString(Resource.String.text_no_data) : t.Text);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = null;
            if (viewType == 0)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_sensors_value, parent, false);
                var v = new ViewHolders.SensorValueViewHolder(view);
                v.Refresh.Click += delegate { RefreshClick(v, null); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else if (viewType == 1)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_sensors_text, parent, false);
                var v = new ViewHolders.SensorTextViewHolder(view);
                v.Refresh.Click += delegate { RefreshClick(v, null); };
                v.Edit.Click += delegate { EditClick(v, null); };
                v.Delete.Click += delegate { DeleteClick(v, null); };
                return v;
            }
            else
                throw new ArgumentException("Unable to find correct sensor view holder");
        }

        private void RefreshClick(Interfaces.IViewHolder<Models.SensorModel> sender, EventArgs e)
        {
            RefreshClickEvent?.Invoke(sender.Model, null);
        }

        private void DeleteClick(Interfaces.IViewHolder<Models.SensorModel> sender, EventArgs e)
        {
            _sensors.Remove(sender.Model);
        }

        private void EditClick(Interfaces.IViewHolder<Models.SensorModel> sender, EventArgs e)
        {
            EditClickEvent?.Invoke(sender.Model, null);
        }
    }
}