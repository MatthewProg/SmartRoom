using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using SmartRoom.Events;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Fragments
{
    public class FragmentSelectOld : Fragment, Interfaces.ISelectFragment
    {
        private IEnumerable<Interfaces.IMacroItemModel> _sourceItems;
        private IEnumerable<Models.ListItemIconModel> _convertedItems;

        public event EventHandler<PopupEventArgs> Selected;

        public FragmentSelectOld(IEnumerable<Interfaces.IMacroItemModel> items)
        {
            _sourceItems = items;
            _convertedItems = new List<ListItemIconModel>();
        }

        private List<Models.ListItemIconModel> ConvertItems(IEnumerable<Interfaces.IMacroItemModel> items)
        {
            var output = new List<Models.ListItemIconModel>();
            if (items.Count() == 0) 
                return output;
        
            var sPin = Activity.GetString(Resource.String.text_alias_pin) + ":";
            var sRPin = Activity.GetString(Resource.String.text_alias_red_pin) + ":";
            var sGPin = Activity.GetString(Resource.String.text_alias_green_pin) + ":";
            var sBPin = Activity.GetString(Resource.String.text_alias_blue_pin) + ":";
            
            foreach (var item in items)
            {
                var e = new Models.ListItemIconModel();
                if(item is Models.SwitchModel m)
                {
                    e.Title = m.Title;
                    e.Model = m.Clone();

                    if (item is Models.ToggleSwitchModel t)
                    {
                        e.Subtitle = sPin + t.Pin;
                        e.Icon = Resource.Drawable.ic_electric_switch;
                    }
                    else if (item is Models.SliderSwitchModel s)
                    {
                        e.Subtitle = sPin + s.Pin;
                        e.Icon = Resource.Drawable.ic_slider;

                    }
                    else if (item is Models.ColorSwitchModel c)
                    {
                        e.Subtitle = $"{sRPin}{c.RedPin}  {sGPin}{c.GreenPin}  {sBPin}{c.BluePin}";
                        e.Icon = Resource.Drawable.ic_led;
                    }
                    else 
                        continue;
                }
                else 
                    continue;

                output.Add(e);
            }

            return output;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            _convertedItems = _convertedItems.Union(ConvertItems(_sourceItems))
                                             .GroupBy(m => new { m.Title, m.Subtitle, m.Icon })
                                             .Select(g => g.First()); //Distinct ignoring Model
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.macro_add_selector_tab, container, false);

            var list = v.FindViewById<ListView>(Resource.Id.macro_add_selector_list);

            list.Adapter = new Adapters.ListItemIconAdapter(Activity, _convertedItems);
            list.ItemClick += List_ItemClick;

            return v;
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Selected?.Invoke(this, new PopupEventArgs(true, _convertedItems.ElementAt(e.Position).Model));
        }
    }
}