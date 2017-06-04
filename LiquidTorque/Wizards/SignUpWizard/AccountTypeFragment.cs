using System;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;

namespace liquidtorque.Wizards.SignUpWizard
{
    public class AccountTypeFragment : WizardStep
    {
        [WizardState]
        public UserProfile UserProfile;

        private View _view;
        private ToggleButton _togglePrivate;
        private ToggleButton _toggleDealer;
        private string _accountType;
        public AccountTypeFragment()
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
            try
            {
                if (container == null)
                {
                    Log.Debug("SmartApp", "Dialer Fragment is in a view without container");
                    return null;
                }
                // Use this to return your custom view for this Fragment
                // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

                if (_view == null)
                {
                    _view = inflater.Inflate(Resource.Layout.account_type, container, false);


                    //btnPrivateParty = view.FindViewById<Button>(Resource.Id.btnPrivateParty);
                    // btnDealer = view.FindViewById<Button>(Resource.Id.btnDealer);
                    _togglePrivate = _view.FindViewById<ToggleButton>(Resource.Id.togglePrivate);
                    _toggleDealer = _view.FindViewById<ToggleButton>(Resource.Id.toggleDealer);
                    //accountType = "Private";
                    //btnPrivateParty.Click += delegate { Validate(); };
                    //btnDealer.Click += delegate { Validate(); };
                    //btnPrivateParty.Click += BtnPrivatePartyClick;
                    //btnDealer.Click += BtnDealerClick;

                    _togglePrivate.Click += TogglePrivateClicked;
                    _toggleDealer.Click += ToggleDealerClicked;
                    //txtAccountType.AfterTextChanged += delegate { Validate(); };
                    _togglePrivate.Typeface = FontClass.BoldTypeface;
                    _toggleDealer.Typeface = FontClass.BoldTypeface;
                }
                //return base.OnCreateView(inflater, container, savedInstanceState);
            }
            catch (Exception ex)
            {
                var messsage = string.Format("View error {0} stack {1}", ex.Message, ex.StackTrace);
                MetricsManager.TrackEvent(messsage);
            }

            return _view;
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            //do something when memory is low
        }

        private void ToggleDealerClicked(object sender, EventArgs e)
        {
            if (_togglePrivate.Checked)
            {
                _togglePrivate.Checked = false;
            }
            _accountType = "Dealer";
            Validate();
        }

        private void TogglePrivateClicked(object sender, EventArgs e)
        {
            if (_toggleDealer.Checked)
            {
                _toggleDealer.Checked = false;
            }
            _accountType = "Private Party";
            Validate();
        }

        private void OnStepExited(StepExitCode exitCode)
        {
            if (exitCode == StepExitCode.ExitPrevious) return;

            if (UserProfile == null)
            {
                UserProfile = new UserProfile();
            }

            UserProfile.userType = _accountType;
        }

        private void Validate()
        {
            bool valid = true;

            if (_togglePrivate.Checked)
            {
                if (string.IsNullOrWhiteSpace(_accountType) || _accountType.Length < 3)
                {
                    valid = false;
                }
            }
            else if (_toggleDealer.Checked)
            {
                if (string.IsNullOrWhiteSpace(_accountType) || _accountType.Length < 3)
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
    }
}