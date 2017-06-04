using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using Fragment = Android.Support.V4.App.Fragment;

namespace liquidtorque.Fragments
{
    public class UserSettingsFragment : Fragment
    {
        DataManager _dataManager = DataManager.GetInstance();
        private UserProfileData _userProfileData;

        private string _username;

        private EditText _facebook;
        private EditText _instagram;
        private EditText _twitter;
        private EditText _linkedin;
        private Button _btnUpdateSettings;
        private TextView _versionLabel;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            _userProfileData = UserProfileData.GetInstance();
        }

        public static UserSettingsFragment NewInstance()
        {
            var userSettingsFragment = new UserSettingsFragment() {Arguments = new Bundle()};
            return userSettingsFragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            AndHUD.Shared.Show(Context, "Loading...");
            // Use this to return your custom view for this Fragment
            //get username from activity data passed
            Bundle bundle = Arguments;

            //int myInt = bundle.getInt(key, defaultValue);
            if (bundle != null) _username = bundle.GetString("username");
            var view = inflater.Inflate(Resource.Layout.user_settings, container, false);
            _facebook = view.FindViewById<EditText>(Resource.Id.editFacebook);
            _instagram = view.FindViewById<EditText>(Resource.Id.editInstagram);
            _twitter = view.FindViewById<EditText>(Resource.Id.editTwitter);
            _linkedin = view.FindViewById<EditText>(Resource.Id.editLinkedin);
            _btnUpdateSettings = view.FindViewById<Button>(Resource.Id.btnUpdateSettings);
            _versionLabel = view.FindViewById<TextView>(Resource.Id.versionNumber);

            //set the version number
            _versionLabel.Text = string.Format("Version {0}", HelperClass.BuildNumber);
            //populate the other fields
            PopulateFields(_username);

            //click action
            _btnUpdateSettings.Click += BtnUpdateSettings;
            AndHUD.Shared.Dismiss(Context);
            return view;
        }

        private async void BtnUpdateSettings(object sender, EventArgs e)
        {
            AndHUD.Shared.Show(Context, "Saving...");
            try
            {
                if (_facebook.Text.Length > 0)
                {
                    _dataManager.setFacebookUserName(_facebook.Text.Trim());
                }

                if (_instagram.Text.Length > 0)
                {
                    _dataManager.setInstagramUserName(_instagram.Text.Trim());

                }

                if (_twitter.Text.Length > 0)
                {
                    _dataManager.setTwitterUserName(_twitter.Text.Trim());

                }
                if (_linkedin.Text.Length > 0)
                {
                    _dataManager.setLinkedInUserName(_linkedin.Text.Trim());

                }
                await _dataManager.updateUserProfile(_dataManager.userProfile, true);

                //show toast
                Toast.MakeText(Context, "Settings updated successfully", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                var message = string.Format("Error updating user profile {0} {1}", ex.Message,ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
                Toast.MakeText(Context, string.Format("Settings not updated successfully {0}", ex.Message), ToastLength.Short).Show();
            }
            AndHUD.Shared.Dismiss(Context);
        }

        async void PopulateFields(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username)) return; //return if null


                Dictionary<string, object> userdata = await _userProfileData.FetchUserProfileData(userName: username);
                if (userdata != null)
                {

                    if (userdata.ContainsKey("facebook"))
                    {
                        _facebook.Text = userdata["facebook"].ToString();
                    }


                    if (userdata.ContainsKey("instagram"))
                    {
                        _instagram.Text = userdata["instagram"].ToString();
                    }


                    if (userdata.ContainsKey("twitter"))
                    {
                        _twitter.Text = userdata["twitter"].ToString();
                    }

                    if (userdata.ContainsKey("linkedin"))
                    {
                        _linkedin.Text = userdata["linkedin"].ToString();
                    }
                }
            }
            catch(Exception ex)
            {
                var message = string.Format("Error occured setting profile info please check asap {0} {1}", ex.Message,
                    ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

        }
    }
}