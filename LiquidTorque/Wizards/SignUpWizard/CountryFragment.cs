using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using HockeyApp;
using liquidtorque.DataAccessLayer;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;

namespace liquidtorque.Wizards.SignUpWizard
{
    public class CountryFragment : WizardStep
    {
        [WizardState]
        public UserProfile UserProfile;

        private View _view;
        private ToggleButton _toggleUs;
        private ToggleButton _toggleUk;
        private ImageView _countryFlagImageView;
        private string _selectedCountry;

        public CountryFragment()
        {
            StepExited += OnStepExited;
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            try
            {
                if (_view == null)
                {
                    _view = inflater.Inflate(Resource.Layout.country_selection, container, false);

                    _toggleUs = _view.FindViewById<ToggleButton>(Resource.Id.toggleUS);
                    _toggleUk = _view.FindViewById<ToggleButton>(Resource.Id.toggleUK);
                    _countryFlagImageView = _view.FindViewById<ImageView>(Resource.Id.countryFlag);

                    _toggleUs.Click += ToggleUsClicked;
                    _toggleUk.Click += ToggleUkClicked;
                }
            }
            catch (Exception ex)
            {
                var messsage = string.Format("View error {0} stack {1}", ex.Message, ex.StackTrace);
                MetricsManager.TrackEvent(messsage);
            }

            return _view;
        }

        private void ToggleUkClicked(object sender, EventArgs e)
        {
            const int ukFlag = Resource.Drawable.uk_flag;
            if (_toggleUs.Checked)
            {
                _toggleUs.Checked = false;
            }
            _selectedCountry = "UK";
            ChangeFlag(ukFlag);
            Validate();
        }

        private void ToggleUsClicked(object sender, EventArgs e)
        {
            const int usFlag = Resource.Drawable.us_flag;
            if (_toggleUk.Checked)
            {
                _toggleUk.Checked = false;
            }
            _selectedCountry = "USA";
            ChangeFlag(usFlag);
            Validate();
        }

        private void OnStepExited(StepExitCode exitCode)
        {
            if (exitCode == StepExitCode.ExitPrevious) return;

            if (UserProfile == null)
            {
                UserProfile = new UserProfile();
            }

            UserProfile.country = _selectedCountry;
        }

        private void Validate()
        {
            bool valid = true;

            if (_toggleUs.Checked)
            {
                if (string.IsNullOrWhiteSpace(_selectedCountry))
                {
                    valid = false;
                }
            }
            else if (_toggleUk.Checked)
            {
                if (string.IsNullOrWhiteSpace(_selectedCountry))
                {
                    valid = false;
                }
            }
            else
            {
                valid = false;
            }
            if (valid)
                NotifyCompleted(); // All the input is valid.. Set the step as completed
            else
                NotifyIncomplete();
        }

        private void ChangeFlag(int resid)
        {
            _countryFlagImageView.SetImageResource(resid);
        }
    }
}