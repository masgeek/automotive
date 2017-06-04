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
using HockeyApp.Android.Metrics;
using liquidtorque.Activities;
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Listener;
using Fragment = Android.Support.V4.App.Fragment;

namespace liquidtorque.Fragments
{
    public class UserInventoryFragment : Fragment
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


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public static UserInventoryFragment NewInstance()
        {
            var userInventoryFragment = new UserInventoryFragment() { Arguments = new Bundle() };
            return userInventoryFragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _cars = new List<Tuple<string, string, string, string, Uri, string>>();
            //get username from activity data passed
            Bundle bundle = Arguments;

            //int myInt = bundle.getInt(key, defaultValue);
            _username = bundle.GetString("username");
            // Use this to return your custom view for this Fragment
            var view =  inflater.Inflate(Resource.Layout.inventory_fragment, container, false);
            //get the elements

            var toolbar = view.FindViewById<AppBarLayout>(Resource.Id.toolbar_layout);
            _vehicleListView = view.FindViewById<ListView>(Resource.Id.vehicleListView);
            _refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);

            _btnRequest = view.FindViewById<Button>(Resource.Id.btnRequest);
            _btnMessage = view.FindViewById<Button>(Resource.Id.btnMessage);
            _btnSell = view.FindViewById<Button>(Resource.Id.btnSell);
            _vehicleListView.FastScrollEnabled = true;
            _vehicleListView.FastScrollAlwaysVisible = false;
            //vehicleListView.Fling(500);
            

            //assign click actions to the buttons
            _vehicleListView.ItemClick += VehicleListViewItemClick;

            //add click functions and other properties for pull down to refresh
            _refresher.SetColorSchemeColors(Resource.Color.ltyellow,
                              Resource.Color.accent,
                              Resource.Color.appBackground,
                              Resource.Color.ltyellow);
            _refresher.Refresh += HandleViewRefresh;

            //set toolbasr to be hidden in the fragment view
            toolbar.Visibility = ViewStates.Gone;

            return view;
        }

        public override async void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            await LoadAllVehicles();
        }

        public override void OnResume()
        {
            base.OnResume();
            ApplyScrollListener();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ImageLoader.Instance.ClearMemoryCache(); //clear memory cache of the image loader
            _cars.Clear();
        }

        #region click action handlers
        private async void HandleViewRefresh(object sender, EventArgs e)
        {
            //reload the data
            await LoadAllVehicles();
            //signal the refresher that the refreshing is complete
            _refresher.Refreshing = false;

        }
        private void VehicleListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                var carPosition = e.Position;
                Toast.MakeText(Application.Context, _cars[carPosition].Item2, ToastLength.Long).Show();
                //load details of clicked car
                _intent = new Intent(Context, typeof(OwnerVehicleDetailsActivity));
                //_intent = new Intent(Context, typeof(TestActivity));
                _intent.PutExtra("VehicleID", _cars[carPosition].Item1);
                //pass the vehicle object id to the next activity
                StartActivity(_intent);
            }
            catch (Exception ex)
            {
                var message = string.Format("Image exception {0} {1}", ex.Message,ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);

                Toast.MakeText(Application.Context, "Unable to load vehicle profile, please try again later", ToastLength.Short).Show();
            }
        }
        #endregion

        /// <summary>
        /// Fetch all the initial vehicles
        /// </summary>
        private async Task<bool> LoadAllVehicles()
        {
            try
            {
                Toast.MakeText(Application.Context, "Fetching vehicle list, please wait... ", ToastLength.Short).Show();

                var liveCars = await _dataManager.GetUserVehicles(_username, 1000);
                PopulateListView(liveCars);
            
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Vehicle loading exception " + ex.Message + ex.StackTrace);
            }
            Toast.MakeText(Application.Context,
                string.Format("Unable to load {0}'s vehicles. Please try again", _username),
                ToastLength.Short).Show();
            return false;
        }
        private void PopulateListView(List<Tuple<string, string, string, string, Uri, string>> localCars)
        {
            //insert to cache table
            _cars = localCars;
            Toast.MakeText(Application.Context, string.Format("Loading {0} vehicles, please wait... ", _cars.Count), ToastLength.Short).Show();
            _vehicleListView.Adapter = new ImageAdapter(context: Context, cars: localCars);

        }

        private void ApplyScrollListener()
        {
            _vehicleListView.SetOnScrollListener(new PauseOnScrollListener(ImageLoader.Instance, pauseOnScroll,
                pauseOnFling));
        }
    }
}