using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;

namespace SmartRoom
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Managers.PackagesManager _packagesManager;
        private Managers.MacrosManager _macrosManager;
        private Interfaces.ITcpConnector _tcpConnector;
        private ViewModels.SettingsViewModel _settings;
        private ViewModels.SwitchesViewModel _switches;
        private ViewModels.SensorsViewModel _sensors;
        private ViewModels.MacrosViewModel _macros;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view_menu);
            navigationView.SetNavigationItemSelectedListener(this);

            _tcpConnector = new Connectors.TcpConnector();
            _packagesManager = new Managers.PackagesManager(_tcpConnector);
            _settings = new ViewModels.SettingsViewModel();
            _switches = new ViewModels.SwitchesViewModel(_packagesManager);
            _sensors = new ViewModels.SensorsViewModel(_packagesManager);
            _macros = new ViewModels.MacrosViewModel();
            _macrosManager = new Managers.MacrosManager(_packagesManager, _macros);
            _settings.LoadModelAsync();
            _switches.LoadModelAsync();
            _sensors.LoadModelAsync();
            _macros.LoadModelAsync();
            Task.WhenAll(_switches.TaskLoad, _settings.TaskLoad).ContinueWith(delegate
            {
                _packagesManager.Connection.Connect(
                    ViewModels.SettingsViewModel.Settings.Address,
                    ViewModels.SettingsViewModel.Settings.Port);
            });

            _tcpConnector.ConnectionEvent += (s,e) => RunOnUiThread(() => ConnectionEvent(e));
            FindViewById<Button>(Resource.Id.nav_reconnect).Click += delegate
            {
                _tcpConnector.Connect(ViewModels.SettingsViewModel.Settings.Address,
                                      ViewModels.SettingsViewModel.Settings.Port);
            };

            var mainTransaction = SupportFragmentManager.BeginTransaction();
            mainTransaction.Add(Resource.Id.main_view, new Fragments.FragmentMain(_tcpConnector, _settings), "Main");
            mainTransaction.Commit();
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_reconnect)
            {
                if(_packagesManager != null)
                {
                    _packagesManager.Connection.Close();
                    _packagesManager.Connection.Connect(ViewModels.SettingsViewModel.Settings.Address, ViewModels.SettingsViewModel.Settings.Port);
                }
                return true;
            }
            else if(id == Resource.Id.action_exit)
            {
                this.FinishAffinity();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_switches)
            {
                var transaction = SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.main_view, new Fragments.FragmentSwitches(_switches, _packagesManager), "MainContent");
                transaction.Commit();
            }
            else if (id == Resource.Id.nav_macros)
            {
                var transaction = SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.main_view, new Fragments.FragmentMacros(_macrosManager, _switches), "MainContent");
                transaction.Commit();
            }
            else if (id == Resource.Id.nav_sensors)
            {
                var transaction = SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.main_view, new Fragments.FragmentSensors(_packagesManager, _sensors), "MainContent");
                transaction.Commit();
            }
            else if (id == Resource.Id.nav_settings)
            {
                var transaction = SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.main_view, new Fragments.FragmentSettings(_settings), "MainContent");
                transaction.Commit();
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ConnectionEvent(Events.ConnectionStatusEventArgs e)
        {
            var button = FindViewById<Button>(Resource.Id.nav_reconnect);
            var status = FindViewById<TextView>(Resource.Id.nav_status);

            switch (e.Status)
            {
                case Events.ConnectionStatusEventArgs.ConnectionStatus.FAILED:
                case Events.ConnectionStatusEventArgs.ConnectionStatus.DISCONNECTED:
                    button.Enabled = true;
                    status.Text = Resources.GetString(Resource.String.text_disconnected);
                    status.SetTextColor(Android.Graphics.Color.Red);
                    break;
                case Events.ConnectionStatusEventArgs.ConnectionStatus.CONNECTING:
                    button.Enabled = false;
                    status.Text = Resources.GetString(Resource.String.text_connecting);
                    status.SetTextColor(Android.Graphics.Color.Orange);
                    break;
                case Events.ConnectionStatusEventArgs.ConnectionStatus.CONNECTED:
                    button.Enabled = true;
                    status.Text = Resources.GetString(Resource.String.text_connected);
                    status.SetTextColor(Android.Graphics.Color.Green);
                    break;
                default:
                    break;
            }
        }
    }
}

