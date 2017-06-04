using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;
using UniversalImageLoader.Core.Listener;

namespace liquidtorque.Activities
{
    //[Activity(Label = "Liquid Torque", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/lt_icon", WindowSoftInputMode = SoftInput.AdjustResize)]
    [Activity(Label = "Test activity")]
    public class TestActivity : BaseActivity
    {
        protected override int LayoutResource
        {
            get { return Resource.Layout.test_layout; }
        }

        readonly DataManager _dataManager = DataManager.GetInstance();

        private LinearLayout _linearLayout;
        private ImageView _mainImage;
        readonly ImageLoader _imageLoader = ImageLoader.Instance;
        private string _vehicleObjectId;
        private List<Uri> _images;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _vehicleObjectId = Intent.GetStringExtra("VehicleID") ?? "NA";
            _linearLayout = FindViewById<LinearLayout>(Resource.Id.thumbnails);
            _mainImage = FindViewById<ImageView>(Resource.Id.MainImageView);
            // Create your application here
        }

        protected override async void OnResume()
        {
            base.OnResume();
            var resp =await PopulateFields();
            int imageViewId = 0;
            //lets loop through the images list to see if we have any data
            if (resp) {
                if (_images != null)
                {
                    foreach (var image in _images)
                    {
                        var imageUrl = image.AbsoluteUri;
                       var itemIndex = _images.IndexOf(image);
                        //var bitmap = HelperClass.GetImageBitmapFromUrl(absoluteUri);
                        var imageView = new ImageView(this)
                        {
                            Id =itemIndex

                        };
                        //imageView.SetPadding(2, 2, 2, 2);
                        imageView.SetPadding(5, 5, 5, 5);
                        //imageView.SetImageBitmap(bitmap);
                        #region Imageloader
                        ImageSize targetSize = new ImageSize(640, 480);
                        _imageLoader.LoadImage(
                            imageUrl,
                            //targetSize,
                            HelperClass.ImageDownloaderOptions,
                            new ImageLoadingListener(
                                loadingStarted: delegate
                                {
  
                                },
                                loadingComplete: (imageUri, view, loadedImage) =>
                                {
                                    if (loadedImage != null)
                                    {
                                        //Bitmap loadedImage = e.LoadedImage;
                                        // Do whatever you want with Bitmap (80x50)
                                        imageView.SetImageBitmap(loadedImage);
                                        //Console.WriteLine($"Image dimensions width {loadedImage.Width} height {loadedImage.Height}");
                                    }
                                },
                                loadingFailed: (imageUri, view, failReason) =>
                                {
                                    string message = null;
                                    if (failReason.Type == FailReason.FailType.IoError)
                                    {
                                        message = "Input/Output error";
                                    }
                                    else if (failReason.Type == FailReason.FailType.DecodingError)
                                    {
                                        message = "Image can't be decoded";
                                    }
                                    else if (failReason.Type == FailReason.FailType.NetworkDenied)
                                    {
                                        message = "Downloads are denied";
                                    }
                                    else if (failReason.Type == FailReason.FailType.OutOfMemory)
                                    {
                                        message = "Out Of Memory error";
                                    }
                                    else
                                    {
                                        message = "Unknown error";
                                    }
                                    Toast.MakeText(view.Context, message, ToastLength.Short).Show();
                                }));
                        #endregion ImageLoader
                        //imageView.SetScaleType(ImageView.ScaleType.FitXy);
                        imageView.SetScaleType(ImageView.ScaleType.FitCenter);

                        //lets add it to the layout now
                        _linearLayout.AddView(imageView);
                        imageViewId++;

                        //add click evebts
                        var sender = imageViewId;
                        //imageView.Click += delegate { ImageView_Click(id); };
                        imageView.Click +=ImageViewOnClick;

                    }
                }
            }
        }

        private void ImageViewOnClick(object sender, EventArgs eventArgs)
        {
            //Button btn = sender as Button;
            var img = sender as ImageView;
            if (img != null)
            {
                var imageViewTagId = img.Id;
                //get teh url from the tag number
                var itemURl = _images[imageViewTagId].AbsoluteUri;

                //load this image to main image view
                //imageLoader.DisplayImage(imageUri, imageView);
                _imageLoader.DisplayImage(itemURl,_mainImage);
                Console.WriteLine(string.Format("Clik event for image view {0} image tag {1}", sender, imageViewTagId));
            }
        }

        private async Task<bool> PopulateFields()
        {
            if (!_vehicleObjectId.Equals("NA"))
            {
                //load the details

                await _dataManager.LoadSingleVehicleProfile(_vehicleObjectId);

                 _images = CarProfile.CarsForSale;

                var h = CarProfile.listPrice;

                return true;
            }
            return false;
        }
    }
}