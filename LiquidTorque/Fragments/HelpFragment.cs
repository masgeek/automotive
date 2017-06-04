using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Fragment = Android.Support.V4.App.Fragment;

namespace liquidtorque.Fragments
{
    public class HelpFragment : Fragment
    {
        private View view;
        private TextView helpEmail;
        private string emailAddress;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public static HelpFragment NewInstance()
        {
            var helpFragment = new HelpFragment() { Arguments = new Bundle() };
            return helpFragment;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            if (view == null)
            {
                view = inflater.Inflate(Resource.Layout.help_fragment, container, false);

                helpEmail = view.FindViewById<TextView>(Resource.Id.helpEmail);

                emailAddress = GetString(Resource.String.helpmail);

                helpEmail.Clickable = true;
                helpEmail.Click += HelpEmailClicked;
            }

            return view;
        }

        private void HelpEmailClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                Toast.MakeText(Context, "Email address not provided", ToastLength.Short).Show();
            }
            else
            {
                var uri = Android.Net.Uri.Parse("mailto:" + emailAddress);
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }
    }
}