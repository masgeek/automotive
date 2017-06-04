using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using HockeyApp;
using liquidtorque.Activities;
using liquidtorque.Adapters;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;
using Parse;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Listener;
using Fragment = Android.Support.V4.App.Fragment;

namespace liquidtorque.Fragments
{
    public class HomePageFragmentOld : Fragment
    {
        private string _modelToSearch;
        private string _makeToSearch;
        private string _yearToSearch;
        private bool _filterEnabled;
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

        public static HomePageFragmentOld NewInstance()
        {
            var homePageFragment = new HomePageFragmentOld {Arguments = new Bundle()};
            return homePageFragment;
        }

        public override void OnResume()
        {
            base.OnResume();
            ApplyScrollListener();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                // Create your fragment here
                if (ParseUser.CurrentUser != null)
                {
                    var currentUser = ParseUser.CurrentUser;
                    _username = currentUser.Username;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching current user " + ex.Message + ex.StackTrace);
            }
        }

        public override async void OnStart()
        {
            base.OnStart();
            //lets check if the filter values are there
            _filterEnabled = VehicleFilterStrings.FiltersSet;

            if (_filterEnabled)
            {
                _modelToSearch = VehicleFilterStrings.Model;
                _makeToSearch = VehicleFilterStrings.Make;
                _yearToSearch = VehicleFilterStrings.Year;

                if (string.Equals(_modelToSearch, "All")) _modelToSearch = null;
                if (string.IsNullOrEmpty(_makeToSearch)) _makeToSearch = "All";
                if (string.Equals(_yearToSearch, "All")) _yearToSearch = null;
                //check if the string is for all or specifics
                //load the vehicles
                await LoadAllVehicles(refreshDataFromNetwork: true); //callthe network stuff
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _cars = new List<Tuple<string, string, string, string, Uri, string>>();
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.homepage_fragment, container, false);

            //get the elements
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
            _btnRequest.Click += BtnRequestClick;
            _btnMessage.Click += BtnMessageClick;
            _btnSell.Click += BtnSellClick;

            //add click functions and other properties for pull down to refresh
            _refresher.SetColorSchemeColors(
                Resource.Color.ltyellow,
                Resource.Color.accent,
                Resource.Color.appBackground,
                Resource.Color.ltyellow);

            _refresher.Refresh += HandleViewRefresh;

            return view;
        }

        private async void HandleViewRefresh(object sender, EventArgs e)
        {
            //reload the data
            await LoadAllVehicles(refreshDataFromNetwork: true);
            //signal the refresher that the refreshing is complete
            _refresher.Refreshing = false;

        }

        public override async void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            await LoadAllVehicles();
        }


        #region click action handlers

        private void VehicleListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //@TODO add check for payment and exception of vehicle owner's
            try
            {
                var carPosition = e.Position;
                Toast.MakeText(Application.Context, _cars[carPosition].Item2.ToString(), ToastLength.Long).Show();
                //load details of clicked car
                _intent = new Intent(Context, typeof(VehicleDetailsActivity));
                //_intent = new Intent(Context, typeof(TestActivity));
                _intent.PutExtra("VehicleID", _cars[carPosition].Item1);
                //pass the vehicle object id to the next activity
                StartActivity(_intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Image exception {0} {1}", ex.Message, ex.StackTrace));
                Toast.MakeText(Application.Context, "Unable to load vehicle profile, please try again later",
                    ToastLength.Short).Show();
            }
        }


        private void BtnSellClick(object sender, EventArgs e)
        {
            //create a new activity for the sell my car action
            _intent = new Intent(Context, typeof(SellMyCar));
            StartActivity(_intent);
        }

        private void BtnMessageClick(object sender, EventArgs e)
        {
            var intent = new Intent(Context, typeof(MessageInboxActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }

        private void BtnRequestClick(object sender, EventArgs e)
        {
            //Toast.MakeText(Application.Context, "Under implementation", ToastLength.Short).Show();
            var intent = new Intent(Context, typeof(RequestsActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }

        #endregion

        private async Task<List<Tuple<string, string, string, string, Uri, string>>> FetchCarsFromNetwork()
        {
            List<Tuple<string, string, string, string, Uri, string>> liveCars = null;


            if (_filterEnabled)
            {
                string message =
                    string.Format("Searching based on the following parameters: Model {0} Make {1} Year {2}",
                        _makeToSearch, _modelToSearch, _yearToSearch);


                AndHUD.Shared.Show(Context, message, -1, MaskType.Black);


                liveCars = await _dataManager.SearchVehcileProfile(_makeToSearch, _modelToSearch, _yearToSearch);
                AndHUD.Shared.Dismiss(Context);

            }
            else
            {
                liveCars = await _dataManager.GetVehcileProfile(30, 0);
            }

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
            Toast.MakeText(Application.Context, string.Format("Loading {0} vehicles, please wait... ", _cars.Count),
                ToastLength.Short).Show();
            _vehicleListView.Adapter = new HomeListViewAdapter(context: this.Activity, cars: localCars);

        }

        /// <summary>
        /// Fetch all the initial vehicles
        /// </summary>
        private async Task<bool> LoadAllVehicles(bool refreshDataFromNetwork = false)
        {
            try
            {
                List<Tuple<string, string, string, string, Uri, string>> liveCars;
                //Toast.MakeText(Application.Context, "Fetching vehicle list, please wait... ", ToastLength.Short).Show();


                if (refreshDataFromNetwork || _filterEnabled)
                {
                    liveCars = await FetchCarsFromNetwork();
                }
                else
                {
                    liveCars = _vCache.FetchCachedVehiclesList();
                    if (liveCars != null && liveCars.Count > 0)
                    {
                        PopulateListView(liveCars);
                    }
                    else
                    {
                        liveCars = await FetchCarsFromNetwork();
                    }
                }
                PopulateListView(liveCars);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Vehicle loading exception " + ex.Message + ex.StackTrace);
                Toast.MakeText(Application.Context, "Unable to load vehicles", ToastLength.Short).Show();
                MetricsManager.TrackEvent(string.Format("Unable to load vehicles {0}{1}", ex.Message, ex.StackTrace));
            }

            return false;
        }

        private void ApplyScrollListener()
        {
            _vehicleListView.SetOnScrollListener(new PauseOnScrollListener(ImageLoader.Instance, pauseOnScroll, pauseOnFling));
        }
    }
}