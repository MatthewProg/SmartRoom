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
    public class FragmentPopupRgb : Extensions.Popup
    {
        private ColorSwitchModel _model;
        private Events.PopupEventArgs _args;

        public FragmentPopupRgb() : this(new ColorSwitchModel()) { ; }

        public FragmentPopupRgb(ColorSwitchModel model)
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
            var view = inflater.Inflate(Resource.Layout.popup_rgb, container, false);
            if(_model != null)
            {
                view.FindViewById<EditText>(Resource.Id.popup_rgb_title).Text = _model.Title;
                view.FindViewById<EditText>(Resource.Id.popup_rgb_r_pin).Text = _model.RedPin;
                view.FindViewById<EditText>(Resource.Id.popup_rgb_g_pin).Text = _model.GreenPin;
                view.FindViewById<EditText>(Resource.Id.popup_rgb_b_pin).Text = _model.BluePin;
                view.FindViewById<Button>(Resource.Id.popup_rgb_save).Click += SaveClick;
                view.FindViewById<Button>(Resource.Id.popup_rgb_cancel).Click += CancelClick;
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
                _model.RedPin = val.FindViewById<EditText>(Resource.Id.popup_rgb_r_pin).Text.Trim().ToUpper();
                _model.GreenPin = val.FindViewById<EditText>(Resource.Id.popup_rgb_g_pin).Text.Trim().ToUpper();
                _model.BluePin = val.FindViewById<EditText>(Resource.Id.popup_rgb_b_pin).Text.Trim().ToUpper();
                _model.Title = val.FindViewById<EditText>(Resource.Id.popup_rgb_title).Text.Trim();
                _args = new Events.PopupEventArgs(true, _model);
                Dialog.Dismiss();
                Dialog.Hide();
            }
        }

        private bool ValidateView(View val)
        {
            bool output = true;

            bool valid(EditText e)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.Text))
                {
                    e.Error = Resources.GetString(Resource.String.input_empty);
                    return false;
                }
                else if (Regex.IsMatch(e.Text, @"^(([ADad]\d+)|(\d+))$") == false)
                {
                    e.Error = Resources.GetString(Resource.String.input_wrong);
                    return false;
                }
                else if(ViewModels.SettingsViewModel.Settings.Pins.Split(',').Contains(e.Text))
                {
                    e.Error = Resources.GetString(Resource.String.input_pin_restricted);
                    return false;
                }
                return true;
            }

            var rPin = val.FindViewById<EditText>(Resource.Id.popup_rgb_r_pin);
            var gPin = val.FindViewById<EditText>(Resource.Id.popup_rgb_g_pin);
            var bPin = val.FindViewById<EditText>(Resource.Id.popup_rgb_b_pin);

            if (valid(rPin) == false) output = false;
            if (valid(gPin) == false) output = false;
            if (valid(bPin) == false) output = false;

            return output;
        }
    }
}