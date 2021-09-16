using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using SmartRoom.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Fragments
{
    public class FragmentMacros : Fragment
    {
        private readonly MacrosManager _macrosManager;

        public FragmentMacros(Managers.MacrosManager macrosManager)
        {
            _macrosManager = macrosManager;
            _macrosManager.Macros.Add(new Models.MacroModel()
            { 
                Enabled = true,
                Name = "Test",
                Repeat = true,
                Running = false,
                Items = new System.Collections.ObjectModel.ObservableCollection<Interfaces.IMacroItemModel>()
                {
                    new Models.SliderSwitchModel() { Enabled=true, Fade=true, Pin="10", Value=0F },
                    new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 },
                    new Models.SliderSwitchModel() { Enabled=true, Fade=true, Pin="10", Value=0.5F },
                    new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 },
                    new Models.SliderSwitchModel() { Enabled = true,  Fade=true, Pin="10", Value=0F },
                    new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 },
                    new Models.SliderSwitchModel() { Enabled = true,  Fade=true, Pin="10", Value=1F },
                    new Models.DelayMacroItemModel() { Enabled=true, Delay=1000 }
                }
            });
            _macrosManager.StartMacro(_macrosManager.Macros[0]);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.content_macros, container, false);
            return v;
        }
    }
}