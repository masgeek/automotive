using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using UniversalImageLoader.Core;

namespace liquidtorque.Activities
{
    [Activity(Label = "Owner Profile")]
    public class OwnerProfileActivity : BaseActivity
    {
        string _facebook;
        string _instagram;
        string _twitter;
        string _linkedin;

        DataManager _dataManager = DataManager.GetInstance();
        SocialMediaLinks socialMediaLinks = new SocialMediaLinks();
        private UserProfileData _userProfileData;
        ImageLoader _imageLoader = ImageLoader.Instance;

        //view elements
        private ImageButton _facebookImageButton;
        private ImageButton _twitterImageButton;
        private ImageButton _instagramImageButton;
        private ImageButton _linkedinImageButton;

        private Button _btnAutoFolio;
        private Button _btnRequests;

        private ImageView _profilePhotoView;


        //dynamic text
        private TextView _usernameTextView;
        private TextView _countryATextView;
        private TextView _firstNameTextView;
        private TextView _lastNameTextView;
        private TextView _companyNameTextView;
        private TextView _addressTextView;
        private TextView _cityTextView;
        private TextView _countryBTextView;
        private TextView _emailTextView;
        private TextView _phoneTextView;
        private string _username;

        //company related labels
        private TextView _companyNameLabel;
        private TextView _addressLabel;

        protected override int LayoutResource
        {
            get { return Resource.Layout.profile_view; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {

            //instantiate classes
            _userProfileData = UserProfileData.GetInstance();
            base.OnCreate(savedInstanceState);
            //get the owner username
            if (Intent != null) _username = Intent.GetStringExtra("username");

            //* Create your application here
            _facebookImageButton = FindViewById<ImageButton>(Resource.Id.btnFacebook);
            _twitterImageButton = FindViewById<ImageButton>(Resource.Id.btnTwitter);
            _instagramImageButton = FindViewById<ImageButton>(Resource.Id.btnInstagram);
            _linkedinImageButton = FindViewById<ImageButton>(Resource.Id.btnLinkedin);

            _btnAutoFolio = FindViewById<Button>(Resource.Id.btnAutoFolio);
            _btnRequests = FindViewById<Button>(Resource.Id.btnRequests);

            _profilePhotoView = FindViewById<ImageView>(Resource.Id.user_profile_photo);

            _usernameTextView = FindViewById<TextView>(Resource.Id.username);
            _countryATextView = FindViewById<TextView>(Resource.Id.user_country);
            _firstNameTextView = FindViewById<TextView>(Resource.Id.firstName);
            _lastNameTextView = FindViewById<TextView>(Resource.Id.lastName);
            _cityTextView = FindViewById<TextView>(Resource.Id.city);
            _countryBTextView = FindViewById<TextView>(Resource.Id.country);
            _emailTextView = FindViewById<TextView>(Resource.Id.email);
            _phoneTextView = FindViewById<TextView>(Resource.Id.phone);

            _profilePhotoView = FindViewById<ImageView>(Resource.Id.user_profile_photo);


            //Company stuff
            _companyNameTextView = FindViewById<TextView>(Resource.Id.companyName);
            _addressTextView = FindViewById<TextView>(Resource.Id.address);
            _companyNameLabel = FindViewById<TextView>(Resource.Id.lblCompanyName);
            _addressLabel = FindViewById<TextView>(Resource.Id.lblAddress);
            //hide the above initialy
            _companyNameTextView.Visibility = ViewStates.Invisible;
            _companyNameLabel.Visibility = ViewStates.Invisible;
            _addressTextView.Visibility = ViewStates.Invisible;
            _addressLabel.Visibility = ViewStates.Invisible;

            //social media click action
            _facebookImageButton.Click += FaceBookClicked;
            _instagramImageButton.Click += InstagramClicked;
            _twitterImageButton.Click += TwitterClicked;
            _linkedinImageButton.Click += LinkedInClicked;

            //phone and email clicks
            _phoneTextView.Click += PhoneNumberClicked;
            _emailTextView.Click += EmailAddressClicked;

            //button clicks
            _btnRequests.Click += BtnRequestsClicked;
            _btnAutoFolio.Click += AutofolioClicked;
        }

        #region click actions
        private void AutofolioClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(OwnerAutofolioActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }

        private void BtnRequestsClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(OwnerRequestsActivity));
            intent.PutExtra("username", _username);
            StartActivity(intent);
        }
        #endregion
        #region data actions
        private void EmailAddressClicked(object sender, EventArgs e)
        {
            var emailAddress = _emailTextView.Text;
            if (string.IsNullOrEmpty(emailAddress))
            {
                Toast.MakeText(this, "Email address not provided", ToastLength.Short).Show();
            }
            else
            {
                var uri = Android.Net.Uri.Parse("mailto:" + emailAddress);
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }

        private void PhoneNumberClicked(object sender, EventArgs e)
        {
            var phoneNo = _phoneTextView.Text;
            if (string.IsNullOrEmpty(phoneNo))
            {
                Toast.MakeText(this, "Phone number not provided", ToastLength.Short).Show();
            }
            else
            {
                var uri = Android.Net.Uri.Parse("tel:"+phoneNo);
                var intent = new Intent(Intent.ActionDial, uri);
                StartActivity(intent);
            }

        }

        private void LinkedInClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_linkedin))
            {
                Toast.MakeText(this,"Linked username not provided", ToastLength.Short).Show();
            }
            else
            {
                var link = socialMediaLinks.OpenLinkedin(_linkedin);
                var uri = Android.Net.Uri.Parse(link.ToString());
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }

