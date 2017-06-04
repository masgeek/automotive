using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using HockeyApp.Android.Metrics;
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Listener;

namespace liquidtorque.Activities
{
    [Activity(Label = "Owner's Autofolio")]
    public class OwnerAutofolioActivity : BaseActivity
    {
        List<Tuple<string, string>> _items;
        private List<Tuple<string, string, string, string, Uri, string>> _cars;
        readonly DataManager _dataManager = DataManager.GetInstance();
        readonly VehicleDataCache _vCache = VehicleDataCache.GetInstance();
        //activity intent
        private Intent _intent;
        //view components
        private ListView _vehicleListView;
        private SwipeRefreshLayout _refresher;
        private Button _btnRequest;
        private Button _btnMessage;
        private Button _btnSell;


        private bool pauseOnScroll = true;
        private bool pauseOnFling = true;
        private string _username;

        protected override int LayoutResource
        {
            get { return Resource.Layout.inventory_fragment; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //initialize the toolbar
            SetSupportActionBar(Toolbar);
            _cars = new List<Tuple<string, string, string, string, Uri, string>>();
            if (Intent != null)
            {
                _username = Intent.GetStringExtra("username");
            }

            // Create your application here
            var toolbar = FindViewById<AppBarLayout>(Resource.Id.toolbar_layout);
            _vehicleListView = FindViewById<ListView>(Resource.Id.vehicleListView);
            _refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);

            _btnRequest = FindViewById<Button>(Resource.Id.btnRequest);
            _btnMessage = FindViewById<Button>(Resource.Id.btnMessage);
            _btnSell = FindViewById<Button>(Resource.Id.btnSell);

            _vehicleListView.FastScrollEnabled = false;
            _vehicleListView.FastScrollAlwaysVisible = false;

            //assign click actions to the buttons
            _vehicleListView.ItemClick += VehicleListViewItemClick;

            //add click functions and other properties for pull down to refresh
            _refresher.SetColorSchemeColors(Resource.Color.ltyellow,
                Resource.Color.accent,
                Resource.Color.appBackground,
                Resource.Color.ltyellow);
            _refresher.Refresh += HandleViewRefresh;

            //set toolbar to be hidden in the fragment view
            toolbar.Visibility = ViewStates.Visible;
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await LoadAllVehicles();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ApplyScrollListener();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ImageLoader.Instance.ClearMemoryCache(); //clear memory cache of the image loader
            _cars.Clear();
        }

        #region click action handlers

        private async void HandleViewRefresh(object sender, EventArgs e)
        {
            //reload the data
            Toast.MakeText(Application.Context, "Refreshing vehicle list", ToastLength.Short).Show();
            await LoadAllVehicles();
            //signal the refresher that the refreshing is complete
            _refresher.Refreshing = false;

        }

        private void VehicleListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                var carPosition = e.Position;
                var carname = _cars[carPosition].Item2;
                Toast.MakeText(Application.Context,
                    string.Format("To view {0} details, go to the home page and search for it", carname),
                    ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                var message = string.Format("Image exception {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);

                Toast.MakeText(Application.Context, "Unable to load vehicle profile, please try again later",
                    ToastLength.Short).Show();
            }
        }

        //handles the back arrow on the action bar
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                Finish();

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        /// <summary>
        /// Fetch all the initial vehicles
        /// </summary>

        /// <summary>
        /// Fetch all the initial vehicles
        /// </summary>
        private async Task<bool> LoadAllVehicles(bool refreshDataFromNetwork = false)
        {
            try
            {
                AndHUD.Shared.Show(this, "Loading vehicles", -1, MaskType.Black);
                List<Tuple<string, string, string, string, Uri, string>> liveCars;

                //liveCars = await _dataManager.GetVehcileProfile(30, 0);
                liveCars = await _dataManager.GetUserVehicles(_username, 1000);

                AndHUD.Shared.Dismiss(this);
                PopulateListView(liveCars);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Vehicle loading exception " + ex.Message + ex.StackTrace);
                Toast.MakeText(Application.Context, "Unable to load vehicles", ToastLength.Short).Show();
                MetricsManager.TrackEvent(string.Format("Unable to load vehicles {0}{1}", ex.Message, ex.StackTrace));
                AndHUD.Shared.Dismiss(this);
            }

            return false;
        }

        private void PopulateListView(List<Tuple<string, string, string, string, Uri, string>> localCars)
        {
            //insert to cache table
            _cars = localCars;
            Toast.MakeText(Application.Context, string.Format("Loading {0} vehicles, please wait... ", _cars.Count),
                ToastLength.Short).Show();
            _vehicleListView.Adapter = new ImageAdapter(context: this, cars: localCars);
        }

        private void ApplyScrollListener()
        {
            _vehicleListView.SetOnScrollListener(new PauseOnScrollListener(ImageLoader.Instance, pauseOnScroll,
                pauseOnFling));
        }
    }
}