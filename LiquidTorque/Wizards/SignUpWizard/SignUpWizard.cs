using System;
using Android.App;
using HockeyApp;
using liquidtorque.DataAccessLayer;
using Parse;
using WizarDroid.NET;
using WizarDroid.NET.Infrastructure.Layouts;
using WizarDroid.NET.Persistence;

namespace liquidtorque.Wizards.SignUpWizard
{

    [Activity(Label = "User signup form wizard")]
    public class SignUpWizard : BasicWizardLayout
    {
        [WizardState] public UserProfile UserProfile;
        [WizardState] public DealerProfile DealerProfile;
        readonly DataManager _dataManager = DataManager.GetInstance();
        private bool _alreadyRun;
        string _message = "Signup not successfull, please try again";

        public SignUpWizard()
        {
            WizardCompleted += OnWizardComplete;
        }



        public override WizardFlow OnSetup()
        {
            return new WizardFlow.Builder()
                .AddStep(new AccountTypeFragment(), true /*isRequired*/)
                .AddStep(new CountryFragment(), true /*isRequired*/)
                .AddStep(new UserProfileFragment(), true /*isRequired*/)
                .AddStep(new TermsOfService(), true /*isRequired*/)
                .AddStep(new EmailCodeFragment(), true /*isRequired*/)
                .AddStep(new SmsCodeFragment(), true /*isRequired*/)
                .Create();
        }

        async void OnWizardComplete()
        {

            ParseUser.LogOut(); //clear any sessions
            try
            {
                string jsonObj = null;
                string selectedAccountType = null;
                bool successFull = false;
                //sequential checking
                if (DealerProfile != null && DealerProfile.userProfile != null &&
                    DealerProfile.userProfile.userType != null)
                {
                    selectedAccountType = DealerProfile.userProfile.userType;
                }
                else if (UserProfile != null && UserProfile.userType != null)
                {
                    selectedAccountType = UserProfile.userType;
                }

                if (!_alreadyRun)
                {
                    _alreadyRun = true;
                    //now we can post to parse and tell the user that the sign up is successfull
                    if (selectedAccountType == "Dealer")
                    {
                        if (DealerProfile != null)
                        {
                            var licensePath = DealerProfile.dealerLicense;
                            //save the dealer
                            successFull = await _dataManager.signUpDealer(DealerProfile, licensePath);
                            jsonObj = Newtonsoft.Json.JsonConvert.SerializeObject(DealerProfile);
                        }
                    }
                    else if (selectedAccountType == "Private Party")
                    {
                        //save the regular user
                        successFull = await _dataManager.signUpPrivateParty(UserProfile);
                        jsonObj = Newtonsoft.Json.JsonConvert.SerializeObject(UserProfile);
                    }

                    if (successFull)
                    {
                        _message = "Signup Successfull";
                    }
                }

                Android.Util.Log.Info("CustomerWizard", jsonObj);
                Android.Widget.Toast.MakeText(Application.Context, _message, Android.Widget.ToastLength.Long)
                    .Show();
                Activity.Finish();
            }
            catch (ParseException ex)
            {
                var message = "Error saving to parse " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
            catch (Exception ex)
            {
                var message = "Error saving to profile data " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
        }
    }
}