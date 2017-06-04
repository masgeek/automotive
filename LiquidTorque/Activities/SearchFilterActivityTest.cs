using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using liquidtorque.Adapters;
using liquidtorque.Fragments;

using Frag = Android.Support.V4.App.Fragment;

namespace liquidtorque.Activities
{
    [Activity(Label = "Search Vehicles")]
    public class SearchFilterActivityTest : BaseActivity
    {
        private bool _clearFilterVisible;
        private IMenuItem _clearFilterMenuItem;
        
        protected override int LayoutResource
        {
            get { return Resource.Layout.test_layout; }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            //initialize the toolbar
            SetSupportActionBar(Toolbar);

            var fragments = new Frag[]
            {
                new HelpFragment(), 
                new UserSettingsFragment(), 
            };

            var titles = CharSequence.ArrayFromStringArray(new[]
                {
                    "People",
                    "Films",
                });

            var viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
            viewPager.Adapter = new TabsFragmentPagerAdapter(SupportFragmentManager, fragments, titles);

            // Give the TabLayout the ViewPager
            var tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            tabLayout.SetupWithViewPager(viewPager);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.search_filter_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            //editProfile = FindViewById(Resource.Id.menu_edit);

            _clearFilterMenuItem = menu.FindItem(Resource.Id.menu_clear_filter);


            _clearFilterMenuItem.SetVisible(_clearFilterVisible);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                Finish();

            return base.OnOptionsItemSelected(item);
        }
    }
}