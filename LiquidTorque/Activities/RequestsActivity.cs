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
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;

namespace liquidtorque.Activities
{
    [Activity(Label = "Requests")]
    public class RequestsActivity : BaseActivity
    {
        DataManager _dataManager = DataManager.GetInstance();
        private SwipeRefreshLayout _refresher;
        private ListView _requestsListView;
        private Button _btnAddRequests;
        private string _username;
        protected override int LayoutResource
        {
            get { return Resource.Layout.user_requests; }
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
            _btnAddRequests = FindViewById<Button>(Resource.Id.btnAddRequest);
            _requestsListView = FindViewById<ListView>(Resource.Id.requestsListView);
            _btnAddRequests.Click += BtnAddNewRequests;
        }

        protected override async void OnResume()
        {
            base.OnResume();
            var requests = await FetchUserRequests(_username);
            //now pass it to the adapter
            if (_requestsListView != null)
            {
                _requestsListView.Adapter = new RequestsListViewAdapter(this, requests);
            }
        }

        private void BtnAddNewRequests(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(NewRequestActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }


        //handles the back arrow on the action bar
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                Finish();

            return base.OnOptionsItemSelected(item);
        }

        //data fetching methods
        async Task<List<Tuple<string, string, string, string>>> FetchUserRequests(string username)
        {
            AndHUD.Shared.Show(this, "Loading user requests");
            List<Tuple<string, string, string, string>> requests = await _dataManager.GetUserRequests(username);
            AndHUD.Shared.Dismiss(this); //dismiss the hud
            return requests;
        }
    }
}