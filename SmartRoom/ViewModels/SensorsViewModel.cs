using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoom.ViewModels
{
    public class SensorsViewModel : Interfaces.IViewModel
    {
        private readonly Interfaces.IPackagesManager _packagesManager;
        private bool _saveScheduled;
        private bool _loadScheduled;

        public Task TaskSave { get; private set; }
        public Task TaskLoad { get; private set; }
        public ObservableCollection<Models.SensorModel> Sensors { get; private set; }

        public SensorsViewModel(Interfaces.IPackagesManager packagesManager)
        {
            Sensors = new ObservableCollection<Models.SensorModel>();
            Sensors.CollectionChanged += SensorsCollectionChanged;
            _saveScheduled = false;
            _loadScheduled = false;
            _packagesManager = packagesManager;
        }

        private void SensorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Models.SensorModel m in e.OldItems)
                {
                    m.PropertyChanged -= SensorPropertyChanged;
                    if (m.IsId) _packagesManager.IdValuesReceived -= m.UpdateListener;
                    else _packagesManager.PinValuesUpdated -= m.UpdateListener;
                }
            }
            if (e.NewItems != null)
            {
                foreach (Models.SensorModel m in e.NewItems)
                {
                    m.PropertyChanged += SensorPropertyChanged;
                    if (m.IsId) _packagesManager.IdValuesReceived += m.UpdateListener;
                    else _packagesManager.PinValuesUpdated += m.UpdateListener;
                }
            }
            SaveModelAsync();
        }

        private void SensorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsId")
            {
                var s = sender as Models.SensorModel;
                if (s.IsId == true)
                {
                    _packagesManager.PinValuesUpdated -= s.UpdateListener;
                    _packagesManager.IdValuesReceived += s.UpdateListener;
                }
                else
                {
                    _packagesManager.PinValuesUpdated += s.UpdateListener;
                    _packagesManager.IdValuesReceived -= s.UpdateListener;
                }
            }

            if (e.PropertyName != "Text" && e.PropertyName != "Value")
                SaveModelAsync();
        }

        private async Task SaveSensorsAsync(string filename)
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            using (var writer = File.CreateText(path))
                await writer.WriteLineAsync(JsonConvert.SerializeObject(Sensors, new Converters.SensorsJsonConverter()));
        }

        private async Task LoadSensorsAsync(string filename)
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var ds = JsonConvert.DeserializeObject<List<Models.SensorModel>>(await reader.ReadToEndAsync(), new Converters.SensorsJsonConverter());
                    Sensors.Clear();
                    foreach (var e in ds)
                        Sensors.Add(e);
                }
            }
        }

        public Task SaveModelAsync(string filename = "sensors.json")
        {
            _saveScheduled = true;

            async Task Save(string filename)
            {
                do
                {
                    _saveScheduled = false;
                    await SaveSensorsAsync(filename);
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

        public Task LoadModelAsync(string filename = "sensors.json")
        {
            _loadScheduled = true;

            async Task Load(string filename)
            {
                do
                {
                    _loadScheduled = false;
                    await LoadSensorsAsync(filename);
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