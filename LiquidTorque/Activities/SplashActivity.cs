using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;
using Parse;

namespace liquidtorque.Activities
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : BaseActivity
    {
        static readonly string Tag = "X:" + typeof(SplashActivity).Name;
        readonly SqlLiteDataStore _datastore = SqlLiteDataStore.GetInstance();

        protected override int LayoutResource
        {
            get { return Resource.Layout.splash_layout; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            Log.Debug(Tag, "SplashActivity.OnCreate");
        }

        protected override void OnResume()
        {
            base.OnResume();

            Task startupWork = new Task(InitLocalStorage);

            startupWork.ContinueWith(t =>
            {
                //check if we already have a parse user session
                if (ParseUser.CurrentUser != null)
                {
                    // take the user directly to the main homepage
                    //StartActivity(new Intent(Application.Context, typeof(MainActivity)));
                    StartActivity(new Intent(this, typeof(MainActivity)));
                }
                else
                {
                    // show the signup or login screen
                    StartActivity(new Intent(Application.Context, typeof(LoginActivity)));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }


        public void InitLocalStorage()
        {
            //init the datmanger class
            DataManager dataManager = DataManager.GetInstance();
            _datastore.CheckIfDatabaseExists(); //check if the database exists and create it if it doesnt
        }
    }
}