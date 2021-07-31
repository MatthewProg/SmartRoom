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
    public class ListCellModel : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private string _description;
        private string _value;
        private Android.Text.InputTypes _inputType;
        private string _regex;
        private int _errorMessageId;

        public ListCellModel(string id, string title, string description, string value, Android.Text.InputTypes inputType, string regex, int errorMessageId)
        {
            ID = id;
            Title = title;
            Description = description;
            Value = value;
            InputType = inputType;
            Regex = regex;
            ErrorMessageId = errorMessageId;
        }

        public string ID 
        { 
            get => _id; 
            private set => _id = value; 
        }
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
        public string Description 
        { 
            get => _description;
            set
            {
                if (_description == value) return;

                _description = value;
                OnPropertyChanged("Description");
            }
        }
        public string Value 
        { 
            get => _value;
            set
            {
                if (_value == value)
                    return;

                _value = value;
                OnPropertyChanged("Value");
            }
        }
        public Android.Text.InputTypes InputType
        {
            get => _inputType;
            set 
            {
                if (_inputType == value) return;

                _inputType = value;
                OnPropertyChanged("InputType");
            }
        }
        public string Regex 
        {
            get => _regex;
            set
            {
                if (_regex == value)
                    return;

                _regex = value;
                OnPropertyChanged("Regex");
            }
        }
        public int ErrorMessageId
        {
            get => _errorMessageId;
            set
            {
                if (_errorMessageId == value)
                    return;

                _errorMessageId = value;
                OnPropertyChanged("ErrorMessageId");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}