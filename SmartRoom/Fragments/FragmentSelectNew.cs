using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using SmartRoom.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Fragments
{
    public class FragmentSelectNew : Fragment, Interfaces.ISelectFragment
    {
        private IEnumerable<Models.ListItemIconModel> _items;
        private Extensions.Popup _popup;

        public event EventHandler<PopupEventArgs> Selected;

        public override void OnCreate(Bundle savedInstanceState)
        {
            _items = new List<Models.ListItemIconModel>()
            {
                new Models.ListItemIconModel()
                {
                    Title = Activity.GetString(Resource.String.fab_switch),
                    Subtitle = null,
                    Icon = Resource.Drawable.ic_electric_switch,
                    Model = new Models.ToggleSwitchModel()
                },
                new Models.ListItemIconModel()
                {
                    Title = Activity.GetString(Resource.String.fab_slider),
                    Subtitle = null,
                    Icon = Resource.Drawable.ic_slider,
                    Model = new Models.SliderSwitchModel()
                },
                new Models.ListItemIconModel()
                {
                    Title = Activity.GetString(Resource.String.fab_rgb),
                    Subtitle = null,
                    Icon = Resource.Drawable.ic_led,
                    Model = new Models.ColorSwitchModel()
                },
                new Models.ListItemIconModel()
                {
                    Title = Activity.GetString(Resource.String.text_delay),
                    Subtitle = null,
                    Icon = Resource.Drawable.ic_hourglass,
                    Model = new Models.DelayMacroItemModel()
                }
            };
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.macro_add_selector_tab, container, false);

            var list = v.FindViewById<ListView>(Resource.Id.macro_add_selector_list);

            list.Adapter = new Adapters.ListItemIconAdapter(Activity, _items);
            list.ItemClick += List_ItemClick;

            return v;
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = _items.ElementAt(e.Position);
            if (item.Model is Models.ToggleSwitchModel) _popup = new FragmentPopupSwitch();
            else if (item.Model is Models.SliderSwitchModel) _popup = new FragmentPopupSlider();
            else if (item.Model is Models.ColorSwitchModel) _popup = new FragmentPopupRgb();
            else if (item.Model is Models.DelayMacroItemModel)
            {
                var m = new Models.DelayMacroItemModel();
                m.Enabled = true;
                Selected?.Invoke(this, new PopupEventArgs(true, m));
                return;
            }

            _popup.PopupClose += (o, e) =>
            {
                if(e.HasResult)
                {
                    (e.Result as Models.SwitchModel).Enabled = true;
                    Selected?.Invoke(this, e);
                }
            };
            _popup?.Show(Activity.SupportFragmentManager, "PopupAddSwitch");
        }
    }
}