using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using com.refractored.fab;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;
using UniversalImageLoader.Core.Listener;

namespace liquidtorque.Activities
{
    [Activity(Label = "Owner Vehicle Details")]
    public class OwnerVehicleDetailsActivity : BaseActivity
    {
        readonly DataManager _dataManager = DataManager.GetInstance();

        private string _vehicleObjectId;
        private string _imageObjectId;
        private string _amountToPay = "699";

        readonly ImageLoader _imageLoader = ImageLoader.Instance;
        private List<Uri> _images;

        private LinearLayout _linearLayout;
        private ObservableScrollView _observableScrollView;
        private ImageView _mainImage;
        //labels
        private TextView _lblPrice;
        private TextView _lblDescription;
        private TextView _lblMake;
        private TextView _lblModel;
        private TextView _lblModelVar;
        private TextView _lblYear;
        private TextView _lblDriveTrain;
        private TextView _lblEngine;
        private TextView _lblTransmission;
        private TextView _lblMileage;
        private TextView _lblVin;
        private TextView _lblStockNo;
        private TextView _lblVat;
        private TextView _lblExtColor;
        private TextView _lblIntColor;
        private TextView _lblOwnerName;

        private ImageView _ownerImage;
        private Button _btnMessageOwner;
        protected override int LayoutResource
        {
            get { return Resource.Layout.vehicle_details; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            //first lets get the passed activity data
            _vehicleObjectId = Intent.GetStringExtra("VehicleID") ?? "NA";

            AndHUD.Shared.Show(this, "Loading vehicle details...", -1, MaskType.Black);

            _observableScrollView = FindViewById<ObservableScrollView>(Resource.Id.scrollableContents);
            _linearLayout = FindViewById<LinearLayout>(Resource.Id.thumbnails);
            _mainImage = FindViewById<ImageView>(Resource.Id.MainImageView);

            //initialize the labels
            _lblDescription = FindViewById<Button>(Resource.Id.txtDescription);
            _lblPrice = FindViewById<TextView>(Resource.Id.lblPrice);
            _lblDescription = FindViewById<TextView>(Resource.Id.lblDescription);
            _lblMake = FindViewById<TextView>(Resource.Id.txtMake);
            _lblModel = FindViewById<TextView>(Resource.Id.txtModel);
            _lblModelVar = FindViewById<TextView>(Resource.Id.txtModelVar);
            _lblYear = FindViewById<TextView>(Resource.Id.txtYear);
            _lblDriveTrain = FindViewById<TextView>(Resource.Id.txtDriveTrain);
            _lblEngine = FindViewById<TextView>(Resource.Id.txtEngine);
            _lblTransmission = FindViewById<TextView>(Resource.Id.txtTransmission);
            _lblMileage = FindViewById<TextView>(Resource.Id.txtMileage);
            _lblVin = FindViewById<TextView>(Resource.Id.txtVin);
            _lblStockNo = FindViewById<TextView>(Resource.Id.txtStockNo);
            _lblVat = FindViewById<TextView>(Resource.Id.txtVat);
            _lblExtColor = FindViewById<TextView>(Resource.Id.txtExtColor);
            _lblIntColor = FindViewById<TextView>(Resource.Id.txtIntColor);
            _lblOwnerName = FindViewById<TextView>(Resource.Id.txtOwner);

            //will only be visible for the owner
            var editFab = FindViewById<FloatingActionButton>(Resource.Id.editFab);
            //fab.AttachToRecyclerView(recyclerView, this);
            editFab.AttachToScrollView(_observableScrollView);
            editFab.Size = FabSize.Normal;
            editFab.Click += EditVehicleProfile;
            editFab.Visibility = ViewStates.Visible;

            _ownerImage = FindViewById<ImageView>(Resource.Id.ownerImage);
            _btnMessageOwner = FindViewById<Button>(Resource.Id.btnMessageOwner);

            //click events
            _ownerImage.Click += OwnerImageClicked;
            _lblOwnerName.Click += OwnerImageClicked;


            //make main image view clickable
            _mainImage.Clickable = true;
            _mainImage.Click += MainImageTapped;

            _btnMessageOwner.Visibility = ViewStates.Gone; //naot applicable to teh owner's profile
        }

        private void EditVehicleProfile(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "FAB Clicked!", ToastLength.Short).Show();

            //load details of clicked car
            var intent = new Intent(this, typeof(EditVehicleActivity));
            intent.PutExtra("VehicleID", _vehicleObjectId);
            //pass the vehicle object id to the next activity
            StartActivity(intent);
        }

        private void OwnerImageClicked(object sender, EventArgs e)
        {
            //show the owner profile here
            var intent = new Intent(this, typeof(OwnerProfileActivity));
            intent.PutExtra("username", _lblOwnerName.Text);
            StartActivity(intent);
        }

        private void MainImageTapped(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(ImagePagerActivity));
            intent.PutExtra("imageObjectId", _imageObjectId);
            intent.PutExtra("vehicleObjectId", _vehicleObjectId);
            StartActivity(intent);
        }

        protected override void OnPause()
        {
            base.OnPause();
            //clear the list temporarily
            _linearLayout.RemoveAllViews();
        }

