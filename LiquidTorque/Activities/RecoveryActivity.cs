using Android.App;
using Android.OS;
using Android.Support.V4.App;

namespace liquidtorque.Activities
{
    [Activity(Label = "Account Recovery")]
    public class RecoveryActivity : FragmentActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.recovery_wizard);
        }
    }
}