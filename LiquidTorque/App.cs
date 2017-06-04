using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using HockeyApp.Android;
using HockeyApp.Android.Metrics;
using Parse;
using Plugin.Connectivity;
using UK.CO.Chrisjenx.Calligraphy;
using UniversalImageLoader.Cache.Disc.Naming;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;

namespace liquidtorque
{
    [Application]
    public class App : Application
    {
        private readonly string _hockeyAppId = "9bfa3bad3c9d45458cf1e8ac3122cc03 ";

        public App(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();

                //insantiate the hockey application insights
                InitHockeyInsights();
                // Initialize the Parse client with your Application ID and .NET Key
                //ParseClient.Initialize(AppId, NetKey);
                //log out any current user
                //initialize image loader
                InitImageLoader(ApplicationContext);

                //initialize font across the entire application
                CalligraphyConfig.InitDefault(new CalligraphyConfig.Builder()
                    .SetDefaultFontPath("fonts/bgothm.ttf")
                    //this will be the default typeface, similar to that of the IOS version
                    .SetFontAttrId(Resource.Attribute.fontPath)
                    .Build());

                //@TODO check connectivity
               var connected = CrossConnectivity.Current.IsConnected ? "Connected" : "No Connection";
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Error initalizing application {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent(String.Format("Error initalizing application {0} {1}", ex.Message,
                    ex.StackTrace));
            }
        }


        #region override methods


        #endregion

        #region External libraries for added value
        /// <summary>
        /// Initalize the hockey application crashes tracing
        /// </summary>
        public void InitHockeyInsights()
        {
            //instantiate the hockey application for crashes reporting
            CrashManager.Register(this, _hockeyAppId);
            MetricsManager.Register(this, this, _hockeyAppId);
            MetricsManager.EnableUserMetrics(); //metrics for user activities
        }
        public static void InitImageLoader(Context context)
        {
            // This configuration tuning is custom. 
            var config = new ImageLoaderConfiguration.Builder(context);
            config.ThreadPriority(Java.Lang.Thread.NormPriority - 2);
            //thread priority of the app, it is lower than normal to prevent lockup
            config.DenyCacheImageMultipleSizesInMemory();
            config.DiskCacheFileNameGenerator(new Md5FileNameGenerator());
            config.ThreadPoolSize(4); //number of threads to run default is 3
            config.DiskCacheSize(100*1024*1024); // 100 MiB maximum cache size
            config.TasksProcessingOrder(QueueProcessingType.Lifo); //Last in fast out...dont know how it works 

            config.WriteDebugLogs(); // Remove for release app

            // Initialize ImageLoader with configuration setting above.
            ImageLoader.Instance.Init(config.Build());
        }

        #endregion
    }
}