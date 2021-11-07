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
using System.Globalization;

namespace SmartRoom.Fragments
{
    public class FragmentPopupSensor : Extensions.Popup
    {
        private SensorModel _model;
        private Events.PopupEventArgs _args;
        private HashSet<RadioButton> _radios;
        private Extensions.Popup _popup;
        private View _view;

        public FragmentPopupSensor() 
        {
            _model = null;
            _args = new Events.PopupEventArgs();
            _radios = new HashSet<RadioButton>();
            _popup = null;
            _view = null;
        }

        public FragmentPopupSensor(SensorModel model) : this()
        {
            _model = model;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.popup_sensor, container, false);
            var s = view.FindViewById<TextView>(Resource.Id.popup_sensor_refresh);
            _view = view;
            _radios = new HashSet<RadioButton>()
            {
                view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_text),
                view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_value),
                view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_percent),
                view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_state)
            };
            foreach (var r in _radios)
                r.CheckedChange += CheckedChange;

            if(_model != null)
            {
                view.FindViewById<EditText>(Resource.Id.popup_sensor_title).Text = _model.Title;
                view.FindViewById<EditText>(Resource.Id.popup_sensor_input).Text = _model.Input;
                view.FindViewById<CheckBox>(Resource.Id.popup_sensor_id).Checked = _model.IsId;
                if (_model is Models.TextSensorModel)
                {
                    view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_text).Checked = true;
                    view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_value).Enabled = false;
                    view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_percent).Enabled = false;
                    view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_state).Enabled = false;
                }
                else if (_model is Models.ValueSensorModel v)
                {
                    switch (v.Display)
                    {
                        case ValueSensorModel.DisplayType.VALUE:
                            view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_value).Checked = true;
                            break;
                        case ValueSensorModel.DisplayType.PERCENT:
                            view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_percent).Checked = true;
                            break;
                        case ValueSensorModel.DisplayType.STATE:
                            view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_state).Checked = true;
                            break;
                        default:
                            break;
                    }
                    view.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_text).Enabled = false;
                }
                s.Text = _model.Refresh.ToString(@"hh\:mm\:ss\.fff", new CultureInfo("en-US"));
            }
            s.Click += RefreshSelect;
            view.FindViewById<Button>(Resource.Id.popup_sensor_save).Click += SaveClick;
            view.FindViewById<Button>(Resource.Id.popup_sensor_cancel).Click += CancelClick;
            return view;
        }

        private void CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked == false)
                return;
            SelectRadio(sender as RadioButton);
        }

        private void SelectRadio(RadioButton radio)
        {
            Activity.RunOnUiThread(() =>
            {
                foreach (var r in _radios)
                {
                    if (r != radio)
                        r.Checked = false;
                    r.Error = null;
                }
                radio.Checked = true;
            });
        }

        private void RefreshSelect(object sender, EventArgs e)
        {
            _popup = new FragmentPopupTimespan((sender as TextView).Text, @"hh\:mm\:ss\.fff", new CultureInfo("en-US"));
            _popup.PopupClose += RefreshPopupClose;
            _popup.Show(Activity.SupportFragmentManager, "RefreshPopup");
        }

        private void RefreshPopupClose(object sender, Events.PopupEventArgs e)
        {
            if(e.HasResult)
            {
                Activity.RunOnUiThread(() =>
                {
                    _model.Refresh = (TimeSpan)(e.Result as TimeSpan?);
                    _view.FindViewById<TextView>(Resource.Id.popup_sensor_refresh).Text = _model.Refresh.ToString(@"hh\:mm\:ss\.fff", new CultureInfo("en-US"));
                });
            }
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
                if(_model is null)
                {
                    if (val.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_text).Checked) _model = new Models.TextSensorModel();
                    else
                        _model = new Models.ValueSensorModel();
                }
                if(_model is Models.ValueSensorModel v)
                {
                    ValueSensorModel.DisplayType type = ValueSensorModel.DisplayType.VALUE;
                    if (val.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_value).Checked) type = ValueSensorModel.DisplayType.VALUE;
                    else if (val.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_percent).Checked) type = ValueSensorModel.DisplayType.PERCENT;
                    else if (val.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_state).Checked) type = ValueSensorModel.DisplayType.STATE;

                    v.Display = type;
                }
                _model.Title = val.FindViewById<EditText>(Resource.Id.popup_sensor_title).Text.Trim();
                _model.Input = val.FindViewById<EditText>(Resource.Id.popup_sensor_input).Text.Trim().ToUpper();
                _model.IsId = val.FindViewById<CheckBox>(Resource.Id.popup_sensor_id).Checked;
                if (TimeSpan.TryParseExact(val.FindViewById<TextView>(Resource.Id.popup_sensor_refresh).Text,
                                          @"hh\:mm\:ss\.fff", new CultureInfo("en-US") ,out TimeSpan t))
                    _model.Refresh = t;
                else
                    _model.Refresh = TimeSpan.FromSeconds(10);
                    
                _args = new Events.PopupEventArgs(true, _model);
                Dialog.Dismiss();
                Dialog.Hide();
            }
        }

        private bool ValidateView(View val)
        {
            var input = val.FindViewById<EditText>(Resource.Id.popup_sensor_input);
            var isId = val.FindViewById<CheckBox>(Resource.Id.popup_sensor_id);
            var isText = val.FindViewById<RadioButton>(Resource.Id.popup_sensor_radio_text);

            if (string.IsNullOrWhiteSpace(input.Text))
            {
                input.Error = Resources.GetString(Resource.String.input_empty);
                return false;
            }
            else if(isId.Checked)
            {
                if (Regex.IsMatch(input.Text, @"^(\d+)$") == false)
                {
                    input.Error = Resources.GetString(Resource.String.input_wrong);
                    return false;
                }
            }
            else
            {
                if(isText.Checked == true)
                {
                    isText.Error = Resources.GetString(Resource.String.input_pin_as_text);
                    return false;
                }
                else if (Regex.IsMatch(input.Text, @"^(([ADad]\d+)|(\d+))$") == false)
                {
                    input.Error = Resources.GetString(Resource.String.input_wrong);
                    return false;
                }
                else if (ViewModels.SettingsViewModel.Settings.Pins.Split(',').Contains(input.Text))
                {
                    input.Error = Resources.GetString(Resource.String.input_pin_restricted);
                    return false;
                }
            }
            return true;
        }
    }
}