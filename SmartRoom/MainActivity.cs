using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
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
        private ViewModels.SettingsViewModel _settings;
        private ViewModels.SwitchesViewModel _switches;
        private Task _taskLoadSettings;
        private Task _taskLoadSwitches;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            var mainTransaction = SupportFragmentManager.BeginTransaction();
            mainTransaction.Add(Resource.Id.main_view, new Fragments.FragmentMain(), "Main");
            mainTransaction.Commit();

            _settings = new ViewModels.SettingsViewModel();
            _switches = new ViewModels.SwitchesViewModel();
            _packagesManager = new Managers.PackagesManager(_switches.SwitchesCollection, ViewModels.SettingsViewModel.Settings);
            _taskLoadSettings = Task.Run(async () => await _settings.LoadSettingsAsync());
            _taskLoadSwitches = Task.Run(async () => await _switches.LoadSwitchesAsync());
            Task.Run(() =>
            {
                while(_taskLoadSettings.IsCompleted == false ||
                      _taskLoadSwitches.IsCompleted == false)
                {
                    ; //Idle
                }
                _packagesManager.Connection.Connect(
                    ViewModels.SettingsViewModel.Settings.Address, 
                    ViewModels.SettingsViewModel.Settings.Port);
            });
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
                transaction.Replace(Resource.Id.main_view, new Fragments.FragmentSwitches(_taskLoadSwitches, _switches.SwitchesCollection), "MainContent");
                transaction.Commit();
            }
            else if (id == Resource.Id.nav_sensors)
            {
                var transaction = SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.main_view, new Fragments.FragmentSensors(), "MainContent");
                transaction.Commit();
            }
            else if (id == Resource.Id.nav_settings)
            {
                var transaction = SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.main_view, new Fragments.FragmentSettings(_taskLoadSettings, _settings.SettingsCollection), "MainContent");
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
    }
}

