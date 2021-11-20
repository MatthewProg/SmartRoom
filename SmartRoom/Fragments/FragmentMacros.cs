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
using System.Threading.Tasks;

namespace SmartRoom.Fragments
{
    public class FragmentMacros : Fragment
    {
        private readonly MacrosManager _macrosManager;
        private readonly ViewModels.SwitchesViewModel _switches;
        private Fragments.FragmentPopupValue _popup;
        private View _view;

        public FragmentMacros(Managers.MacrosManager macrosManager, ViewModels.SwitchesViewModel switches)
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
            _view = v;
            
            var add = v.FindViewById<Button>(Resource.Id.macros_add);

            if (_macrosManager.MacrosViewModel.TaskLoad.IsCompleted == true &&
                _switches.TaskLoad.IsCompleted == true)
            {
                _macrosManager.MacrosViewModel.Macros.CollectionChanged += Macros_CollectionChanged;
                ShowLoaded(v);
            }
            else
                Task.WhenAll(_macrosManager.MacrosViewModel.TaskLoad, _switches.TaskLoad).ContinueWith(delegate
                {
                    _macrosManager.MacrosViewModel.Macros.CollectionChanged += Macros_CollectionChanged;
                    Activity?.RunOnUiThread(() => ShowLoaded(v));
                });

            add.Click += Add_Click;

            return v;
        }

        private void Macros_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
               e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                if (_macrosManager.MacrosViewModel.Macros.Count < 2)
                    UpdateTextVisibility();
        }

        private void ShowLoaded(View v)
        {
            var list = v.FindViewById<ListView>(Resource.Id.macros_list);
            var text = v.FindViewById<TextView>(Resource.Id.macros_title);
            var loading = v.FindViewById<RelativeLayout>(Resource.Id.macros_loading);

            list.Adapter = new Adapters.MacrosAdapter(Activity, _macrosManager, _switches.SwitchesCollection);
            text.Visibility = (_macrosManager.MacrosViewModel.Macros.Count == 0) ? ViewStates.Visible : ViewStates.Gone;
            loading.Visibility = ViewStates.Gone;
        }

        private void UpdateTextVisibility()
        {
            Activity?.RunOnUiThread(() =>
            {
                var text = _view.FindViewById<TextView>(Resource.Id.macros_title);
                text.Visibility = (_macrosManager.MacrosViewModel.Macros.Count == 0) ? ViewStates.Visible : ViewStates.Gone;
            });
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