using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using liquidtorque.Activities;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;

namespace liquidtorque.Wizards.SignUpWizard
{
    public class TermsOfService : WizardStep
    {
        [WizardState]
        public UserProfile UserProfile;

        private View _view;
        private CheckBox _acceptTermsCheckBox;
        private TextView _termsTextView;
        public TermsOfService()
        {
            StepExited += OnStepExited;
        }

        private void OnStepExited(StepExitCode exitCode)
        {
            if (exitCode == StepExitCode.ExitPrevious) return;

            if (UserProfile == null)
            {
                UserProfile = new UserProfile();
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            if (_view != null) return _view;

            _view = inflater.Inflate(Resource.Layout.terms_of_service, container, false);


            _termsTextView = _view.FindViewById<TextView>(Resource.Id.terms);
            _acceptTermsCheckBox = _view.FindViewById<CheckBox>(Resource.Id.chkAcceptTerms);

            _termsTextView.Clickable = true;

            return _view;

        }

        public override void OnStart()
        {
            base.OnStart();
            _acceptTermsCheckBox.CheckedChange += TermsAccepted;
            _acceptTermsCheckBox.Typeface = FontClass.BoldTypeface;

            _termsTextView.Click += TemrTextViewClicked;
        }

        private void TemrTextViewClicked(object sender, System.EventArgs e)
        {
            //open the terms text view
            var intent = new Intent(Context, typeof(TermsOfServiceActivity));
            StartActivity(intent);
        }

        private void TermsAccepted(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                NotifyCompleted();
            }
            else
            {
                NotifyIncomplete();
            }
        }
    }
}