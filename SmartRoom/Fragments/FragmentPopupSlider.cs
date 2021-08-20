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
    public class FragmentPopupSlider : DialogFragment
    {
        private SliderSwitchModel _model;

        public FragmentPopupSlider()
        {
            _model = new SliderSwitchModel();
        }

        public FragmentPopupSlider(SliderSwitchModel model)
        {
            _model = model;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.popup_slider, container, false);
            if(_model != null)
            {
                view.FindViewById<EditText>(Resource.Id.popup_slider_title).Text = _model.Title;
                view.FindViewById<EditText>(Resource.Id.popup_slider_pin).Text = _model.Pin;
                view.FindViewById<Button>(Resource.Id.popup_slider_save).Click += SaveClick;
                view.FindViewById<Button>(Resource.Id.popup_slider_cancel).Click += CancelClick;
            }
            return view;
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
                _model.Pin = val.FindViewById<EditText>(Resource.Id.popup_slider_pin).Text.Trim().ToUpper();
                _model.Title = val.FindViewById<EditText>(Resource.Id.popup_slider_title).Text;
                Dialog.Dismiss();
                Dialog.Hide();
            }
        }

        private bool ValidateView(View val)
        {
            var pin = val.FindViewById<EditText>(Resource.Id.popup_slider_pin);
            if (pin == null || string.IsNullOrWhiteSpace(pin.Text))
            {
                pin.Error = Resources.GetString(Resource.String.input_empty);
                return false;
            }
            if (Regex.IsMatch(pin.Text, @"^(([ADad]\d+)|(\d+))$") == false)
            {
                pin.Error = Resources.GetString(Resource.String.input_wrong);
                return false;
            }
            return true;
        }
    }
}