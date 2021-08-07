﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class ColorSwitchModel : SwitchModel
    { 
        private ColorModel _color;
        private string _redPin;
        private string _greenPin;
        private string _bluePin;

        public ColorSwitchModel()
        {
            _color = new ColorModel();
        }

        public ColorModel Color
        {
            get => _color;
            set
            {
                _color = value;
                _color.PropertyChanged += _color_PropertyChanged;
                OnPropertyChanged("Color");
            }
        }

        private void _color_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Color");
        }

        public string RedPin
        {
            get => _redPin;
            set
            {
                if (_redPin == value) return;
                _redPin = value;
                OnPropertyChanged("RedPin");
            }
        }

        public string GreenPin
        {
            get => _greenPin;
            set
            {
                if (_greenPin == value) return;
                _greenPin = value;
                OnPropertyChanged("GreenPin");
            }
        }

        public string BluePin
        {
            get => _bluePin;
            set
            {
                if (_bluePin == value) return;
                _bluePin = value;
                OnPropertyChanged("BluePin");
            }
        }

        protected override void OnPropertyChanged(String info)
        {
            base.OnPropertyChanged(info);
        }
    }
}