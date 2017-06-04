using Android.OS;
using Android.Views;
using Fragment = Android.Support.V4.App.Fragment;

namespace liquidtorque.Fragments
{
    public class TestLayoutFragment : Fragment
    {

        public static TestLayoutFragment NewInstance()
        {
            var testLayoutFragment = new TestLayoutFragment { Arguments = new Bundle() };
            return testLayoutFragment;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            var view = inflater.Inflate(Resource.Layout.user_profile_view, container, false);

            return view;
        }
    }
}