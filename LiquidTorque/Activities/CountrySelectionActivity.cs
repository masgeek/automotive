using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace liquidtorque.Activities
{
    [Activity(Label = "Select Country")]
    public class CountrySelectionActivity : BaseActivity
    {
        protected override int LayoutResource
        {
            get { return Resource.Layout.country_selection; }
        }

        private string _selectedCountry;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //---Create your application here---\\
            
            /*
            
            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner);

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
                
            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.account_array, Android.Resource.Layout.SimpleSpinnerItem);
            
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            
            spinner.Adapter = adapter;
            
            */

            //var btnUs = FindViewById<ImageButton>(Resource.Id.usButton);
            //var btnUk = FindViewById<ImageButton>(Resource.Id.ukButton);

            //btnUs.Click += BtnUsClicked;
           // btnUk.Click += BtnUkClicked;
        }

        private void BtnUsClicked(object sender, EventArgs e)
        {
            _selectedCountry = "USA";
            LaunchSignUpActivity(_selectedCountry);
        }

        private void BtnUkClicked(object sender, EventArgs e)
        {
            _selectedCountry = "UK";
            LaunchSignUpActivity(_selectedCountry);
        }


        private void LaunchSignUpActivity(string country)
        {
            try
            {
                //first lets get the profile type set in the previous activity
                Intent intent = null;
                var accountType = Intent.GetStringExtra("AccountType") ?? "NA";

                Android.Widget.Toast.MakeText(this, string.Format("Registering {0} account", accountType), Android.Widget.ToastLength.Short).Show();
                switch (accountType)
                {
                    case "Private Party":
                        //launch the private party signup
                         //intent = new Intent(this, typeof(PrivatePartySignUpActivity));
                        //StartActivity(intent);
                        break;
                    case "Dealer":
                        //launch dealer signup
                         //intent = new Intent(this, typeof(DealerSignUpActivity));
                        break;;
                        
                }
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (intent != null)
                {
                    StartActivity(intent);
                }
                else
                {
                    Toast.MakeText(this, "Invalid account type",ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting account type switch "+ex.Message+ex.StackTrace);
            }

        }
        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            string toast = string.Format("Selected account type is {0}", spinner.GetItemAtPosition(e.Position));
            Toast.MakeText(this, toast, ToastLength.Long).Show();
        }
    }
}