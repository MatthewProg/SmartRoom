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
using SmartRoom.Interfaces;
using SmartRoom.ViewModels;
using Google.Android.Material.Button;

namespace SmartRoom.Fragments
{
    public class FragmentMain : Fragment
    {
        private Interfaces.ITcpConnector _connector;
        private ViewModels.SettingsViewModel _settings;
        private View _view;

        public FragmentMain(ITcpConnector connector, SettingsViewModel settings)
        {
            _connector = connector;
            _settings = settings;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnPause()
        {
            _connector.ConnectionEvent -= ConnectionEvent;
            base.OnPause();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.content_main, container, false);
            var button = view.FindViewById<MaterialButton>(Resource.Id.main_connect);

            _view = view;

            if (_settings.TaskLoad.IsCompleted == true)
            {
                UpdateSettings(view);
                UpdateStatus(view);
            }
            else
                _settings.TaskLoad.ContinueWith(delegate 
                {
                    Activity?.RunOnUiThread(() => {
                        UpdateSettings(view);
                        UpdateStatus(view);
                    });
                });

            button.Click += ConnectClick;
            _connector.ConnectionEvent += ConnectionEvent;

            return view;
        }

        private void ConnectClick(object sender, EventArgs e)
        {
            _connector.Connect();
        }

        private void ConnectionEvent(object sender, Events.ConnectionStatusEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                var button = _view.FindViewById<MaterialButton>(Resource.Id.main_connect);
                var status = _view.FindViewById<TextView>(Resource.Id.main_status);

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
                        button.Enabled = false;
                        status.Text = Resources.GetString(Resource.String.text_connected);
                        status.SetTextColor(Android.Graphics.Color.Green);
                        break;
                    default:
                        break;
                }
            });
        }

        private void UpdateStatus(View v)
        {
            var button = v.FindViewById<MaterialButton>(Resource.Id.main_connect);
            var status = v.FindViewById<TextView>(Resource.Id.main_status);

            if (_connector.IsConnected)
            {
                button.Enabled = false;
                status.Text = Resources.GetString(Resource.String.text_connected);
                status.SetTextColor(Android.Graphics.Color.Green);
            }
        }

        private void UpdateSettings(View v)
        {
            var server = v.FindViewById<TextView>(Resource.Id.main_server);

            server.Text = SettingsViewModel.Settings.Address + ":" + SettingsViewModel.Settings.Port;
        }
    }
}