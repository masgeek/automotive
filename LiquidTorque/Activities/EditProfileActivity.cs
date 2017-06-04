using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using UniversalImageLoader.Core;

namespace liquidtorque.Activities
{
    [Activity(Label = "Edit Profile")]
    public class EditProfileActivity : BaseActivity
    {
        private string _username;
        private int _pickImageId = 1000;

        private Bitmap _selectedProfileImage;
        DataManager _dataManager = DataManager.GetInstance();
        ImageLoader _imageLoader = ImageLoader.Instance;
        private readonly UserProfileData _userProfileData = UserProfileData.GetInstance();


        //Edit fields
        private TextView _usernameTextView;
        private TextView _countryTextView;

        private EditText _countryEditText;
        private EditText _firstNameTextView;
        private EditText _lastNameTextView;
        private EditText _companyNameTextView;
        private EditText _addressTextView;
        private EditText _cityTextView;
        private EditText _emailTextView;
        private EditText _phoneTextView;

        private ImageView _profilePhotoView;
        private Button _btnSaveProfile;
        protected override int LayoutResource
        {
            get { return Resource.Layout.profile_edit; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //initialize the toolbar
            SetSupportActionBar(Toolbar);
            //Toolbar.Title = "Edit Profile";

            if (Intent != null) _username = Intent.GetStringExtra("username");

            // Create your application here
            _usernameTextView = FindViewById<TextView>(Resource.Id.username);
            _countryTextView = FindViewById<TextView>(Resource.Id.user_country);

            _countryEditText = FindViewById<EditText>(Resource.Id.editCountry);
            _firstNameTextView = FindViewById<EditText>(Resource.Id.editFirstName);
            _lastNameTextView = FindViewById<EditText>(Resource.Id.editLastName);
            _companyNameTextView = FindViewById<EditText>(Resource.Id.editCompanyName);
            _addressTextView = FindViewById<EditText>(Resource.Id.editAddress);
            _cityTextView = FindViewById<EditText>(Resource.Id.editCity);
            _emailTextView = FindViewById<EditText>(Resource.Id.editEmail);
            _phoneTextView = FindViewById<EditText>(Resource.Id.editPhone);

            _profilePhotoView = FindViewById<ImageView>(Resource.Id.user_profile_photo);
            _btnSaveProfile = FindViewById<Button>(Resource.Id.btnSaveProfile);

            //Hide the fields for the dealer data
            _companyNameTextView.Visibility = ViewStates.Gone;
            _addressTextView.Visibility = ViewStates.Gone;

            //tap action for saving and changin profile photo
            _profilePhotoView.Click += ChangeProfilePhotoView;
            _btnSaveProfile.Click += BtnSaveUserProfile;
        }

        #region Click actions
        private async void BtnSaveUserProfile(object sender, EventArgs e)
        {
            //save the data now
            AndHUD.Shared.Show(this, "Updating profile", -1, MaskType.Black);
            try
            {
                //if dealer save dealer related stuff

                _dataManager.userProfile.firstName = _firstNameTextView.Text.Trim();
                _dataManager.userProfile.lastName = _lastNameTextView.Text.Trim();
                //dataManager.userProfile.username = usernameTextView.Text.Trim();
                _dataManager.userProfile.country = _countryEditText.Text.Trim();
                _dataManager.userProfile.city = _cityTextView.Text.Trim();
                _dataManager.userProfile.email = _emailTextView.Text.Trim();
                _dataManager.userProfile.phone = _phoneTextView.Text.Trim();


                if (_selectedProfileImage != null)
                {

                    _dataManager.userProfile.profilePicture = _selectedProfileImage;
                }

                await _dataManager.updateUserProfile(_dataManager.userProfile);

                Toast.MakeText(this, "Profile updated successfully", ToastLength.Short).Show();
                Finish(); //end the activity
                AndHUD.Shared.Dismiss();
            }
            catch (Exception ex)
            {
                AndHUD.Shared.Dismiss();
                Console.WriteLine(string.Format("Error saving data {0} {1}", ex.Message, ex.StackTrace));
                Toast.MakeText(this, "Error updating profile", ToastLength.Short).Show();
            }
        }

        private void ChangeProfilePhotoView(object sender, EventArgs e)
        {
            //open the image picker for selecting the profile images
            var imageIntent = new Intent(Intent.ActionPick);

            imageIntent.SetType("image/*");
            imageIntent.PutExtra(Intent.ExtraAllowMultiple, false); //no multi selection
            //imageIntent.SetAction(Intent.ActionCameraButton);

            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(imageIntent, "Select profile picture"), _pickImageId);
        }

        //handles the back arrow on the action bar
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                Finish();

            return base.OnOptionsItemSelected(item);
        }
        #endregion


        #region Image processing
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                if ((requestCode == _pickImageId) && (resultCode == Result.Ok) && (data != null))
                {
                    var uri = data.Data;
                    var path = HelperClass.GetPathToImage(uri);
                    //covert to bitmap
                    var tempBmp =  HelperClass.GetImageBitmapFromUrl(path);

                    _selectedProfileImage = HelperClass.ResizeImage(tempBmp, 60); //resize image by 60%
                    _profilePhotoView.SetImageBitmap(_selectedProfileImage); //set in the preview section
                    //clear from memory
                    tempBmp.Recycle();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error setting image {0} {1}", ex.Message, ex.StackTrace));
            }
        }
        #endregion
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
                    _usernameTextView.Text = username;
                    //get the profile picture
                    var profilePicUrl = await _dataManager.FetchUserProfileImage(username);
                    if (profilePicUrl != null)
                    {
                        //use cache image
                        _imageLoader.DisplayImage(profilePicUrl.AbsoluteUri, _profilePhotoView);
                    }

                    if (ownerData.ContainsKey("usertype"))
                    {
                        accountType = ownerData["usertype"].ToString();
                    }

                    //if dealer get dealer profile
                    if (string.Equals(accountType, "Dealer"))
                    {
                        _companyNameTextView.Visibility = ViewStates.Visible;
                        _addressTextView.Visibility = ViewStates.Visible;

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
                        _addressTextView.Visibility = ViewStates.Gone;
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
                        _countryEditText.Text = country;
                        _countryTextView.Text = country;
                    }

                    if (ownerData.ContainsKey("email"))
                    {
                        _emailTextView.Text = ownerData["email"].ToString();
                    }

                    if (ownerData.ContainsKey("phone"))
                    {
                        _phoneTextView.Text = ownerData["phone"].ToString();
                    }


                    if (ownerData.ContainsKey("state"))
                    {
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
                Console.WriteLine(string.Format("Error happened please check asap {0} {1}", ex.Message, ex.StackTrace));
            }

        }
    }
}