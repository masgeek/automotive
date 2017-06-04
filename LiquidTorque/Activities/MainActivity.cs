using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;

using liquidtorque.Fragments;
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using HockeyApp.Android.Metrics;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using Parse;
using UniversalImageLoader.Core;
using Exception = Java.Lang.Exception;

namespace liquidtorque.Activities
{
    [Activity(Label = "Liquid Torque")]
    //[Activity(Label = "Liquid Torque", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/Icon")]
    public class MainActivity : BaseActivity
    {
        private static long _backPressed = DateTime.Now.Millisecond;
        DrawerLayout _drawerLayout;
        NavigationView _navigationView;
        //private View _editProfile;
        private IMenuItem _editProfileMenuItem;
        private IMenuItem _addRequestItem;
        private Bundle _dataBundle;
        private DataManager _dataManager;

        int _oldPosition = -1;
        private string _viewTitle = "Auto Folio";
        private string _username;
        private bool _editItemVisible;
        private bool _requestItemVisible;
        private bool _doubleBackToExitPressedOnce;

        protected override int LayoutResource
        {
            get { return Resource.Layout.main; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //initialize the toolbar
            SetSupportActionBar(Toolbar);
            Toolbar.Title = _viewTitle;

            _dataManager = DataManager.GetInstance();
            _dataBundle = new Bundle();
            //get current logged in user
            var currentUser = ParseUser.CurrentUser;

            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);


            //Set hamburger items menu
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);

            //setup navigation view
            _navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            // View h = FindViewById(Resource.Id.menu_edit);

            //handle navigation
            _navigationView.NavigationItemSelected += (sender, e) =>
            {

                e.MenuItem.SetChecked(true);
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_home:
                        ListItemClicked(0);
                        break;
                    case Resource.Id.nav_user_profile:
                        ListItemClicked(1);
                        break;
                    case Resource.Id.nav_user_inventory:
                        ListItemClicked(2);
                        break;
                    case Resource.Id.nav_user_requests:
                        ListItemClicked(3);
                        break;
                    case Resource.Id.nav_user_settings:
                        ListItemClicked(4);
                        break;
                    case Resource.Id.nav_help:
                        ListItemClicked(5);
                        break;
                    case Resource.Id.nav_signout:
                        ListItemClicked(6);
                        break;
                }


                //Snackbar.Make(_drawerLayout, "You selected: " + e.MenuItem.TitleFormatted, Snackbar.LengthLong).Show();

                //lets put the data in the bundle we created
                //we will pass this bundlle to all needed activities
                _username = currentUser.Username;
                _dataBundle.PutString("username", currentUser.Username); //add the username
                _dataBundle.PutString("userid", currentUser.ObjectId);

                _drawerLayout.CloseDrawers();

            };



            //set the build number in the menu
            var buildMenu = _navigationView.Menu.FindItem(Resource.Id.app_build);
            buildMenu.SetTitle(string.Format("Version {0}", HelperClass.BuildNumber));

