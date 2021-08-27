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
using SmartRoom.Models;
using Android.Views.InputMethods;
using System.Text.RegularExpressions;

namespace SmartRoom.Fragments
{
    public class FragmentPopupSwitch : Extensions.Popup
    {
        private ToggleSwitchModel _model;
        private Events.PopupEventArgs _args;

        public FragmentPopupSwitch() : this(new ToggleSwitchModel()) { ; }

        public FragmentPopupSwitch(ToggleSwitchModel model)
        {
            _model = model;
            _args = new Events.PopupEventArgs();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.popup_toggle, container, false);
            if(_model != null)
            {
                view.FindViewById<EditText>(Resource.Id.popup_toggle_title).Text = _model.Title;
                view.FindViewById<EditText>(Resource.Id.popup_toggle_pin).Text = _model.Pin;
                view.FindViewById<Button>(Resource.Id.popup_toggle_save).Click += SaveClick;
                view.FindViewById<Button>(Resource.Id.popup_toggle_cancel).Click += CancelClick;
            }
            return view;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnPopupClose(this, _args);
        }

        private void CancelClick(object sender, EventArgs e)
        {
            Dialog.Dismiss();
            Dialog.Hide();
        }

        private void SaveClick(object sender, EventArgs e)
        {
            var val = ((Button)sender).RootView;
            if (ValidateView(val))
            {
                _model.Pin = val.FindViewById<EditText>(Resource.Id.popup_toggle_pin).Text.Trim().ToUpper();
                _model.Title = val.FindViewById<EditText>(Resource.Id.popup_toggle_title).Text.Trim();
                _args = new Events.PopupEventArgs(true, _model);
                Dialog.Dismiss();
                Dialog.Hide();
            }
        }

        private bool ValidateView(View val)
        {
            var pin = val.FindViewById<EditText>(Resource.Id.popup_toggle_pin);
            if (pin == null || string.IsNullOrWhiteSpace(pin.Text))
            {
                pin.Error = Resources.GetString(Resource.String.input_empty);
                return false;
            }
            else if (Regex.IsMatch(pin.Text, @"^(([ADad]\d+)|(\d+))$") == false)
            {
                pin.Error = Resources.GetString(Resource.String.input_wrong);
                return false;
            }
            else if (ViewModels.SettingsViewModel.Settings.Pins.Split(',').Contains(pin.Text))
            {
                pin.Error = Resources.GetString(Resource.String.input_pin_restricted);
                return false;
            }

            return true;
        }
    }
}