using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class ListItemIconModel : INotifyPropertyChanged
    {
        private string _title;
        private string _subtitle;
        private int _icon;
        private object _model;

        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;

                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Subtitle
        {
            get => _subtitle;
            set
            {
                if (_subtitle == value) return;

                _subtitle = value;
                OnPropertyChanged("Subtitle");
            }
        }

        public int Icon
        {
            get => _icon;
            set
            {
                if (_icon == value) return;

                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        public object Model
        {
            get => _model;
            set
            {
                if (_model == value) return;

                _model = value;
                OnPropertyChanged("Model");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}