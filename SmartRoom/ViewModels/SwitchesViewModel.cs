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
    public class SwitchesViewModel : Interfaces.IViewModel
    {
        private readonly Interfaces.IPackagesManager _packagesManager;
        private bool _saveScheduled;
        private bool _loadScheduled;

        public Task TaskSave { get; private set; }
        public Task TaskLoad { get; private set; }
        public ObservableCollection<Models.SwitchModel> SwitchesCollection { get; private set; }

        public SwitchesViewModel(Interfaces.IPackagesManager packagesManager)
        {
            SwitchesCollection = new ObservableCollection<Models.SwitchModel>();
            SwitchesCollection.CollectionChanged += SwitchesChanged;
            _saveScheduled = false;
            _loadScheduled = false;
            _packagesManager = packagesManager;
        }

        private void SwitchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.OldItems != null)
            {
                foreach (Models.SwitchModel m in e.OldItems)
                {
                    m.PropertyChanged -= SwitchesPropertyChanged;
                    _packagesManager.PinValuesUpdated -= m.PinUpdateListener;
                }
            }
            if (e.NewItems != null)
            {
                foreach (Models.SwitchModel m in e.NewItems)
                {
                    m.PropertyChanged += SwitchesPropertyChanged;
                    _packagesManager.PinValuesUpdated += m.PinUpdateListener;
                }
            }
            SaveModelAsync();
        }

        private void SwitchesPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Toggle" || e.PropertyName == "Value" || e.PropertyName == "Color")
                _packagesManager.SetValue(sender as Models.SwitchModel);

            if (e.PropertyName != "Toggle" && e.PropertyName != "Value" && e.PropertyName != "Color" &&
                e.PropertyName != "Fade" && e.PropertyName != "Enabled")
                SaveModelAsync();
        }

        private async Task SaveSwitchesAsync(string filename)
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            using (var writer = File.CreateText(path))
                await writer.WriteLineAsync(JsonConvert.SerializeObject(SwitchesCollection, new Converters.SwitchesJsonConverter()));
        }

        private async Task LoadSwitchesAsync(string filename)
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var ds = JsonConvert.DeserializeObject<List<Models.SwitchModel>>(await reader.ReadToEndAsync(), new Converters.SwitchesJsonConverter());
                    SwitchesCollection.CollectionChanged -= SwitchesChanged;
                    foreach (var e in SwitchesCollection)
                    {
                        e.PropertyChanged -= SwitchesPropertyChanged;
                        _packagesManager.PinValuesUpdated -= e.PinUpdateListener;
                    }
                    SwitchesCollection.Clear();
                    foreach (var e in ds)
                        SwitchesCollection.Add(e);
                }
                foreach (var e in SwitchesCollection)
                {
                    e.PropertyChanged += SwitchesPropertyChanged;
                    _packagesManager.PinValuesUpdated += e.PinUpdateListener;
                }
                SwitchesCollection.CollectionChanged += SwitchesChanged;
            }
        }

        public Task SaveModelAsync(string filename = "switches.json")
        {
            _saveScheduled = true;

            async Task Save(string filename)
            {
                do
                {
                    _saveScheduled = false;
                    await SaveSwitchesAsync(filename);
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

        public Task LoadModelAsync(string filename = "switches.json")
        {
            _loadScheduled = true;

            async Task Load(string filename)
            {
                do
                {
                    _loadScheduled = false;
                    await LoadSwitchesAsync(filename);
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