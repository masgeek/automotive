using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using HockeyApp.Android.Metrics;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using RestSharp.Extensions;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;
using String = System.String;
using Uri = Android.Net.Uri;

namespace liquidtorque.Wizards.SignUpWizard
{
    [Activity(Label = "User Registration", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class UserProfileFragment : WizardStep
    {
        [WizardState] public UserProfile UserProfile;
        [WizardState] public DealerProfile DealerProfile;

        GeoLocation _geoLocation = new GeoLocation();

        private const int TakePictureRequestCode = 42;
        private string _pictureFilePath;
        private Uri _licenseUri;
        //general profile information
        private EditText _firstName;
        private EditText _lastName;
        private EditText _emailAddress;
        private EditText _confirmEmailAddress;
        private EditText _phoneNumber;
        private EditText _zipCode;
        private EditText _country;
        private EditText _userName;
        private EditText _password;
        private EditText _confirmPassword;

        private Button _pickImage;
        //stuff for company profile
        private ImageView _licenseImage;
        private EditText _companyName;
        private EditText _dealerAddress;
        private EditText _dealerLicense; //should be a file picker
        private EditText _numberOfDevices;

        //global screens
        bool _isDataValid;
        private string _userAccountType;
        private string _userCountry;

        public DataManager DataManager;
        public UserProfileFragment()
        {
            StepExited += OnStepExited;
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            DataManager = DataManager.GetInstance();
        }

       
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            //check what type of account
            _userAccountType = UserProfile.userType;
            var view = inflater.Inflate(_userAccountType == "Dealer" ? Resource.Layout.dealer_signup : Resource.Layout.user_signup, container, false);

            _firstName = view.FindViewById<EditText>(Resource.Id.txtFirstName);
            _lastName = view.FindViewById<EditText>(Resource.Id.txtLastName);
            _emailAddress = view.FindViewById<EditText>(Resource.Id.txtEmail);
            _confirmEmailAddress = view.FindViewById<EditText>(Resource.Id.txtConfirmEmail);
            _phoneNumber = view.FindViewById<EditText>(Resource.Id.txtPhoneNumber);
            _zipCode = view.FindViewById<EditText>(Resource.Id.txtZipCode);
            _country = view.FindViewById<EditText>(Resource.Id.txtCountry);
            _userName = view.FindViewById<EditText>(Resource.Id.txtUsername);
            _password = view.FindViewById<EditText>(Resource.Id.txtPassword);
            _confirmPassword = view.FindViewById<EditText>(Resource.Id.txtConfirmPassword);


            //instanitate fields when the account type is dealer
            if (_userAccountType == "Dealer")
            {
                _companyName = view.FindViewById<EditText>(Resource.Id.txtCompanyName);
                _dealerAddress = view.FindViewById<EditText>(Resource.Id.txtAddress);
                _dealerLicense = view.FindViewById<EditText>(Resource.Id.txtLicense);
                _numberOfDevices = view.FindViewById<EditText>(Resource.Id.txtNoOfDevices);
                _licenseImage = view.FindViewById<ImageView>(Resource.Id.licenseImageView);
                _pickImage = view.FindViewById<Button>(Resource.Id.btnPickImage);

                //action delegates
                _companyName.AfterTextChanged += delegate { Validate(); };
                _dealerAddress.AfterTextChanged += delegate { Validate(); };
                _dealerLicense.AfterTextChanged += delegate { Validate(); };
                //numberOfDevices.AfterTextChanged += delegate { Validate(); };
                _dealerLicense.Enabled = false; //make it readonly

                _pickImage.Click += PickLicenseImageClicked;
            }

            _firstName.AfterTextChanged += delegate { Validate(); };
            _lastName.AfterTextChanged += delegate { Validate(); };

            //validate email address too
            //emailAddress.AfterTextChanged += delegate { Validate(); };
            _emailAddress.AfterTextChanged += ValidateEmailAddress;
            _confirmEmailAddress.AfterTextChanged += delegate { Validate(); };
            _phoneNumber.AfterTextChanged += delegate { Validate(); };
            _zipCode.AfterTextChanged += delegate { Validate(); };
            //country.AfterTextChanged += delegate { Validate(); };

            //validate username first
            //userName.AfterTextChanged += delegate { Validate(); };
            _userName.AfterTextChanged += ValidateUserName;
            _password.AfterTextChanged += delegate { Validate(); };
            _confirmPassword.AfterTextChanged += delegate { Validate(); };

            //set the typefaces
            SetTypeFaces();

            return view;
        }

        private void PickLicenseImageClicked(object sender, EventArgs e)
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            StartActivityForResult(intent, TakePictureRequestCode);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            try
            {
                if (requestCode != TakePictureRequestCode) return;

                _licenseUri = data.Data;
                _pictureFilePath = HelperClass.GetPathToImage(_licenseUri);
                _dealerLicense.Text = _pictureFilePath;

                var bmp = HelperClass.GetImageBitmapFromUrl(_pictureFilePath);
                //first resize this bitmap to a smaller size to prevent our  of memory error
                if (bmp != null)
                {
                    //licenseImage.SetImageURI(licenseUri);
                    _licenseImage.SetImageBitmap(bmp);
                }
            }
            catch (Java.Lang.OutOfMemoryError ex)
            {
                Console.WriteLine(string.Format("Memory error {0} {1}", ex.Message, ex.StackTrace));
                Toast.MakeText(Application.Context,
                    string.Format("Memory error {0}", ex.Message), ToastLength.Short).Show();

                MetricsManager.TrackEvent("Image selection error " + ex.Message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Image selection error {0} {1}", ex.Message, ex.StackTrace));
                Toast.MakeText(Application.Context,
                    string.Format("Unable to select image please try again {0}", ex.Message), ToastLength.Short).Show();

                MetricsManager.TrackEvent("Image selection error " + ex.Message + ex.StackTrace);
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            PopulateFields();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
        }

        private async void ValidateEmailAddress(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            _isDataValid = true;
            var userEmail = _emailAddress.Text;

            if (string.IsNullOrWhiteSpace(userEmail) || userEmail.Length < 3)
            {
                _emailAddress.Error = "Invalid email address";
                _isDataValid = false;
            }
            else
            {
                var emailExists = await DataManager.checkEmailAlreadyExists(userEmail);
                if (emailExists)
                {
                    _emailAddress.Error = String.Format(string.Format("The email address {0} is already used", userEmail));
                    _isDataValid = false;
                }
                else
                {
                    _emailAddress.Error = null;
                    _isDataValid = true;
                }

                Console.WriteLine(string.Format("Email address taken {0}", emailExists));
            }
        }

        private async void ValidateUserName(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            _isDataValid = true;
            var username = _userName.Text;
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                _userName.Error = "Invalid username, must be at least 3 characters";
                _isDataValid = false;
            }
            else
            {
                var userNameExists = await DataManager.checkUsernameAlreadyExists(username);
                if (userNameExists)
                {
                    _userName.Error = String.Format("The username {0} is taken", username);
                    _isDataValid = false;
                }
                else
                {
                    _userName.Error = null;
                    _isDataValid = true;
                }

                Console.WriteLine(String.Format("Username taken {0}", userNameExists));
            }
        }

        private void OnStepExited(StepExitCode exitCode)
        {
            if (exitCode == StepExitCode.ExitPrevious) return;

            if (UserProfile == null)
            {
                UserProfile = new UserProfile();
            }


            if (DealerProfile == null)
            {
                DealerProfile = new DealerProfile();
            }


            //add the dealer details
            if (_userAccountType == "Dealer")
            {
                DealerProfile.userProfile = UserProfile;
                DealerProfile.officerFirstName = _firstName.Text;
                DealerProfile.officerLastName = _lastName.Text;
                DealerProfile.companyName = _companyName.Text;
                DealerProfile.companyAddress = _dealerAddress.Text;
                DealerProfile.country = _userCountry;
                DealerProfile.zipCode = _zipCode.Text;
                //DealerProfile.dealerLicense = dealerLicense.Text;
                DealerProfile.dealerLicense = _pictureFilePath;
                DealerProfile.userName = _userName.Text;

                UserProfile.firstName = _firstName.Text;
                UserProfile.lastName = _lastName.Text;
                UserProfile.email = _emailAddress.Text;
                UserProfile.phone = _phoneNumber.Text;
                UserProfile.zipCode = _zipCode.Text;
                UserProfile.username = _userName.Text.ToLower(); //always lowercase this one
                UserProfile.password = _password.Text;

            }
            else
            {
                UserProfile.firstName = _firstName.Text;
                UserProfile.lastName = _lastName.Text;
                UserProfile.email = _emailAddress.Text;
                UserProfile.phone = _phoneNumber.Text;
                UserProfile.zipCode = _zipCode.Text;
                UserProfile.username = _userName.Text.ToLower(); //always lower case this one
                UserProfile.password = _password.Text;
            }
        }

        private void PopulateFields()
        {

            if (UserProfile.country != null)
            {
                _userCountry = UserProfile.country;
                _country.Text = _userCountry;
                _country.Enabled = false;
                /* Set placeholders for phone number */
                if (_userCountry != null)
                {
                    if (_userCountry.Equals("USA"))
                    {
                        _phoneNumber.Hint = "Phone e.g +1 832 123 4567";
                        _zipCode.Hint = "Zip Code";
                    }
                    else if (_userCountry.Equals("UK"))
                    {
                        _phoneNumber.Hint = "Phone e.g +44 20 1234 5678";
                        _zipCode.Hint = "Postal Code";
                    }
                }
            }

        }
        private void Validate()
        {

            _isDataValid = true;
            if (string.IsNullOrWhiteSpace(_userName.Text) || _userName.Text.Length < 3)
            {
                _userName.Error = "Invalid username, must be at least 3 characters";
                _isDataValid = false;
            }
            else
            {
                _userName.Error = null;
            }

            if (string.IsNullOrWhiteSpace(_emailAddress.Text) || _emailAddress.Text.Length < 3)
            {
                _emailAddress.Error = "Invalid email address";
                _isDataValid = false;
            }
            else
            {
                _emailAddress.Error = null;
            }
            //check if the emails match
            if (_confirmEmailAddress.Text.Matches(_emailAddress.Text))
            {
                _confirmEmailAddress.Error = null;
            }
            else
            {
                _confirmEmailAddress.Error = "Email addresses do not match";
                _isDataValid = false;
            }


            //validation for dealer profile
            if (_userAccountType == "Dealer")
            {
                if (string.IsNullOrWhiteSpace(_companyName.Text) || _companyName.Text.Length < 3)
                {
                    _companyName.Error = "Please enter company name";
                    _isDataValid = false;
                }
                else
                {
                    _companyName.Error = null;
                }
            }

            if (_userAccountType == "Dealer")
            {
                if (string.IsNullOrWhiteSpace(_companyName.Text) || _companyName.Text.Length < 3)
                {
                    _companyName.Error = "Please enter company name";
                    _isDataValid = false;
                }
                else
                {
                    _companyName.Error = null;
                }

                if (string.IsNullOrWhiteSpace(_dealerAddress.Text) || _dealerAddress.Text.Length < 3)
                {
                    _dealerAddress.Error = "Please enter address";
                    _isDataValid = false;
                }
                else
                {
                    _dealerAddress.Error = null;
                }

                if (string.IsNullOrWhiteSpace(_dealerLicense.Text) || _dealerLicense.Text.Length < 3)
                {
                    _dealerLicense.Error = "Please upload dealer license";
                    _isDataValid = false;
                }
                else
                {
                    _dealerLicense.Error = null;
                }

                /*if (string.IsNullOrWhiteSpace(numberOfDevices.Text) || numberOfDevices.Text.Length < 1)
                {
                    numberOfDevices.Error = "Please enter the number of devices to be used with this App";
                    _isDataValid = false;
                }
                else
                {
                    numberOfDevices.Error = null;
                }*/
            }
            //end dealer validation

            if (_isDataValid)
            {
                NotifyCompleted(); // All the input is valid.. Set the step as completed
            }
            else
            {
                NotifyIncomplete();
            }
        }


        //set typeface for the inuts
        private void SetTypeFaces()
        {
            _firstName.Typeface = FontClass.LightTypeface;
            _lastName.Typeface = FontClass.LightTypeface;
            _emailAddress.Typeface = FontClass.LightTypeface;
            _confirmEmailAddress.Typeface = FontClass.LightTypeface;
            _phoneNumber.Typeface = FontClass.LightTypeface;
            _zipCode.Typeface = FontClass.LightTypeface;
            _country.Typeface = FontClass.LightTypeface;
            _userName.Typeface = FontClass.LightTypeface;
            _password.Typeface = FontClass.LightTypeface;
            _confirmPassword.Typeface = FontClass.LightTypeface;

            if (_userAccountType == "Dealer")
            {
                _companyName.Typeface = FontClass.LightTypeface;
                _dealerAddress.Typeface = FontClass.LightTypeface;
                _dealerLicense.Typeface = FontClass.LightTypeface;
                _numberOfDevices.Typeface = FontClass.LightTypeface;
                _pickImage.Typeface = FontClass.BoldTypeface;
            }
        }
    }
}