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
        private Task _refreshTask;

        public FragmentSensors(IPackagesManager packagesManager, SensorsViewModel sensors)
        {
            _sensors = sensors;
            _sensorsManager = new SensorsManager(packagesManager, sensors);
        }

        public override void OnResume()
        {
            Refresh_Refresh(Activity.FindViewById<SwipeRefreshLayout>(Resource.Id.sensors_refresh), null);
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
            Snackbar.Make(view, "Add sensors", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.content_sensors, container, false);

            var fab = view.FindViewById<FloatingActionButton>(Resource.Id.fab_sensors);
            var title = view.FindViewById<TextView>(Resource.Id.sensors_title);
            var refresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.sensors_refresh);
            var list = view.FindViewById<RecyclerView>(Resource.Id.sensors_list);

            refresh.Refresh += Refresh_Refresh;

            var adapter = new Adapters.SensorsAdapter(this.Activity, _sensors.Sensors);
            adapter.EditClickEvent += Adapter_EditClickEvent;
            adapter.RefreshClickEvent += Adapter_RefreshClickEvent;

            list.SetLayoutManager(new LinearLayoutManager(Context));
            list.AddItemDecoration(new DividerItemDecoration(Context, DividerItemDecoration.Vertical));
            list.SetAdapter(adapter);

            var touchHelper = new ItemTouchHelper(new Callbacks.ObservableCollectionCallback<Models.SensorModel>(_sensors.Sensors));
            touchHelper.AttachToRecyclerView(list);

            fab.Click += Fab_Click;

            return view;
        }

        private void Refresh_Refresh(object sender, EventArgs e)
        {
            var r = sender as SwipeRefreshLayout;
            if (r == null || _refreshTask?.Status == TaskStatus.Running)
                return;

            _refreshTask = Task.Run(async () =>
            {
                foreach (var s in _sensors.Sensors)
                    _sensorsManager.RefreshSensor(s);

                await Task.Delay(500);
                Activity.RunOnUiThread(() => { r.Refreshing = false; });
            });
        }

        private void Adapter_RefreshClickEvent(object sender, EventArgs e)
        {
            _sensorsManager.RefreshSensor(sender as Models.SensorModel);
        }

        private void Adapter_EditClickEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException(); //Show popup and edit
        }
    }
}