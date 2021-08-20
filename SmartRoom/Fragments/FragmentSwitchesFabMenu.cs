using AndroidX.Fragment.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;

namespace SmartRoom.Fragments
{
    public class FragmentSwitchesFabMenu : Fragment
    {
        public event EventHandler ShowDialog;
        private bool _isFabOpen;

        public FragmentSwitchesFabMenu()
        {
            _isFabOpen = false;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.switches_fab_menu, container, false);
            view.Click += View_Click;

            var fab = view.FindViewById<FloatingActionButton>(Resource.Id.fab_switches);
            var fabAddSwitch = view.FindViewById<FloatingActionButton>(Resource.Id.fab_add_switch);
            var fabAddSilder = view.FindViewById<FloatingActionButton>(Resource.Id.fab_add_slider);
            var fabAddRgb = view.FindViewById<FloatingActionButton>(Resource.Id.fab_add_rgb);

            if (fab != null) fab.Click += Fab_Click;
            if (fabAddSwitch != null) fabAddSwitch.Click += MenuFab_Click;
            if (fabAddSilder != null) fabAddSilder.Click += MenuFab_Click;
            if (fabAddRgb != null) fabAddRgb.Click += MenuFab_Click;

            return view;
        }

        private void Fab_Click(object sender, EventArgs e)
        {
            View view = ((FloatingActionButton)sender).RootView;

            if (_isFabOpen)
                CloseFabMenu(view);
            else
                OpenFabMenu(view);
        }

        private void OpenFabMenu(View view)
        {
            view.FindViewById<FloatingActionButton>(Resource.Id.fab_switches).Animate().Rotation(90f);

            var layoutFabSwitch = view.FindViewById<LinearLayout>(Resource.Id.layout_fab_switch);
            var layoutFabSilder = view.FindViewById<LinearLayout>(Resource.Id.layout_fab_slider);
            var layoutFabRgb = view.FindViewById<LinearLayout>(Resource.Id.layout_fab_rgb);

            layoutFabSwitch.Visibility = ViewStates.Visible;
            layoutFabSilder.Visibility = ViewStates.Visible;
            layoutFabRgb.Visibility = ViewStates.Visible;
            layoutFabSwitch.Animate().TranslationY(-3.15f * Resources.GetDimension(Resource.Dimension.fab_menu_distance));
            layoutFabSilder.Animate().TranslationY(-2.15f * Resources.GetDimension(Resource.Dimension.fab_menu_distance));
            layoutFabRgb.Animate().TranslationY(-1.15f * Resources.GetDimension(Resource.Dimension.fab_menu_distance));
            _isFabOpen = true;
        }

        private void View_Click(object sender, EventArgs e)
        {
            if (_isFabOpen)
                CloseFabMenu((View)sender);
        }

        private void CloseFabMenu(View view)
        {
            view.FindViewById<FloatingActionButton>(Resource.Id.fab_switches).Animate().Rotation(0f);

            var layoutFabSwitch = view.FindViewById<LinearLayout>(Resource.Id.layout_fab_switch);
            var layoutFabSilder = view.FindViewById<LinearLayout>(Resource.Id.layout_fab_slider);
            var layoutFabRgb = view.FindViewById<LinearLayout>(Resource.Id.layout_fab_rgb);

            layoutFabSwitch.Animate().TranslationY(0f).WithEndAction(new Adapters.JavaRunnableAdapter(() => { layoutFabSwitch.Visibility = ViewStates.Gone; }));
            layoutFabSilder.Animate().TranslationY(0f).WithEndAction(new Adapters.JavaRunnableAdapter(() => { layoutFabSilder.Visibility = ViewStates.Gone; }));
            layoutFabRgb.Animate().TranslationY(0f).WithEndAction(new Adapters.JavaRunnableAdapter(() => { layoutFabRgb.Visibility = ViewStates.Gone; }));
            _isFabOpen = false;
        }

        private void MenuFab_Click(object sender, EventArgs e)
        {
            var s = sender as FloatingActionButton;

            DialogFragment popup = null;
            if (s.Id == Resource.Id.fab_add_switch) popup = new FragmentPopupSwitch();
            //else if (s.Id == Resource.Id.fab_add_slider) popup = new FragmentSliderPopup();
            //else if (s.Id == Resource.Id.fab_add_rgb) popup = new FragmentRgbPopup();

            if (popup != null)
            {
                if (_isFabOpen)
                    CloseFabMenu(s.RootView);
                ShowDialog?.Invoke(popup, null);
            }
        }
    }
}