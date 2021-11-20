using AndroidX.Fragment.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Android.Material.Tabs;
using AndroidX.ViewPager.Widget;
using System.Collections.ObjectModel;
using SmartRoom.Models;
using AndroidX.ViewPager2.Widget;
using static Google.Android.Material.Tabs.TabLayoutMediator;

namespace SmartRoom.Fragments
{
    public class FragmentPopupMacroAddSelector : Extensions.Popup
    {
        private class TabsStrategy : Java.Lang.Object, ITabConfigurationStrategy
        {
            public void OnConfigureTab(TabLayout.Tab p0, int p1)
            {
                p0.SetText(p1 switch
                {
                    0 => Resource.String.tab_new,
                    1 => Resource.String.tab_macro,
                    2 => Resource.String.tab_switches,
                    _ => throw new System.Exception("Wrong position in TabsStrategy.OnConfigureTab()")
                });
            }
        }

        private Events.PopupEventArgs _args;
        private readonly ObservableCollection<Models.MacroModel> _macros;
        private readonly ObservableCollection<Models.SwitchModel> _switches;

        public FragmentPopupMacroAddSelector(ObservableCollection<MacroModel> macros, ObservableCollection<SwitchModel> switches)
        {
            _macros = macros;
            _switches = switches;
            _args = new Events.PopupEventArgs();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.popup_macro_add_selector, container, false);

            var tabs = view.FindViewById<TabLayout>(Resource.Id.popup_macro_add_selector_tabs);
            var pages = view.FindViewById<ViewPager2>(Resource.Id.popup_macro_add_selector_pager);
            var adapter = new Adapters.MacroAddPagerAdapter(Activity, _macros, _switches);

            adapter.Selected += Adapter_Selected;
            pages.OffscreenPageLimit = 2;
            pages.Adapter = adapter;
            new TabLayoutMediator(tabs, pages, new TabsStrategy()).Attach();

            var res = new Android.Graphics.Drawables.InsetDrawable(
                new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.White), 
                2, 55, 2, 55);
            Dialog.Window.SetBackgroundDrawable(res);

            return view;
        }

        private void Adapter_Selected(object sender, Events.PopupEventArgs e)
        {
            _args = e;
            Dialog.Dismiss();
            Dialog.Hide();
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnPopupClose(this, _args);
        }
    }
}