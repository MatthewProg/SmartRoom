using AndroidX.Fragment.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.RecyclerView.Widget;
using SmartRoom.Interfaces;
using SmartRoom.Managers;
using SmartRoom.ViewModels;
using System.Threading.Tasks;

namespace SmartRoom.Fragments
{
    public class FragmentSensors : Fragment
    {
        private Managers.SensorsManager _sensorsManager;
        private ViewModels.SensorsViewModel _sensors;
        private Extensions.Popup _popup;
        private Task _refreshTask;
        private View _view;

        public FragmentSensors(IPackagesManager packagesManager, SensorsViewModel sensors)
        {
            _popup = null;
            _sensors = sensors;
            _sensors.Sensors.CollectionChanged += Sensors_CollectionChanged;
            _sensorsManager = new SensorsManager(packagesManager, sensors);
        }

        private void Sensors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
               e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                UpdateTitleVisibity(_view);
        }

        public override void OnResume()
        {
            Refresh_Refresh(_view.FindViewById<SwipeRefreshLayout>(Resource.Id.sensors_refresh), null);
            base.OnResume();
        }

        public override void OnPause()
        {
            _sensorsManager.PauseAll();
            base.OnPause();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        private void Fab_Click(object sender, EventArgs e)
        {
            View view = (View)sender;
            _popup = new FragmentPopupSensor();
            _popup.PopupClose += PopupClose;
            _popup.Show(Activity.SupportFragmentManager, "SensorPopup");
        }

        private void PopupClose(object sender, Events.PopupEventArgs e)
        {
            if (e.HasResult)
                _sensors.Sensors.Add(e.Result as Models.SensorModel);
        }

        private void LoadSensors(View v)
        {
            if (v == null)
                return;

            var list = v.FindViewById<RecyclerView>(Resource.Id.sensors_list);
            
            var loading = v.FindViewById<RelativeLayout>(Resource.Id.sensors_loading);
            var fab = v.FindViewById<FloatingActionButton>(Resource.Id.fab_sensors);

            var adapter = new Adapters.SensorsAdapter(this.Activity, _sensors.Sensors);
            adapter.EditClickEvent += Adapter_EditClickEvent;
            adapter.RefreshClickEvent += Adapter_RefreshClickEvent;

            list.SetLayoutManager(new LinearLayoutManager(Context));
            list.AddItemDecoration(new DividerItemDecoration(Context, DividerItemDecoration.Vertical));
            list.SetAdapter(adapter);

            var touchHelper = new ItemTouchHelper(new Callbacks.ObservableCollectionCallback<Models.SensorModel>(_sensors.Sensors));
            touchHelper.AttachToRecyclerView(list);

            loading.Visibility = ViewStates.Gone;
            fab.Visibility = ViewStates.Visible;
            UpdateTitleVisibity(v);
            Refresh_Refresh(v.FindViewById<SwipeRefreshLayout>(Resource.Id.sensors_refresh), null);
        }

        private void UpdateTitleVisibity(View v)
        {
            if (v == null) return;

            Activity?.RunOnUiThread(() =>
            {
                var title = v.FindViewById<TextView>(Resource.Id.sensors_title);
                if (_sensors.Sensors.Count == 0)
                    title.Visibility = ViewStates.Visible;
                else
                    title.Visibility = ViewStates.Gone;
            });
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.content_sensors, container, false);
            _view = view;

            var fab = view.FindViewById<FloatingActionButton>(Resource.Id.fab_sensors);
            var refresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.sensors_refresh);

            refresh.Refresh += Refresh_Refresh;
            fab.Click += Fab_Click;
            fab.Visibility = ViewStates.Gone;

            if (_sensors.TaskLoad.IsCompleted == false)
                _sensors.TaskLoad.ContinueWith(delegate
                {
                    Activity.RunOnUiThread(() => LoadSensors(view));
                });
            else
                LoadSensors(view);

            return view;
        }

        private void Refresh_Refresh(object sender, EventArgs e)
        {
            var r = sender as SwipeRefreshLayout;
            if (r == null || _refreshTask?.Status == TaskStatus.Running || _sensors.Sensors.Count == 0 || _sensors.TaskLoad.IsCompleted == false)
                return;

            _refreshTask = Task.Run(async () =>
            {
                foreach (var s in _sensors.Sensors)
                    _sensorsManager.RefreshSensor(s);

                await Task.Delay(500);
                Activity?.RunOnUiThread(() => { r.Refreshing = false; });
            });
        }

        private void Adapter_RefreshClickEvent(object sender, EventArgs e)
        {
            _sensorsManager.RefreshSensor(sender as Models.SensorModel);
        }

        private void Adapter_EditClickEvent(object sender, EventArgs e)
        {
            _popup = new FragmentPopupSensor(sender as Models.SensorModel);
            _popup?.Show(this.Activity.SupportFragmentManager, "SensorPopup");
        }
    }
}