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
    public class MacrosViewModel
    {
        public ObservableCollection<Models.MacroModel> Macros { get; private set; }

        public MacrosViewModel()
        {
            Macros = new ObservableCollection<Models.MacroModel>();
            Macros.CollectionChanged += MacrosCollectionChanged;
            Task.Run(async () => await LoadMacro());
            //Macros.Add(new Models.MacroModel() //Just tmp for dev
            //{
            //    Enabled = true,
            //    Name = "Test",
            //    Repeat = true,
            //    Running = false,
            //    Items = new System.Collections.ObjectModel.ObservableCollection<Interfaces.IMacroItemModel>()
            //    {
            //        new Models.SliderSwitchModel() { Title="First", Enabled=true, Fade=true, Pin="10", Value=1F },
            //        new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 },
            //        new Models.ToggleSwitchModel() { Title="Arduino", Enabled=true, Fade=true, Pin="D2", Toggle=true },
            //        new Models.SliderSwitchModel() { Title="First", Enabled=true, Fade=true, Pin="10", Value=0F },
            //        new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 },
            //        new Models.ColorSwitchModel() { Title="Color test nice", Enabled = true,  Fade=true, RedPin="1", GreenPin="2", BluePin="3", Color=new Models.ColorModel(255,128,192) },
            //        new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 },
            //        new Models.ColorSwitchModel() { Title="Color test nice", Enabled = true,  Fade=true, RedPin="1", GreenPin="2", BluePin="3", Color=new Models.ColorModel(0,0,0) },
            //        new Models.ToggleSwitchModel() { Title="Arduino", Enabled=true, Fade=false, Pin="D2", Toggle=false },
            //        new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 }
            //    }
            //});
            //MacroItemsCollectionChanged(null, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, Macros[0].Items));
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
                    }
                }
            }
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
        }

        private void MacroItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "Name") //Change to saving by button click
                Task.Run(async () => await SaveMacros());
        }

        private void MacroPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async Task SaveMacros(string filename = "macros.json")
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            using (var writer = File.CreateText(path))
                await writer.WriteLineAsync(JsonConvert.SerializeObject(Macros, new Converters.MacroJsonConverter()));

        }

        public async Task LoadMacro(string filename = "macros.json")
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), filename);
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    Macros = JsonConvert.DeserializeObject<ObservableCollection<Models.MacroModel>>(await reader.ReadToEndAsync(), new Converters.MacroJsonConverter());
                    Macros.CollectionChanged += MacrosCollectionChanged;
                    MacrosCollectionChanged(null, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, Macros));
                }
            }
            else
                await SaveMacros(filename);
        }
    }
}