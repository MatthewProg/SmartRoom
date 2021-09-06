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
        private class AllSwitches
        {
            public AllSwitches(IEnumerable<Models.SwitchModel> switches) : this()
            {
                foreach (var s in switches)
                    if      (s is Models.ToggleSwitchModel) Toggles.Add(s as Models.ToggleSwitchModel);
                    else if (s is Models.SliderSwitchModel) Sliders.Add(s as Models.SliderSwitchModel);
                    else if (s is Models.ColorSwitchModel)  Colors.Add(s as Models.ColorSwitchModel);
            }
            public AllSwitches()
            {
                Toggles = new List<Models.ToggleSwitchModel>();
                Sliders = new List<Models.SliderSwitchModel>();
                Colors = new List<Models.ColorSwitchModel>();
            }
            public List<Models.ToggleSwitchModel> Toggles { get; set; }
            public List<Models.SliderSwitchModel> Sliders { get; set; }
            public List<Models.ColorSwitchModel> Colors { get; set; }
        }

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
                await writer.WriteLineAsync(JsonConvert.SerializeObject(new AllSwitches(SwitchesCollection)));
        }

        public async Task LoadSwitchesAsync(string filename = "switches.json")
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var ds = JsonConvert.DeserializeObject<AllSwitches>(await reader.ReadToEndAsync());
                    SwitchesCollection.CollectionChanged -= SwitchesChanged;
                    foreach (var e in SwitchesCollection)
                        e.PropertyChanged -= SwitchesPropertyChanged;
                    SwitchesCollection.Clear();
                    foreach (var e in ds.Toggles) SwitchesCollection.Add(e as Models.SwitchModel);
                    foreach (var e in ds.Sliders) SwitchesCollection.Add(e as Models.SwitchModel);
                    foreach (var e in ds.Colors) SwitchesCollection.Add(e as Models.SwitchModel);
                }
                foreach (var e in SwitchesCollection)
                    e.PropertyChanged += SwitchesPropertyChanged;
                SwitchesCollection.CollectionChanged += SwitchesChanged;
            }
        }
    }
}