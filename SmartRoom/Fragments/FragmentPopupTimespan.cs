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
    public class FragmentPopupTimespan : Extensions.Popup
    {
        private TimeSpan _model;
        private Events.PopupEventArgs _args;

        public FragmentPopupTimespan()
        {
            _model = TimeSpan.FromSeconds(10);
            _args = new Events.PopupEventArgs();
        }

        public FragmentPopupTimespan(TimeSpan time) : this()
        {
            _model = time;
        }
        public FragmentPopupTimespan(string time, string format, IFormatProvider provider) : this()
        {
            TimeSpan.TryParseExact(time, format, provider, out _model);
            _args = new Events.PopupEventArgs();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.popup_timespan, container, false);
            if(_model != null)
            {
                view.FindViewById<EditText>(Resource.Id.popup_timespan_h).Text = _model.Hours.ToString("D2");
                view.FindViewById<EditText>(Resource.Id.popup_timespan_m).Text = _model.Minutes.ToString("D2");
                view.FindViewById<EditText>(Resource.Id.popup_timespan_s).Text = _model.Seconds.ToString("D2");
                view.FindViewById<EditText>(Resource.Id.popup_timespan_f).Text = new string(_model.Milliseconds.ToString("D3").Take(3).ToArray());
                view.FindViewById<Button>(Resource.Id.popup_timespan_ok).Click += OkClick; ;
                view.FindViewById<Button>(Resource.Id.popup_timespan_cancel).Click += CancelClick;
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

        private void OkClick(object sender, EventArgs e)
        {
            var val = ((Button)sender).RootView;
            if (ValidateView(val))
            {
                var h = int.Parse(val.FindViewById<EditText>(Resource.Id.popup_timespan_h).Text);
                var m = int.Parse(val.FindViewById<EditText>(Resource.Id.popup_timespan_m).Text);
                var s = int.Parse(val.FindViewById<EditText>(Resource.Id.popup_timespan_s).Text);
                var f = int.Parse(val.FindViewById<EditText>(Resource.Id.popup_timespan_f).Text);
                _model = new TimeSpan(0, h, m, s, f);
                _args = new Events.PopupEventArgs(true, _model);
                Dialog.Dismiss();
                Dialog.Hide();
            }
        }

        private bool ValidateView(View v)
        {
            var values = new List<EditText>()
            {
                v.FindViewById<EditText>(Resource.Id.popup_timespan_h),
                v.FindViewById<EditText>(Resource.Id.popup_timespan_m),
                v.FindViewById<EditText>(Resource.Id.popup_timespan_s),
                v.FindViewById<EditText>(Resource.Id.popup_timespan_f)
            };

            int sum = 0;
            foreach (var p in values)
            {
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    p.Error = Resources.GetString(Resource.String.input_empty);
                    return false;
                }
                else if (int.TryParse(p.Text, out int res) == false)
                {
                    p.Error = Resources.GetString(Resource.String.input_wrong);
                    return false;
                }
                else
                {
                    bool wrong = false;
                    if (p == values[0] && (res < 0 || res > 24)) wrong = true;
                    else if (p == values[1] && (res < 0 || res > 59)) wrong = true;
                    else if (p == values[2] && (res < 0 || res > 59)) wrong = true;
                    else if (p == values[3] && (res < 0 || res > 999)) wrong = true;

                    if (wrong)
                    {
                        p.Error = Resources.GetString(Resource.String.input_wrong);
                        return false;
                    }
                    if (p == values[3])
                        sum += res;
                    else if(res != 0)
                        sum += 1000;
                }
            }

            if(sum < 250)
            {
                values[3].Error = Resources.GetString(Resource.String.input_low_time);
                return false;
            }
            return true;
        }
    }
}