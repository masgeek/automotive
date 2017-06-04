using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using liquidtorque.DataAccessLayer;
using Plugin.Vibrate;

namespace liquidtorque.Activities
{
    //    android:windowSoftInputMode="adjustResize"
     [Activity(Label = "User Login")]
    //[Activity(Label = "Liquid Torque", MainLauncher = false, LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/lt_icon", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class LoginActivity : BaseActivity
    {
        DataManager _dataManager = DataManager.GetInstance();
        protected override int LayoutResource
        {
            get { return Resource.Layout.user_login; }
        }

        private Button _btnLogin;
        private Button _btnRegister;
        private Button _btnForgotPassword;
        private EditText _username;
        private EditText _password;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            //SetContentView(Resource.Layout.Login);


            //initalize the login button
            _btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            _btnRegister = FindViewById<Button>(Resource.Id.btnRegister);
            _btnForgotPassword = FindViewById<Button>(Resource.Id.btnForgotPassword);
            _username = FindViewById<EditText>(Resource.Id.userName);
            _password = FindViewById<EditText>(Resource.Id.password);
            //login action click
            _btnLogin.Click += BtnLoginClicked;
            _btnRegister.Click += BtnRegisterClicked;
            _btnForgotPassword.Click += BtnForgotPasswordClicked;
        }

        private async void BtnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                //let us login the user
                if (string.IsNullOrWhiteSpace(_username.Text) || _username.Text.Length < 3)
                {
                    _username.Error = "Please enter a valid username";
                    Toast.MakeText(this, "Type a valid username", Android.Widget.ToastLength.Short).Show();
                }
                else if (string.IsNullOrWhiteSpace(_password.Text) || _password.Text.Length < 3)
                {
                    _password.Error = "Please enter a valid password";
                    Toast.MakeText(this, "Password cannot be empty", Android.Widget.ToastLength.Short).Show();

                }
                else
                {
                    string username = _username.Text.ToLower(); //make it lower case at all times
                    string password = _password.Text;

                    //show the HUD here
                    AndHUD.Shared.Show(this, "Verifying user...", -1, MaskType.Black);
                    var resp = await _dataManager.LoginUser(username, password);
                    if (resp)
                    {
                        /*Android.Telephony.TelephonyManager tMgr = (Android.Telephony.TelephonyManager)GetSystemService(Android.Content.Context.TelephonyService);
                    string imei = tMgr.DeviceId;
                    string imsi = tMgr.SubscriberId;*/

                        //dismiss the hud after getting a response
                        AndHUD.Shared.Dismiss(this);
                        //AndHUD.Shared.ShowSuccess(this, "Success", MaskType.Black, TimeSpan.FromSeconds(5));
                        CrossVibrate.Current.Vibration(250);
                        //clear the text
                        ClearValues();
                        //launch the main windows activity if sign in is successfull
                        var intent = new Intent(this, typeof(MainActivity));
                        Toast.MakeText(this, string.Format("Login successfull welcome [{0}]", username.ToUpper()),
                            ToastLength.Short).Show();
                        StartActivity(intent);
                    }
                    else
                    {
                        //dismiss the hud after getting a response
                        AndHUD.Shared.Dismiss(this);
                        Toast.MakeText(this, "Unable to log you in, please check you username and password",
                            ToastLength.Short).Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message+ex.StackTrace);
            }
        }


        private void BtnRegisterClicked(object sender, EventArgs e)
        {
            //open the registration activity
            Toast.MakeText(this, "Sign up for a liquidtorque account", ToastLength.Short).Show();
            //launch the main windows activity
            var intent = new Intent(this, typeof(RegisterActivity));
            StartActivity(intent);
        }
        private void BtnForgotPasswordClicked(object sender, EventArgs e)
        {
            //open the registration activity
            Toast.MakeText(this, "Password recovery", ToastLength.Short).Show();
            //launch the main windows activity
            var intent = new Intent(this, typeof(RecoveryActivity));
            StartActivity(intent);
        }
        private void ClearValues()
        {
            _username.Text = null;
            _password.Text = null;
        }
    }
}