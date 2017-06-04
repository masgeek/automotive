using Android.App;
using Android.OS;

namespace liquidtorque.Activities
{
    [Activity(Label = "Register Activity")]
    public class RegisterActivity : Android.Support.V4.App.FragmentActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.user_signup_wizard);
        }
    }
}