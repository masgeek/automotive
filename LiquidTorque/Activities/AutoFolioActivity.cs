using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;
using MetricsManager = HockeyApp.Android.Metrics.MetricsManager;

namespace liquidtorque.Activities
{
    [Activity(Label = "Inventory")]
    public class AutoFolioActivity : BaseActivity
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

            if (Intent != null)
            {
                _username = Intent.GetStringExtra("username");
            }
            
            // Create your application here
            _cars = new List<Tuple<string, string, string, string, Uri, string>>();

            // Use this to return your custom view for this Fragment

            //get the elements
            _vehicleListView = FindViewById<ListView>(Resource.Id.vehicleListView);
            _refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);

            _btnRequest = FindViewById<Button>(Resource.Id.btnRequest);
            _btnMessage = FindViewById<Button>(Resource.Id.btnMessage);
            _btnSell = FindViewById<Button>(Resource.Id.btnSell);
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
            
        }


        protected override async void OnResume()
        {
            base.OnResume();
            await LoadAllVehicles();
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
                _intent = new Intent(this, typeof(OwnerVehicleDetailsActivity));
                //_intent = new Intent(Context, typeof(TestActivity));
                _intent.PutExtra("VehicleID", _cars[carPosition].Item1);
                //pass the vehicle object id to the next activity
                StartActivity(_intent);
            }
            catch (Exception ex)
            {
                var message = string.Format("Image exception {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);

                Toast.MakeText(Application.Context, "Unable to load user vehicles, please try again later", ToastLength.Short).Show();
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
        private async Task<bool> LoadAllVehicles()
        {
            try
            {
                Toast.MakeText(Application.Context, "Fetching vehicle list, please wait... ", ToastLength.Short).Show();

                if (!string.IsNullOrEmpty(_username))
                {
                    var liveCars = await FetchCarsFromNetwork();

                    PopulateListView(liveCars);

                    return true;
                }

            }
            catch (Exception ex)
            {
                var message = string.Format("Vehicle loading exception {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
            Toast.MakeText(Application.Context,
                string.Format("Unable to load {0} user's vehicles. Please try again", _username),
                ToastLength.Short).Show();
            return false;
        }

        private async Task<List<Tuple<string, string, string, string, Uri, string>>> FetchCarsFromNetwork()
        {

            var liveCars = await _dataManager.GetUserVehicles(_username, 1000);

            if (liveCars != null && liveCars.Count > 0)
            {
                //trigger data fetching for local table
                _vCache.CacheVehicleProfiles();
            }

            return liveCars;
        }
        private void PopulateListView(List<Tuple<string, string, string, string, Uri, string>> localCars)
        {
            //insert to cache table
            _cars = localCars;
            Toast.MakeText(Application.Context, string.Format("Loading {0} vehicles, please wait... ", _cars.Count), ToastLength.Short).Show();
            _vehicleListView.Adapter = new HomeListViewAdapter(context: this, cars: localCars);

        }
    }
}