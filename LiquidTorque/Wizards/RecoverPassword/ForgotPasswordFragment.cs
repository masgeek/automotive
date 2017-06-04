using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using HockeyApp.Android.Metrics;
using liquidtorque.ComponentClasses;
using Parse;
using WizarDroid.NET;

namespace liquidtorque.Wizards.RecoverPassword
{
    public class ForgotPasswordFragment : WizardStep
    {
        private EditText _txtRecoveryEmail;
        private TextView _instructionText;
        private string _recoveryEmail;
        bool _isDataValid;

        public ForgotPasswordFragment()
        {
            StepExited += OnStepExited;
            ;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.forgot_password, container, false);

            _instructionText = view.FindViewById<TextView>(Resource.Id.instructionText);
            _txtRecoveryEmail = view.FindViewById<EditText>(Resource.Id.recoveryEmailAddress);

            _txtRecoveryEmail.AfterTextChanged += delegate { ValidateEntries(); }; //validate the typed email

            SetTypeFaces();
            return view;
        }

        private async void OnStepExited(StepExitCode exitCode)
        {
            var message = "Recovery email not sent successfully";
            AndHUD.Shared.Show(Context, string.Format("Authenticating email {0}", _recoveryEmail), -1, MaskType.Black);
            try
            {
                if (exitCode == StepExitCode.ExitPrevious) return;

                message = "Recovery email sent successfully";
                //let us send the recovery email

                await ParseUser.RequestPasswordResetAsync(_recoveryEmail);
            }
            catch (ParseException ex)
            {
                message = "Error sending password reset " + ex.Message;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                message = "Error sending password reset " + ex.Message;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message + ex.StackTrace);
            }
            finally
            {
                AndHUD.Shared.Dismiss(Context);
            }

            Toast.MakeText(Context, message, Android.Widget.ToastLength.Long).Show();
        }

        public void ValidateEntries()
        {
            try
            {
                _isDataValid = true;
                _recoveryEmail = _txtRecoveryEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(_recoveryEmail) || _recoveryEmail.Length < 3)
                {
                    _txtRecoveryEmail.Error = "Invalid email address";
                    _isDataValid = false;
                }
                else
                {
                    _txtRecoveryEmail.Error = null;
                    _isDataValid = true;
                }

                if (_isDataValid)
                {
                    NotifyCompleted(); // All the input is valid.. Set the step as completed
                }
                else
                {
                    NotifyIncomplete();
                }
            }
            catch (Exception ex)
            {
                var message = "Error " + ex.Message;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message + ex.StackTrace);
            }
        }

        //set typeface for the inuts
        private void SetTypeFaces()
        {
            try
            {
                _txtRecoveryEmail.Typeface = FontClass.LightTypeface;
                _instructionText.Typeface = FontClass.BoldTypeface;
            }
            catch (Exception ex)
            {
                var message = "Error " + ex.Message;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message + ex.StackTrace);
            }
        }
    }
}