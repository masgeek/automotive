using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using HockeyApp;
using liquidtorque.Activities;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using UniversalImageLoader.Core;
using Fragment = Android.Support.V4.App.Fragment;

namespace liquidtorque.Fragments
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class UserProfileFragment : Fragment
    {
        readonly DataManager _dataManager = DataManager.GetInstance();
        readonly ImageLoader _imageLoader = ImageLoader.Instance;
        private UserProfileData userProfileData;
        //view elements
        private ImageButton facebookImageButton;
        private ImageButton twitterImageButton;
        private ImageButton instagramImageButton;
        private ImageButton linkedinImageButton;

        private Button btnAutoFolio;
        private Button btnRequests;

        private ImageView profilePhotoView;

        private TextView usernameTextView;
        private TextView countryATextView;
        private TextView firstNameTextView;
        private TextView lastNameTextView;
        private TextView companyNameTextView;
        private TextView addressTextView;
        private TextView cityTextView;
        private TextView countryBTextView;
        private TextView emailTextView;
        private TextView phoneTextView;

        //company related labels
        private TextView companyNameLabel;
        private TextView addressLabel;
        private string _username;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            userProfileData = new UserProfileData();
        }

        public static UserProfileFragment NewInstance()
        {
            var userProfileFragment = new UserProfileFragment { Arguments = new Bundle() };
            return userProfileFragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //get username from activity data passed
            Bundle bundle = Arguments;
            //int myInt = bundle.getInt(key, defaultValue);
            _username = bundle.GetString("username");
            //int myInt = bundle.getInt(key, defaultValue);
            var view = inflater.Inflate(Resource.Layout.profile_view, container, false);

            //get the view items
            facebookImageButton = view.FindViewById<ImageButton>(Resource.Id.btnFacebook);
            twitterImageButton = view.FindViewById<ImageButton>(Resource.Id.btnTwitter);
            instagramImageButton = view.FindViewById<ImageButton>(Resource.Id.btnInstagram);
            linkedinImageButton = view.FindViewById<ImageButton>(Resource.Id.btnLinkedin);

            btnAutoFolio = view.FindViewById<Button>(Resource.Id.btnAutoFolio);
            btnRequests = view.FindViewById<Button>(Resource.Id.btnRequests);

            profilePhotoView = view.FindViewById<ImageView>(Resource.Id.user_profile_photo);

            usernameTextView = view.FindViewById<TextView>(Resource.Id.username);
            countryATextView = view.FindViewById<TextView>(Resource.Id.user_country);
            firstNameTextView = view.FindViewById<TextView>(Resource.Id.firstName);
            lastNameTextView = view.FindViewById<TextView>(Resource.Id.lastName);
            addressTextView = view.FindViewById<TextView>(Resource.Id.address);
            cityTextView = view.FindViewById<TextView>(Resource.Id.city);
            countryBTextView = view.FindViewById<TextView>(Resource.Id.country);
            emailTextView = view.FindViewById<TextView>(Resource.Id.email);
            phoneTextView = view.FindViewById<TextView>(Resource.Id.phone);

            //Company stuff
            companyNameTextView = view.FindViewById<TextView>(Resource.Id.companyName);
            addressTextView = view.FindViewById<TextView>(Resource.Id.address);
            companyNameLabel = view.FindViewById<TextView>(Resource.Id.lblCompanyName);
            addressLabel = view.FindViewById<TextView>(Resource.Id.lblAddress);
            //hide the above initialy
            companyNameTextView.Visibility = ViewStates.Gone;
            companyNameLabel.Visibility = ViewStates.Gone;
            addressTextView.Visibility = ViewStates.Gone;
            addressLabel.Visibility = ViewStates.Gone;


            //button click event
            btnAutoFolio.Click += BtnAutoFolioClick;
            btnRequests.Click += BtnRequestsClick;
            return view;
        }

        private void BtnRequestsClick(object sender, EventArgs e)
        {

            var intent = new Intent(Activity, typeof(RequestsActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);

        }

        private void BtnAutoFolioClick(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(AutoFolioActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }

        public override void OnResume()
        {
            base.OnResume();
            //lets populate the labels
            PopulateFields(username: _username);
        }
        async void PopulateFields(string username)
        {
            try
            {
                string accountType = "Private Party";

                if (string.IsNullOrEmpty(username)) return; //return if null


                Dictionary<string, object> ownerData = await userProfileData.FetchUserProfileData(userName: username);
                if (ownerData != null)
                {
                    //get the profile picture
                    var profilePicUrl = await _dataManager.FetchUserProfileImage(username);
                    if (profilePicUrl != null)
                    {
                        //use cache image
                        _imageLoader.DisplayImage(profilePicUrl.AbsoluteUri, profilePhotoView);
                    }
                    usernameTextView.Text = username;

                    if (ownerData.ContainsKey("usertype"))
                    {
                        accountType = ownerData["usertype"].ToString();
                    }

                    //if dealer get dealer profile
                    if (string.Equals(accountType, "Dealer"))
                    {
                        companyNameTextView.Visibility = ViewStates.Visible;
                        companyNameLabel.Visibility = ViewStates.Visible;
                        addressTextView.Visibility = ViewStates.Visible;
                        addressLabel.Visibility = ViewStates.Visible;

                        var dealerData = await userProfileData.FetchDealerProfile(userName: username);

                        if (dealerData.ContainsKey("address"))
                        {
                            addressTextView.Text = dealerData["address"].ToString();
                        }
                        if (dealerData.ContainsKey("company"))
                        {
                            companyNameTextView.Text = dealerData["company"].ToString();
                        }
                    }
                    else //private party specific views and data
                    {
                        //Hide the fields for the dealer data
                        companyNameTextView.Visibility = ViewStates.Gone;
                        companyNameLabel.Visibility = ViewStates.Gone;
                        addressTextView.Visibility = ViewStates.Gone;
                        addressLabel.Visibility = ViewStates.Gone;
                    }

                    if (ownerData.ContainsKey("city"))
                    {
                        cityTextView.Text = ownerData["city"].ToString();
                    }


                    if (ownerData.ContainsKey("lastname"))
                    {
                        lastNameTextView.Text = ownerData["lastname"].ToString();
                    }


                    if (ownerData.ContainsKey("firstname"))
                    {
                        firstNameTextView.Text = ownerData["firstname"].ToString();
                    }

                    if (ownerData.ContainsKey("country"))
                    {
                        var country = ownerData["country"].ToString();
                        countryBTextView.Text = country;
                        countryATextView.Text = country;
                    }

                    if (ownerData.ContainsKey("email"))
                    {
                        emailTextView.Text = ownerData["email"].ToString();
                    }
                    else
                    {
                        emailTextView.Text = "N/A";
                    }

                    if (ownerData.ContainsKey("phone"))
                    {
                        phoneTextView.Text = ownerData["phone"].ToString();
                    }


                    if (ownerData.ContainsKey("state"))
                    {
                    }


                    if (ownerData.ContainsKey("isBlocked"))
                    {
                        //is account blocked
                    }

                    /*

            if (user.ContainsKey("facebookUsername"))
            {

            }


            if (user.ContainsKey("instagramUsername"))
            {

            }


            if (user.ContainsKey("twitterUsername"))
            {

            }

            if (user.ContainsKey("linkedinUsername"))
            {

            }

            if (user.ContainsKey("ipAddress"))
            {

            }

            if (user.ContainsKey("macAddress"))
            {

            }
            */

                    if (ownerData.ContainsKey("zipCode"))
                    {

                    }

                    if (ownerData.ContainsKey("appAccessStartDate"))
                    {
                    }

                    if (ownerData.ContainsKey("appAccessEndDate"))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error happend please check asap {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent("Error setting profile information " + ex.Message + ex.StackTrace);
            }

        }
    }
}