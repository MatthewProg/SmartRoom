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
    public class FragmentValuePopup : DialogFragment
    {
        private Models.ListCellModel _model;
        private InputMethodManager _imm;

        public FragmentValuePopup(ListCellModel model)
        {
            _model = model;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            _imm.ToggleSoftInput(ShowFlags.Implicit, HideSoftInputFlags.None);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.popup_value, container, false);
            var editText = v.FindViewById<EditText>(Resource.Id.popup_value);
            if (_model != null)
            {
                v.FindViewById<TextView>(Resource.Id.popup_title).Text = _model.Title;
                v.FindViewById<TextView>(Resource.Id.popup_subtitle).Text = _model.Description;
                editText.Text = _model.Value;
                editText.InputType = _model.InputType;
                editText.ShowSoftInputOnFocus = true;
            }
            v.FindViewById<Button>(Resource.Id.popup_cancel).Click += ButtonCancelClick;
            v.FindViewById<Button>(Resource.Id.popup_ok).Click += ButtonOkClick;
            editText.EditorAction += EditTextConfirm;
            editText.RequestFocus();

            _imm = Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            _imm.ShowSoftInput(editText, ShowFlags.Forced);
            _imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            return v;
        }

        private void EditTextConfirm(object sender, TextView.EditorActionEventArgs e)
        {
            ((EditText)sender).RootView.FindViewById<Button>(Resource.Id.popup_ok).PerformClick();
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            var val = ((Button)sender).RootView.FindViewById<EditText>(Resource.Id.popup_value);
            if (ValidateEditText(val))
            {
                _model.Value = val.Text;
                Dialog.Dismiss();
                Dialog.Hide();
            }
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            Dialog.Dismiss();
            Dialog.Hide();
        }

        private bool ValidateEditText(EditText input)
        {
            if (input == null) return false;

            if (_model != null)
            {
                if (Regex.IsMatch(input.Text, _model.Regex)) return true;

                input.Error = Resources.GetString(_model.ErrorMessageId);
                return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(input.Text))
                {
                    input.Error = Resources.GetString(Resource.String.input_empty);
                    return false;
                }
            }

            return true;
        }
    }
}