        void TwitterClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_twitter))
            {
                Toast.MakeText(this, "Twitter username not provided", ToastLength.Short).Show();
            }
            else
            {
                var link = socialMediaLinks.OpenTwitter(_twitter);
                var uri = Android.Net.Uri.Parse(link.ToString());
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }

        void InstagramClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_instagram))
            {
                Toast.MakeText(this, "Instagram username not provided", ToastLength.Short).Show();
            }
            else
            {
                var link = socialMediaLinks.OpenInstagram(_instagram);
                var uri = Android.Net.Uri.Parse(link.ToString());
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }

        void FaceBookClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_facebook))
            {
                Toast.MakeText(this, "Facebook username not provided", ToastLength.Short).Show();
            }
            else
            {
                var link = socialMediaLinks.OpenFacebook(_facebook);
                var uri = Android.Net.Uri.Parse(link.ToString());
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }
        #endregion Data actions


        protected override void OnResume()
        {
            base.OnResume();
            //lets populate the labels
            PopulateFields(_username);
        }

        async void PopulateFields(string username)
        {
            try
            {
                string accountType = "Private Party";

                if (string.IsNullOrEmpty(username)) return; //return if null


                Dictionary<string, object> ownerData = await _userProfileData.FetchUserProfileData(userName: username);
                if (ownerData != null)
                {
                    //get the profile picture
                    var profilePicUrl = await _dataManager.FetchUserProfileImage(username);
                    //use cache image
                    if (profilePicUrl != null)
                    {
                        _imageLoader.DisplayImage(profilePicUrl.AbsoluteUri, _profilePhotoView);
                    }

                    _usernameTextView.Text = username;

                    if (ownerData.ContainsKey("usertype"))
                    {
                        accountType = ownerData["usertype"].ToString();
                    }

                    //if dealer get dealer profile
                    if (string.Equals(accountType, "Dealer"))
                    {
                        _companyNameTextView.Visibility = ViewStates.Visible;
                        _companyNameLabel.Visibility = ViewStates.Visible;
                        _addressTextView.Visibility = ViewStates.Visible;
                        _addressLabel.Visibility = ViewStates.Visible;

                        var dealerData = await _userProfileData.FetchDealerProfile(userName: username);

                        if (dealerData.ContainsKey("address"))
                        {
                            _addressTextView.Text = dealerData["address"].ToString();
                        }
                        if (dealerData.ContainsKey("company"))
                        {
                            _companyNameTextView.Text = dealerData["company"].ToString();
                        }
                    }
                    else //private party specific views and data
                    {
                        //Hide the fields for the dealer data
                        _companyNameTextView.Visibility = ViewStates.Gone;
                        _companyNameLabel.Visibility = ViewStates.Gone;
                        _addressTextView.Visibility = ViewStates.Gone;
                        _addressLabel.Visibility = ViewStates.Gone;
                    }

                    if (ownerData.ContainsKey("city"))
                    {
                        _cityTextView.Text = ownerData["city"].ToString();
                    }


                    if (ownerData.ContainsKey("lastname"))
                    {
                        _lastNameTextView.Text = ownerData["lastname"].ToString();
                    }


                    if (ownerData.ContainsKey("firstname"))
                    {
                        _firstNameTextView.Text = ownerData["firstname"].ToString();
                    }

                    if (ownerData.ContainsKey("country"))
                    {
                        var country = ownerData["country"].ToString();
                        _countryBTextView.Text = country;
                        _countryATextView.Text = country;
                    }

                    if (ownerData.ContainsKey("email"))
                    {
                        _emailTextView.Text = ownerData["email"].ToString();
                    }

                    if (ownerData.ContainsKey("phone"))
                    {
                        _phoneTextView.Text = ownerData["phone"].ToString();
                    }

                    if (ownerData.ContainsKey("facebook"))
                    {
                        _facebook = ownerData["facebook"].ToString();
                    }

                    if (ownerData.ContainsKey("instagram"))
                    {
                        _instagram = ownerData["instagram"].ToString();
                    }

                    if (ownerData.ContainsKey("twitter"))
                    {
                        _twitter = ownerData["twitter"].ToString();
                    }

                    if (ownerData.ContainsKey("linkedin"))
                    {
                        _linkedin = ownerData["linkedin"].ToString();
                    }

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
                Console.WriteLine(string.Format("Error happened please check asap {0} {1}", ex.Message, ex.StackTrace));
            }

        }
    }
}
 