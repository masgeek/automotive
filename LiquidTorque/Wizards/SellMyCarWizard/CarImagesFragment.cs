using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;
using Uri = Android.Net.Uri;

namespace liquidtorque.Wizards.SellMyCarWizard
{
    public class CarImagesFragment : WizardStep
    {
        [WizardState] public Car CarProfile;

        private View _view;
        private DataManager _dataManager;
        private ImageView _selectedImage;

        private Button _btnImagePicker;
        public static readonly int PickImageId = 1000;
        private List<string> _paths;

        private Dialog _confirmationDialog;
        private bool _carImageSelected;
        public CarImagesFragment()
        {
            StepExited += OnStepExited;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {


                // Use this to return your custom view for this Fragment
                _dataManager = DataManager.GetInstance();
                if (_paths == null)
                {
                    _paths = new List<string>(); //this will hold the image paths
                }
                if (_view == null)
                {
                    _view = inflater.Inflate(Resource.Layout.image_picker, container, false);

                    _btnImagePicker = _view.FindViewById<Button>(Resource.Id.btnImagePicker);
                    _selectedImage = _view.FindViewById<ImageView>(Resource.Id.imagePreview);

                    _btnImagePicker.Click += BtnImagePickerClick;


                    //set alert for executing the task
                    AlertDialog.Builder alert = new AlertDialog.Builder(Context);
                    alert.SetTitle("Confirm Saving");
                    alert.SetMessage("Are you sure you want to save your changes?");
                    alert.SetPositiveButton("Yes", (senderAlert, args) =>
                    {
                        Toast.MakeText(Context, "Presh finsh to upload images!", ToastLength.Short).Show();
                        OnStepExited(StepExitCode.ExitNext);

                    });
                    alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                    {
                        Toast.MakeText(Context, "Cancelled!", ToastLength.Short).Show();
                        OnStepExited(StepExitCode.ExitPrevious);

                    });

                    _confirmationDialog = alert.Create();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("An error occured while setting car images view {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent("An Error occured" + ex.Message + ex.StackTrace);
            }
            return _view;
        }

        private void BtnImagePickerClick(object sender, EventArgs e)
        {
            var imageIntent = new Intent(Intent.ActionPick);

            imageIntent.SetType("image/*");
            imageIntent.PutExtra(Intent.ExtraAllowMultiple, true);

            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(imageIntent, "Select images (upto 25)"), PickImageId);
        }

        
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            try
            {
                if (_paths != null)
                {
                    _paths.Clear(); /*remove initial image*/
                }

                //result code is -1
                    if (resultCode == (int) Result.Ok)
                    {

                        var clipData = data.ClipData;
                        Uri uri;
                        string path;
                        if (clipData != null) //handlem multi selection
                        {
                            //Console.WriteLine(string.Format("Clip data count {0}", clipData.ItemCount));
                            //var imagePath = HelperClass.GetPathToImage(pickedImageUrl);
                            for (int i = 0; i < clipData.ItemCount; i++)
                            {

                                ClipData.Item item = clipData.GetItemAt(i);

                                uri = item.Uri; //get the image Uris

                                //In case you need image's absolute path
                                path = HelperClass.GetPathToImage(uri);
                                if (path != null)
                                {
                                    if (_paths != null) _paths.Add(path); //add the image paths
                                }
                            }
                            Toast.MakeText(Application.Context, string.Format("{0} images selected", clipData.ItemCount), ToastLength.Short).Show();
                            _carImageSelected = true;
                            //confirmationDialog.Show();
                        }
                        else
                        {
                            uri = data.Data;
                            path = HelperClass.GetPathToImage(uri);
                            if (path != null)
                            {
                                if (_paths != null) _paths.Add(path); //add the image paths
                                _carImageSelected = true;
                            }
                            //@TODO cater for single image selection?
                            Toast.MakeText(Application.Context, "You have selected only one image, you can add upto 25", ToastLength.Short).Show();
                            //confirmationDialog.Show();
                           
                        }

                        //show images previews here
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Image selection error {0} {1}", ex.Message, ex.StackTrace));
                Toast.MakeText(Application.Context,
                    string.Format("Unable to select image please try again {0}", ex.Message),
                    ToastLength.Long).Show();
                _carImageSelected = false;

                MetricsManager.TrackEvent("Image selection error " + ex.Message + ex.StackTrace);
            }

            //verify validation flag
            if (_carImageSelected)
            {
                NotifyCompleted(); // All the input is valid.. Set the step as completed
            }
            else
            {
                NotifyIncomplete(); //Input is invalid do not proceed to next step
            }
        }

        private void OnStepExited(StepExitCode exitCode)
        {
            if (exitCode == StepExitCode.ExitPrevious) { return;}

            try
            {
                if (CarProfile == null)
                {
                    CarProfile = new Car();
                }

                //let us save the vehicle information here
                CarProfile.ownerUsername = HelperClass.CurrentUser; //currently logged in user...duh :)
                //now lets get the bitmap and add them to the list
                foreach (var path in _paths)
                {
                    //get the bitmap image
                    //build image views here for preview purposes
                    //lets add it to our collection
                    CarProfile.SelectedImagePaths.Add(path);
                    //carsList.Add(bitmapImage);
                    //we can also list in ain alist view for user preview
                    //selectedImage.SetImageBitmap(bitmapImage);
                    Console.WriteLine(string.Format(string.Format("Image path is {0}", path)));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("An error occured {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent("An Error occured" + ex.Message + ex.StackTrace);
            }
        }
    }
}