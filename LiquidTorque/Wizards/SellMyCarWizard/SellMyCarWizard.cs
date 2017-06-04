using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using WizarDroid.NET;
using WizarDroid.NET.Infrastructure.Layouts;
using WizarDroid.NET.Persistence;
using Exception = Java.Lang.Exception;

namespace liquidtorque.Wizards.SellMyCarWizard
{
    class SellMyCarWizard: BasicWizardLayout
    {
        [WizardState] public Car CarProfile;


        private DataManager _dataManager;
        private List<Bitmap> _vehicleImages;
        public SellMyCarWizard()
        {
            WizardCompleted += OnWizardComplete;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _dataManager = DataManager.GetInstance();
        }

        public override WizardFlow OnSetup()
        {
            return new WizardFlow.Builder()
                .AddStep(new CarInfoFragment(), true /*isRequired*/)
                .AddStep(new CarImagesFragment(), true /*isRequired*/)
                .Create();
        }

        async void OnWizardComplete()
        {
            //lets save the vehicle data
            string message = "Unable to save vehicle details, please try again";
            try
            {
                var vehicleObjectId = await _dataManager.insertNewCar(CarProfile);
                if (!string.IsNullOrEmpty(vehicleObjectId))
                {
                    //next insert the images
                    //let get the bitmaps now
                    _vehicleImages = new List<Bitmap>();
                    var paths = CarProfile.SelectedImagePaths;
                    foreach (var path in paths)
                    {
                        var bitmapImage = HelperClass.GetImageBitmapFromUrl(path);
                        _vehicleImages.Add(bitmapImage);
                    }

                    _dataManager.SaveParseImage(_vehicleImages, CarProfile.ownerUsername, CarProfile.make, CarProfile.model, CarProfile.year, vehicleObjectId);
                    Android.Util.Log.Info("Vehicle wizard", "Saving vehidle information");
                    message = "Vehicle listed successfully";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("An error occured {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent(string.Format("Error {0} {1}", ex.Message, ex.StackTrace));
            }

            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, Android.Widget.ToastLength.Long).Show();

            //should consider redirecting to inventory activity
            Activity.Finish();
        }
    }
}