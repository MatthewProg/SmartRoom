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
    public class SwitchesViewModel
    {
        public ObservableCollection<Models.SwitchModel> SwitchesCollection { get; private set; }
        public SwitchesViewModel()
        {
            SwitchesCollection = new ObservableCollection<Models.SwitchModel>();
            SwitchesCollection.CollectionChanged += SwitchesChanged;
        }

        private void SwitchesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.OldItems != null)
            {
                foreach (Models.SwitchModel m in e.OldItems)
                    m.PropertyChanged -= SwitchesPropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (Models.SwitchModel m in e.NewItems)
                    m.PropertyChanged += SwitchesPropertyChanged;
            }
            Task.Run(async () => await SaveSwitchesAsync());
        }

        private void SwitchesPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Toggle" && e.PropertyName != "Value" && e.PropertyName != "Color" && 
                e.PropertyName != "Fade" && e.PropertyName != "Enabled")
                Task.Run(async () => await SaveSwitchesAsync());
        }

        public async Task SaveSwitchesAsync(string filename = "switches.json")
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            using (var writer = File.CreateText(path))
                await writer.WriteLineAsync(JsonConvert.SerializeObject(SwitchesCollection, new Converters.SwitchesJsonConverter()));
        }

        public async Task LoadSwitchesAsync(string filename = "switches.json")
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var ds = JsonConvert.DeserializeObject<List<Models.SwitchModel>>(await reader.ReadToEndAsync(), new Converters.SwitchesJsonConverter());
                    SwitchesCollection.CollectionChanged -= SwitchesChanged;
                    foreach (var e in SwitchesCollection)
                        e.PropertyChanged -= SwitchesPropertyChanged;
                    SwitchesCollection.Clear();
                    foreach (var e in ds)
                        SwitchesCollection.Add(e);
                }
                foreach (var e in SwitchesCollection)
                    e.PropertyChanged += SwitchesPropertyChanged;
                SwitchesCollection.CollectionChanged += SwitchesChanged;
            }
        }
    }
}