            //if first time you will want to go ahead and click first item.
            if (savedInstanceState == null)
            {
                ListItemClicked(0);
            }
        }

        private void ListItemClicked(int position)
        {
            try
            {
                _editItemVisible = false;
                _requestItemVisible = false;
                //this way we don't load twice, but you might want to modify this a bit.
                if (position == _oldPosition) //perhaps allow loading of homepage again???
                    return;

                _oldPosition = position;
                Android.Support.V4.App.Fragment interfaceFragment = null;
                switch (position)
                {
                    case 0: //Home page
                        //interfaceFragment = HomePageFragment.NewInstance();
                        interfaceFragment = HomePageFragment.NewInstance();
                        _viewTitle = "Auto Folio";
                        break;
                    case 1: //My Profile
                        interfaceFragment = UserProfileFragment.NewInstance();                   
                        //interfaceFragment = TestLayoutFragment.NewInstance();
                        interfaceFragment.Arguments = _dataBundle;
                        _viewTitle = "My Profile";
                        _editItemVisible = true;
                        break;
                    case 2: //MY Inventory
                        interfaceFragment = UserInventoryFragment.NewInstance();
                        interfaceFragment.Arguments = _dataBundle;
                        _viewTitle = "My Inventory";
                        break;
                    case 3: //MY Requests
                        interfaceFragment = UserRequestsFragment.NewInstance();
                        interfaceFragment.Arguments = _dataBundle;
                        _viewTitle = "Requests";
                        _requestItemVisible = true;
                        break;
                    case 4: //Settings
                        interfaceFragment = UserSettingsFragment.NewInstance();
                        interfaceFragment.Arguments = _dataBundle;
                        _viewTitle = "Settings";
                        break;
                    case 5: //HELP
                        interfaceFragment = HelpFragment.NewInstance();
                        _viewTitle = "Help";
                        break;
                    case 6: //Sign out
                        //confirm user action
                        SignOutUser();
                        break;
                }

                Toolbar.Title = _viewTitle;
                if (_editProfileMenuItem != null)
                {
                    _editProfileMenuItem.SetVisible(_editItemVisible);
                }

                if (_addRequestItem != null)
                {
                    _addRequestItem.SetVisible(_requestItemVisible);
                }
                SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, interfaceFragment)
                    .Commit();
            }
            catch (Exception ex)
            {
                var message = string.Format("Error loading home details {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
                Toast.MakeText(this, "Vehicle not details lodaded successfully, please try again " + ex.Message,
                    ToastLength.Short);
                ListItemClicked(0); //reset to homepage
            }
        }


        /// <Docs>The options menu in which you place your items.</Docs>
        /// <returns>To be added.</returns>
        /// <summary>
        /// This is the menu for the Toolbar/Action Bar to use
        /// </summary>
        /// <param name="menu">Menu.</param>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }


        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            //editProfile = FindViewById(Resource.Id.menu_edit);

            _editProfileMenuItem = menu.FindItem(Resource.Id.menu_edit);
            _addRequestItem = menu.FindItem(Resource.Id.menu_add_request);

            //set initial visiblity
            _addRequestItem.SetVisible(_requestItemVisible);
            _editProfileMenuItem.SetVisible(_editItemVisible);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override void OnBackPressed()
        {
            //handle back key pressing
            if (_doubleBackToExitPressedOnce)
            {
                base.OnBackPressed();
                Java.Lang.JavaSystem.Exit(0);
                return;
            }
            Toast.MakeText(this, "Press once again to exit", ToastLength.Short).Show();
            _doubleBackToExitPressedOnce = true;
            new Handler().PostDelayed(() => { _doubleBackToExitPressedOnce = false; }, 2000);
                //reset to false after 2 seconds

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;
            switch (item.ItemId)
            {

                case Android.Resource.Id.Home:
                    _drawerLayout.OpenDrawer(GravityCompat.Start);
                    return true;
                case Resource.Id.menu_edit:
                    intent = new Intent(this, typeof(EditProfileActivity));
                    intent.PutExtra("username", _username);
                    StartActivity(intent);
                    break;
                case Resource.Id.menu_add_request:
                    intent = new Intent(this, typeof(NewRequestActivity));
                    intent.PutExtra("username", _username);
                    StartActivity(intent);
                    break;
                case Resource.Id.menu_search:
                    intent = new Intent(this, typeof(SearchFilterActivity));
                    intent.PutExtra("username", _username);
                    //StartActivity(intent);
                    //start activity while expecting results
                    StartActivityForResult(intent, 4);
                    break;
                default:
                    Toast.MakeText(this, "Action not available", ToastLength.Short).Show();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }

        public void SignOutUser()
        {
            //stop downloading of teh images
            ImageLoader.Instance.Stop();
            //clear the caches
            //ImageLoader.Instance.ClearDiskCache();
            ImageLoader.Instance.ClearMemoryCache();
            //sign out functionality
            _dataManager.logoutUser(); //logout and end the current user session
            //@TODO also clear the login data from the app preferences 
            //show signout alert
            Toast.MakeText(Application.Context, "Signing out of the application", ToastLength.Short).Show();
           this.Finish(); //end current activity
            //end the activity and return to login page
            var intent = new Intent(this, typeof(LoginActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }
    }
}

