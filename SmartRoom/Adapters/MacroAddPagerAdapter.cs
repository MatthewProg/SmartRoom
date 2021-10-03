using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager.Widget;
using AndroidX.ViewPager2.Adapter;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Adapters
{
    public class MacroAddPagerAdapter : FragmentStateAdapter
    {
        private const int PAGES = 3;
        private readonly FragmentActivity _activity;
        private readonly ObservableCollection<Models.MacroModel> _macros;
        private readonly ObservableCollection<Models.SwitchModel> _switches;

        public event EventHandler<Events.PopupEventArgs> Selected;

        public MacroAddPagerAdapter(FragmentActivity activity, ObservableCollection<Models.MacroModel> macros, ObservableCollection<Models.SwitchModel> switches) : base(activity)
        {
            _activity = activity;
            _macros = macros;
            _switches = switches;
        }
        public override int ItemCount => PAGES;

        public override AndroidX.Fragment.App.Fragment CreateFragment(int p0)
        {
            Interfaces.ISelectFragment frag = p0 switch
            {
                0 => new Fragments.FragmentSelectNew(),
                1 => new Fragments.FragmentSelectOld(_macros.SelectMany(x=>x.Items)),
                2 => new Fragments.FragmentSelectOld(_switches.Cast<Interfaces.IMacroItemModel>()),
                _ => throw new System.Exception("Wrong position in MacroAddPagerAdapter.GetItem()")
            };
            frag.Selected += Frag_Selected;
            return (AndroidX.Fragment.App.Fragment)frag;
        }

        private void Frag_Selected(object sender, Events.PopupEventArgs e)
        {
            if (e.HasResult)
                Selected?.Invoke(this, e);
        }
    }
}