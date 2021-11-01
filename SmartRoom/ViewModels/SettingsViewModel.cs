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
    public class SettingsViewModel : Interfaces.IViewModel
    {
        private bool _saveScheduled;
        private bool _loadScheduled;

        public ObservableCollection<Models.ListCellModel> SettingsCollection { get; private set; }
        public static Models.SettingsModel Settings { get; private set; }

        public Task TaskSave { get; private set; }
        public Task TaskLoad { get; private set; }

        public SettingsViewModel()
        {
            Settings = new Models.SettingsModel();
            SettingsCollection = new ObservableCollection<Models.ListCellModel>()
            {
                new Models.ListCellModel("Address", Resource.String.settings_ip, Resource.String.settings_ip_desc, "192.168.4.1", Android.Text.InputTypes.TextVariationUri,
                                        @"\b(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b|[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]*)?",
                                        Resource.String.input_address),
                new Models.ListCellModel("Port", Resource.String.settings_port, Resource.String.settings_port_desc, "23", Android.Text.InputTypes.ClassNumber,
                                        @"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$",
                                        Resource.String.input_port),
                new Models.ListCellModel("Pins", Resource.String.settings_pins, Resource.String.settings_pins_desc, "D13,D11,D10,D9,D3,A4,A5", Android.Text.InputTypes.TextFlagCapCharacters,
                                        @"(^$)|(^(([ADad]\d+)|(\d+))((\s*,\s*[ADad]\d+)|(\s*,\s*\d+))*$)",
                                        Resource.String.input_pins)
            };
            foreach (var item in SettingsCollection)
                item.PropertyChanged += Item_PropertyChanged;
            Settings.PropertyChanged += Settings_PropertyChanged;
            _saveScheduled = false;
            _loadScheduled = false;
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
            SaveModelAsync();
        }

        private async Task SaveSettingsAsync(string filename)
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            using (var writer = File.CreateText(path))
                await writer.WriteLineAsync(JsonConvert.SerializeObject(Settings));
        }

        private async Task LoadSettingsAsync(string filename)
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
                await SaveModelAsync();
        }

        public Task SaveModelAsync(string filename = "settings.json")
        {
            _saveScheduled = true;

            async Task Save(string filename)
            {
                do
                {
                    _saveScheduled = false;
                    await SaveSettingsAsync(filename);
                } while (_saveScheduled);
            };

            if (TaskSave == null || TaskSave.IsCompleted == true)
            {
                if (TaskLoad != null && TaskLoad.IsCompleted == false)
                    TaskSave = TaskLoad.ContinueWith(async delegate { await Save(filename); });
                else
                    TaskSave = Task.Run(async () => await Save(filename));
            }
            return TaskSave;
        }

        public Task LoadModelAsync(string filename = "settings.json")
        {
            _loadScheduled = true;

            async Task Load(string filename)
            {
                do
                {
                    _loadScheduled = false;
                    await LoadSettingsAsync(filename);
                } while (_loadScheduled);
            };

            if (TaskLoad == null || TaskLoad.IsCompleted == true)
            {
                if (TaskSave != null && TaskSave.IsCompleted == false)
                    TaskLoad = TaskSave.ContinueWith(async delegate { await Load(filename); });
                else
                    TaskLoad = Task.Run(async () => await Load(filename));
            }
            return TaskLoad;
        }
    }
}