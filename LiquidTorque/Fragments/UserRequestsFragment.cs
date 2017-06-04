using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using HockeyApp;
using liquidtorque.Activities;
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;
using Fragment = Android.Support.V4.App.Fragment;

namespace liquidtorque.Fragments
{
    public class UserRequestsFragment : Fragment
    {
        DataManager _dataManager = DataManager.GetInstance();

        private View _view;
        private SwipeRefreshLayout _refresher;
        private ListView _requestsListView;
        private Button _btnAddRequests;
        private AppBarLayout _toolBarLayout;
        private string _username;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Bundle bundle = Arguments;
            // Create your fragment here
            _username = bundle.GetString("username");
        }

        public static UserRequestsFragment NewInstance()
        {
            var userRequestsFragment = new UserRequestsFragment() { Arguments = new Bundle() };
            return userRequestsFragment;
        }

        public override async void OnResume()
        {
            base.OnResume();
            var requests = await FetchUserRequests(_username);
            //now pass it to the adapater
            if (_requestsListView != null)
            {
                _requestsListView.Adapter = new RequestsListViewAdapter(this.Activity, requests);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {


                // Use this to return your custom view for this Fragment
                if (_view == null)
                {
                    _view = inflater.Inflate(Resource.Layout.user_requests, container, false);
                    _btnAddRequests = _view.FindViewById<Button>(Resource.Id.btnAddRequest);
                    _requestsListView = _view.FindViewById<ListView>(Resource.Id.requestsListView);
                    _toolBarLayout = _view.FindViewById<AppBarLayout>(Resource.Id.toolbar_layout);
                    //lets hide the toolbar layout in the fragment
                    _toolBarLayout.Visibility = ViewStates.Gone;
                    _btnAddRequests.Click += BtnAddNewRequests;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Unable to load user's requests {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
            return _view;
        }

        //add new requests when cliked from user profile
        private void BtnAddNewRequests(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(NewRequestActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }

        //data fetching methods
        async Task<List<Tuple<string, string, string, string>>> FetchUserRequests(string username)
        {
            AndHUD.Shared.Show(Context, "Loading user requests");
            List<Tuple<string, string, string, string>> requests = await _dataManager.GetUserRequests(username);
            AndHUD.Shared.Dismiss(Context); //dismiss the hud
            return requests;
        }
    }
}