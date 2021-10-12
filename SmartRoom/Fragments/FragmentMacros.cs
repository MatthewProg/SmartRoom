using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using SmartRoom.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Fragments
{
    public class FragmentMacros : Fragment
    {
        private readonly MacrosManager _macrosManager;
        private readonly ObservableCollection<Models.SwitchModel> _switches;
        private Fragments.FragmentPopupValue _popup;

        public FragmentMacros(Managers.MacrosManager macrosManager, ObservableCollection<Models.SwitchModel> switches)
        {
            _switches = switches;
            _macrosManager = macrosManager;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.content_macros, container, false);

            var list = v.FindViewById<ListView>(Resource.Id.macros_list);
            var text = v.FindViewById<TextView>(Resource.Id.macros_title);
            var add = v.FindViewById<Button>(Resource.Id.macros_add);

            list.Adapter = new Adapters.MacrosAdapter(Activity, _macrosManager, _switches);
            text.Visibility = (_macrosManager.MacrosViewModel.Macros.Count == 0) ? ViewStates.Visible : ViewStates.Gone;
            add.Click += Add_Click;

            return v;
        }

        private void Add_Click(object sender, EventArgs e)
        {
            _popup = new FragmentPopupValue(new Models.ListCellModel(null, 
                                                Resource.String.popup_title_title, 
                                                Resource.String.popup_description_title,
                                                "", Android.Text.InputTypes.TextFlagAutoCorrect, 
                                                "", 0));
            _popup.PopupClose += PopupClose;
            _popup.Show(Activity.SupportFragmentManager, "PopupAddMacro");
        }

        private void PopupClose(object sender, Events.PopupEventArgs e)
        {
            if(e.HasResult == true)
            {
                var macro = new Models.MacroModel()
                {
                    Enabled = true,
                    Name = (e.Result as Models.ListCellModel).Value
                };
                _macrosManager.MacrosViewModel.Macros.Add(macro);
            }
        }
    }
}