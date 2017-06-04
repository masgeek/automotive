using Android.App;
using Android.OS;

namespace liquidtorque.Activities
{
    [Activity(Label = "Sell MyCar")]
    public class SellMyCar : Android.Support.V4.App.FragmentActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.sell_my_car);
        }
    }
}