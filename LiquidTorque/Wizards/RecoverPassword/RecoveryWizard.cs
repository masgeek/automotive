using System;
using Android.App;
using HockeyApp.Android.Metrics;
using WizarDroid.NET;
using WizarDroid.NET.Infrastructure.Layouts;

namespace liquidtorque.Wizards.RecoverPassword
{
    [Activity(Label = "Recover your account password")]
    class RecoveryWizard : BasicWizardLayout
    {
        private WizardFlow _wizard;
        public override WizardFlow OnSetup()
        {
            try { 
           _wizard =  new WizardFlow.Builder()
                .AddStep(new ForgotPasswordFragment(), true /*isRequired*/)
                .AddStep(new RecoveryConfirmation(), false /*isRequired*/)
                .Create();
            }
            catch (Exception ex)
            {
                var message = "Error initalizaing password recovery wizard" + ex.Message;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message + ex.StackTrace);
            }

            return _wizard;
            ;
        }

        public RecoveryWizard()
        {
            WizardCompleted += OnWizardComplete;
        }
        void OnWizardComplete()
        {//end the wizard and go back to login page
            Activity.Finish();
        }
    }
}