using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JaredRummler.Android.ColorPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Fragments
{
    class FragmentPopupColorPicker : Extensions.Popup
    {
        private Events.PopupEventArgs _args;
        private int _color;
        private Models.ColorModel _model;
        public FragmentPopupColorPicker(Models.ColorModel model, int color)
        {
            _color = color;
            _model = model;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.popup_color_picker, container, false);
            view.FindViewById<Button>(Resource.Id.popup_color_picker_cancel).Click += CancelClick;
            view.FindViewById<Button>(Resource.Id.popup_color_picker_select).Click += SelectClick;
            view.FindViewById<ColorPickerView>(Resource.Id.popup_color_picker_picker).Color = _color;
            return view;
        }

        private void SelectClick(object sender, EventArgs e)
        {
            var picker = (sender as Button).RootView.FindViewById<ColorPickerView>(Resource.Id.popup_color_picker_picker);
            _args = new Events.PopupEventArgs(true, picker.Color);
            _model?.FromAndroid(picker.Color);
            Dialog.Dismiss();
            Dialog.Hide();
        }

        private void CancelClick(object sender, EventArgs e)
        {
            Dialog.Dismiss();
            Dialog.Hide();
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnPopupClose(this, _args);
        }
    }
}