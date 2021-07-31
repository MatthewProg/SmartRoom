using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SmartRoom.ViewModels
{
    public class SettingsViewModel
    {
        public ObservableCollection<Models.ListCellModel> SettingsCollection { get; private set; }
        public Models.SettingsModel Settings { get; set; }

        public SettingsViewModel()
        {
            Settings = new Models.SettingsModel();
            SettingsCollection = new ObservableCollection<Models.ListCellModel>()
            {
                new Models.ListCellModel("Address", "IP Address", "Server address", "192.168.4.1", Android.Text.InputTypes.TextVariationUri,
                                        @"\b(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b|[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]*)?",
                                        Resource.String.input_address),
                new Models.ListCellModel("Port", "Port", "Listener port", "23", Android.Text.InputTypes.ClassNumber,
                                        @"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$",
                                        Resource.String.input_port),
                new Models.ListCellModel("Pins", "Restricted pins", "Pins used by hardware", "A0,A1,D13", Android.Text.InputTypes.TextFlagCapCharacters,
                                        @"(^$)|(^(([ADad]\d+)|(\d+))((\s*,\s*[ADad]\d+)|(\s*,\s*\d+))*$)",
                                        Resource.String.input_pins)
            };
            foreach (var item in SettingsCollection)
                item.PropertyChanged += Item_PropertyChanged; ;
            Settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Value")
            {
                var prop = Settings.GetType().GetProperty((sender as Models.ListCellModel).ID);
                if (prop != null)
                {
                    try
                    {
                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromString((sender as Models.ListCellModel).Value);
                        prop.SetValue(Settings, value);

                    }
                    catch (NotSupportedException)
                    {
                        Android.Util.Log.Error("Settings", "Error while parsing");
                    }
                }
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = Settings.GetType().GetProperty(e.PropertyName);
            var index = SettingsCollection.IndexOf(SettingsCollection.Where(x => x.ID == e.PropertyName).SingleOrDefault());
            if(index >= 0)
                SettingsCollection[index].Value = prop.GetValue(Settings).ToString();
            Task.Run(async () => await SaveSettingsAsync());
        }

        public async Task SaveSettingsAsync(string filename = "settings.json")
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            using (var writer = File.CreateText(path))
                await writer.WriteLineAsync(JsonConvert.SerializeObject(Settings));
        }

        public async Task LoadSettingsAsync(string filename = "settings.json")
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var ds = JsonConvert.DeserializeObject<Models.SettingsModel>(await reader.ReadToEndAsync());
                    Settings.Address = ds.Address;
                    Settings.Port = ds.Port;
                    Settings.Pins = ds.Pins;
                }
            }
            else
                await SaveSettingsAsync(filename);
        }
    }
}