        protected override async void OnResume()
        {
            base.OnResume();
            try
            {
                if (IsPurchaseRequired())
                {
                    //show alert and end activity
                    Toast.MakeText(this, string.Format("You need to pay ${0} to view vehile details", _amountToPay),
                        ToastLength.Short).Show();
                    Finish(); //end this activity
                }
                else
                {
                    var resp = await PopulateFields();

                    bool firstImageLoaded = false;
                    //lets loop through the images list to see if we have any data
                    if (resp)
                    {
                        if (_images != null)
                        {
                            foreach (var image in _images)
                            {
                                var imageUrl = image.AbsoluteUri;
                                var itemIndex = _images.IndexOf(image);
                                //var bitmap = HelperClass.GetImageBitmapFromUrl(absoluteUri);
                                var imageView = new ImageView(this)
                                {
                                    Id = itemIndex

                                };
                                //imageView.SetPadding(2, 2, 2, 2);
                                imageView.SetPadding(5, 5, 5, 5);
                                //imageView.SetImageBitmap(bitmap);

                                #region Imageloader

                                //ImageSize targetSize = new ImageSize(640,480);
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
                                                //check if we have a main image in imageview
                                                if (!firstImageLoaded)
                                                {
                                                    SetMainImageView(imageUrl);
                                                    firstImageLoaded = true;
                                                }
                                                var resizedImage = HelperClass.ResizeImage(loadedImage, 60);
                                                imageView.SetImageBitmap(resizedImage);
                                                loadedImage.Recycle(); //clear from memory
                                                //Console.WriteLine($"Image dimensions width {loadedImage.Width} height {loadedImage.Height}");
                                            }
                                        },
                                        loadingFailed: (imageUri, view, failReason) =>
                                        {
                                            string message = "Unable to load image";
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

                                imageView.SetScaleType(ImageView.ScaleType.Center);


                                //lets add it to the layout now
                                _linearLayout.AddView(imageView);

                                //add click events
                                imageView.Click += ImageViewOnClick;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MetricsManager.TrackEvent("Error loading vehicle profile" + ex.StackTrace + ex.Message);
            }
        }

        private async Task<bool> PopulateFields()
        {
            bool resp = false;
            try
            {
                if (!_vehicleObjectId.Equals("NA"))
                {
                    resp = true;
                    await _dataManager.LoadSingleVehicleProfile(_vehicleObjectId);

                    _images = CarProfile.CarsForSale;

                    //load the details
                    string price = "POR - (Price On Request)";
                    await _dataManager.LoadSingleVehicleProfile(_vehicleObjectId);

                    if (CarProfile.description != null || !string.IsNullOrEmpty(CarProfile.description))
                    {
                        var description = CarProfile.description.Trim();
                        if (description.Length > 0)
                        {
                            _lblDescription.Text = CarProfile.description;
                        }
                    }

                    if (CarProfile.listPrice != null)
                    {
                        price = HelperClass.CleanUpPrice(CarProfile.listPrice);
                    }

                    _lblPrice.Text = price ?? "POR - (Price On Request)"; ;
                    _lblMake.Text = CarProfile.make ?? "N/A";
                    _lblModel.Text = CarProfile.model ?? "N/A";
                    _lblModelVar.Text = CarProfile.modelVariant ?? "N/A";
                    _lblYear.Text = CarProfile.year ?? "N/A";
                    _lblDriveTrain.Text = CarProfile.driveTrain ?? "N/A";
                    _lblEngine.Text = CarProfile.engine ?? "N/A";
                    _lblTransmission.Text = CarProfile.transmission ?? "N/A";
                    _lblMileage.Text = CarProfile.mileage.ToString();
                    _lblVin.Text = CarProfile.vin ?? "N/A";
                    _lblStockNo.Text = CarProfile.status ?? "N/A";
                    _lblVat.Text = CarProfile.vat ? "Yes" : "No";
                    _lblIntColor.Text = CarProfile.interiorColor ?? "N/A";
                    _lblExtColor.Text = CarProfile.exteriorColor ?? "N/A";
                    _lblOwnerName.Text = CarProfile.ownerUsername ?? "N/A";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error loading vehicle details {0} {1}", ex.Message, ex.StackTrace));
            }

            AndHUD.Shared.Dismiss(this);
            return resp;
        }


        private void ImageViewOnClick(object sender, EventArgs eventArgs)
        {
            //Button btn = sender as Button;
            var img = sender as ImageView;
            if (img != null)
            {
                var imageViewTagId = img.Id;
                //get teh url from the tag number
                string itemURl = _images[imageViewTagId].AbsoluteUri;
                //load this image to main image view
                SetMainImageView(itemURl);
                Console.WriteLine(string.Format("Click event for image view {0} image tag {1}", sender, imageViewTagId));
            }
        }

        private void SetMainImageView(string imageUrl)
        {
            _imageLoader.DisplayImage(imageUrl, _mainImage);
            _imageObjectId = imageUrl;
        }
        private bool IsPurchaseRequired(bool isOwners = false)
        {
            return false;
        }
    }
}