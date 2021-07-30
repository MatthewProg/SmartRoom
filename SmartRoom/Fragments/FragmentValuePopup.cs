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

namespace SmartRoom.Fragments
{
    public class FragmentValuePopup : DialogFragment
    {
        private Models.ListCellModel _model;

        public FragmentValuePopup(ListCellModel model)
        {
            _model = model;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.popup_value, container, false);
            if (_model != null)
            {
                v.FindViewById<TextView>(Resource.Id.popup_title).Text = _model.Title;
                v.FindViewById<TextView>(Resource.Id.popup_subtitle).Text = _model.Description;
                v.FindViewById<EditText>(Resource.Id.popup_value).Text = _model.Value;
            }
            v.FindViewById<Button>(Resource.Id.popup_cancel).Click += ButtonCancelClick;
            v.FindViewById<Button>(Resource.Id.popup_ok).Click += ButtonOkClick;
            v.FindViewById<EditText>(Resource.Id.popup_value).EditorAction += EditTextConfirm;
            return v;
        }

        private void EditTextConfirm(object sender, TextView.EditorActionEventArgs e)
        {
            ((EditText)sender).RootView.FindViewById<Button>(Resource.Id.popup_ok).PerformClick();
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            var val = ((Button)sender).RootView.FindViewById<EditText>(Resource.Id.popup_value)?.Text;
            if (string.IsNullOrWhiteSpace(val) == false)
                _model.Value = val;
            Dialog.Dismiss();
            Dialog.Hide();
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            Dialog.Dismiss();
            Dialog.Hide();
        }
    }
}