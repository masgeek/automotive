using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using liquidtorque.DataAccessLayer;
using MessagingLib;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;

namespace liquidtorque.Wizards.SignUpWizard
{
    public class SmsCodeFragment : WizardStep
    {
        [WizardState]
        public UserProfile UserProfile;
        [WizardState]
        public DealerProfile DealerProfile;

        DataManager _dataManager = DataManager.GetInstance();
        readonly Messaging _messaging = Messaging.GetInstance();

        private string _enteredSmsCode;
        private string _sentSmsCode;
        private string _userPhoneNumber;
        private Boolean _codeSent;
        //layout items
        private View view;
        private EditText _invitationCodeEditText;
        private TextView _instructionTextView;
        private Button _regenerateCode;

        public SmsCodeFragment()
        {
            StepExited += OnStepExited;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override void OnResume()
        {
            base.OnResume();

            //let us get the codes form here
            GenerateCode();
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

                _instructionTextView.Text = "Enter the code recieved via SMS";

                _invitationCodeEditText.AfterTextChanged += delegate { ValidateCodes(); };
                _regenerateCode.Click += RegenerateInvitationCode;
            }
            return view;
        }

        private void ValidateCodes()
        {
            bool isCodeValid;
            _enteredSmsCode = _invitationCodeEditText.Text.Trim();

            if (string.IsNullOrWhiteSpace(_enteredSmsCode) || !string.Equals(_enteredSmsCode.ToUpper(),_sentSmsCode))
            {
                _invitationCodeEditText.Error = "The sms codes do not match";
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
            //get the user phone number form the passed session
            if (UserProfile.phone != null)
            {
                _userPhoneNumber = UserProfile.phone;
                //let us get the codes form here
                GenerateCode();
            }
        }

        private void RegenerateInvitationCode(object sender, EventArgs e)
        {
            //resend the new code
            _codeSent = false;
            _enteredSmsCode = null;
            _invitationCodeEditText.Text = null;
            GenerateCode();
        }

        /// <summary>
        /// Generate the verification code
        /// </summary>
        private void GenerateCode()
        {
            if (!string.IsNullOrEmpty(_userPhoneNumber)&&_codeSent==false)
            {
                _sentSmsCode = VerificationCode.GenerateCode();
                var message = string.Format("Your invitation code is {0}", _sentSmsCode);
                //now we wait for user input
                Toast.MakeText(Context,
                    string.Format("Verification code sent via sms to the number {0}", _userPhoneNumber), ToastLength.Long)
                    .Show();
                //send the code via the sms channel
                _messaging.SendSms(phoneNo: _userPhoneNumber, message: message);
                _codeSent = true;
            }
        }
            
    }
}