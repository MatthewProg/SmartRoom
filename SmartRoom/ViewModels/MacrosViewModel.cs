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
    public class MacrosViewModel : Interfaces.IViewModel
    {
        private bool _saveScheduled;
        private bool _loadScheduled;

        public ObservableCollection<Models.MacroModel> Macros { get; private set; }

        public Task TaskSave { get; private set; }
        public Task TaskLoad { get; private set; }

        public MacrosViewModel()
        {
            Macros = new ObservableCollection<Models.MacroModel>();
            Macros.CollectionChanged += MacrosCollectionChanged;
            _saveScheduled = false;
            _loadScheduled = false;
        }

        private void MacrosCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if(e.NewItems != null)
                {
                    foreach (Models.MacroModel item in e.NewItems)
                    {
                        item.PropertyChanged += MacroPropertyChanged;
                        item.Items.CollectionChanged += MacroItemsCollectionChanged;
                        foreach (var i in item.Items)
                            i.PropertyChanged += MacroItemPropertyChanged;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Models.MacroModel item in e.OldItems)
                    {
                        item.PropertyChanged -= MacroPropertyChanged;
                        item.Items.CollectionChanged -= MacroItemsCollectionChanged;
                        foreach (var i in item.Items)
                            i.PropertyChanged -= MacroItemPropertyChanged;
                    }
                }
            }
            SaveModelAsync();
        }

        private void MacroItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (Interfaces.IMacroItemModel item in e.NewItems)
                        item.PropertyChanged += MacroItemPropertyChanged;
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Interfaces.IMacroItemModel item in e.OldItems)
                        item.PropertyChanged -= MacroItemPropertyChanged;
                }
            }
            SaveModelAsync();
        }

        private void MacroItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SaveModelAsync();
        }

        private void MacroPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Running")
                SaveModelAsync();
        }

        private async Task SaveMacrosAsync(string filename)
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            using (var writer = File.CreateText(path))
                await writer.WriteLineAsync(JsonConvert.SerializeObject(Macros, new Converters.MacroJsonConverter()));

        }

        private async Task LoadMacrosAsync(string filename)
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    Macros = JsonConvert.DeserializeObject<ObservableCollection<Models.MacroModel>>(await reader.ReadToEndAsync(), new Converters.MacroJsonConverter());
                    Macros.CollectionChanged += MacrosCollectionChanged;
                    foreach (Models.MacroModel item in Macros)
                    {
                        item.PropertyChanged += MacroPropertyChanged;
                        item.Items.CollectionChanged += MacroItemsCollectionChanged;
                        foreach (var i in item.Items)
                            i.PropertyChanged += MacroItemPropertyChanged;
                    }
                }
            }
            else
                await SaveModelAsync(filename);
        }

        public Task SaveModelAsync(string filename = "macros.json")
        {
            _saveScheduled = true;
            
            async Task Save(string filename) //Save until no updates
            {
                do
                {
                    _saveScheduled = false;
                    await SaveMacrosAsync(filename);
                } while (_saveScheduled);
            };

            if (TaskSave == null || TaskSave.IsCompleted == true) //Start saving only if prev was saved
            {
                if (TaskLoad != null && TaskLoad.IsCompleted == false) //If VM is loading file, wait with saving
                    TaskSave = TaskLoad.ContinueWith(async delegate { await Save(filename); });
                else
                    TaskSave = Task.Run(async() => await Save(filename));
            }
            return TaskSave;
        }

        public Task LoadModelAsync(string filename = "macros.json")
        {
            _loadScheduled = true;

            async Task Load(string filename)
            {
                do
                {
                    _loadScheduled = false;
                    await LoadMacrosAsync(filename);
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