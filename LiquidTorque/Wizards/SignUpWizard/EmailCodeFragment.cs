using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using HockeyApp.Android.Metrics;
using liquidtorque.DataAccessLayer;
using MessagingLib;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;

namespace liquidtorque.Wizards.SignUpWizard
{
    public class EmailCodeFragment : WizardStep
    {
        [WizardState]
        public UserProfile UserProfile;
        [WizardState]
        public DealerProfile DealerProfile;

        Messaging _messaging = Messaging.GetInstance();

        private string _enteredEmailCode;
        private string _sentEmailCode;
        private string _userEmailAddress;
        private Boolean _codeSent;
        //layout items
        private View view;
        private EditText _invitationCodeEditText;
        private TextView _instructionTextView;
        private Button _regenerateCode;

        public EmailCodeFragment()
        {
            StepExited += OnStepExited;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            if (view == null)
            {
                view = inflater.Inflate(Resource.Layout.invitation_codes, container, false);
                _invitationCodeEditText = view.FindViewById<EditText>(Resource.Id.txtInvitationCode);
                _instructionTextView = view.FindViewById<TextView>(Resource.Id.instructionText);
                _regenerateCode = view.FindViewById<Button>(Resource.Id.btnRegenerate);

                _instructionTextView.Text = "Enter the code recieved via Email";

                _invitationCodeEditText.AfterTextChanged += delegate { ValidateCodes(); };
                _regenerateCode.Click += RegenerateInvitationCode;
            }
            return view;
        }

        private void ValidateCodes()
        {
            bool isCodeValid;
            _enteredEmailCode = _invitationCodeEditText.Text.Trim();

            if (string.IsNullOrWhiteSpace(_enteredEmailCode) || !string.Equals(_enteredEmailCode.ToUpper(), _sentEmailCode))
            {
                _invitationCodeEditText.Error = "The email codes do not match";
                isCodeValid = false;
            }
            else
            {//the codes match
                _invitationCodeEditText.Error = null;
                isCodeValid = true;
            }


            if (isCodeValid)
            {
                NotifyCompleted(); // All the input is valid.. Set the step as completed
            }
            else
            {
                NotifyIncomplete();
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
        }

        public override void OnStart()
        {
            base.OnStart();
            try
            {
                //get the user phone number form the passed session
                if (UserProfile != null && UserProfile.phone != null)
                {
                    _userEmailAddress = UserProfile.email;
                    //let us get the codes form here
                    GenerateCode();
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Unable to send email invitation code {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
        }

        private void RegenerateInvitationCode(object sender, EventArgs e)
        {
            //resend the new code
            _codeSent = false;
            _enteredEmailCode = null;
            _invitationCodeEditText.Text = null;
            GenerateCode();
        }

        /// <summary>
        /// Generate the verification code
        /// </summary>
        private void GenerateCode()
        {
            if (!string.IsNullOrEmpty(_userEmailAddress) && _codeSent == false)
            {
                _sentEmailCode = VerificationCode.GenerateCode();
                var message = string.Format("Your invitation code is {0}", _sentEmailCode);
                //now we wait for user input
                Toast.MakeText(Context,
                    string.Format("Verification code sent via email to the address {0}", _userEmailAddress), ToastLength.Long)
                    .Show();
                //send the code via the sms channel
                _messaging.SendEmail(_userEmailAddress, "LiquidTorque Invitation code", message);
                _codeSent = true;
            }
        }

    }
}