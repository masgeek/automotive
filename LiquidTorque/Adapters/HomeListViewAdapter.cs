using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using HockeyApp;
using liquidtorque.ComponentClasses;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;
using UniversalImageLoader.Core.Display;
using UniversalImageLoader.Core.Listener;
using Console = System.Console;

namespace liquidtorque.Adapters
{
    public class HomeListViewAdapter : ArrayAdapter<Tuple<string, string, string, string, Uri, string>>
    {
        readonly Activity _context;
        private readonly IList<Tuple<string, string, string, string, Uri, string>> _cars;

        // Get singleton instance of the image loader
        readonly ImageLoader _imageLoader = ImageLoader.Instance;

        private View _fragmentView;
        private LayoutInflater inflater;
        internal static readonly List<string> DisplayedImages = new List<string>();
        private IImageLoadingListener animateFirstListener = new AnimateFirstDisplayListener();

        public HomeListViewAdapter(Activity context, IList<Tuple<string, string, string, string, Uri, string>> cars)
            : base(context, Android.Resource.Id.Text1, cars)
        {
            _context = context;
            _cars = cars;

            //clear any initial images
            AnimateFirstDisplayListener.DisplayedImages.Clear();
        }

        /// <summary>
        /// Return the number of items in the cars list
        /// </summary>
        public override int Count
        {
            get { return _cars.Count; }
        }

        //get the uniqe item id
        public override long GetItemId(int position)
        {
            return position;
        }

       
        public View GetViewOld(int position, View convertView, ViewGroup parent)
        {
            try
            {
                //@TODO add out of memory  exception catch, if it occurs clear the cache
                // re-use an existing view, if one is available otherwise create a new one
                _fragmentView = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.list_item_row, null);

                var item = GetItem(position);
                var makeName = !string.IsNullOrEmpty(item.Item2) ? item.Item2 : "N/A";
                var price = !string.IsNullOrEmpty(item.Item3) ? item.Item3 : "POR";
                var modelName = !string.IsNullOrEmpty(item.Item4) ? item.Item4 : "N/A";

                var imageBaseUrl = item.Item5;

                //get the elements and populate with data
                _fragmentView.FindViewById<TextView>(Resource.Id.Make).Text = makeName;
                _fragmentView.FindViewById<TextView>(Resource.Id.Model).Text = modelName;
                _fragmentView.FindViewById<TextView>(Resource.Id.Price).Text = price;
               // var spinner = _fragmentView.FindViewById<ProgressBar>(Resource.Id.loading);
                var imageView = _fragmentView.FindViewById<ImageView>(Resource.Id.Thumbnail);

                if (imageBaseUrl != null)
                {
                    var imageUrl = imageBaseUrl.AbsoluteUri;
                    ImageSize targetSize = new ImageSize(720, 480);
                    _imageLoader.LoadImage(
                        imageUrl,
                        targetSize,
                        HelperClass.ImageDownloaderOptions,
                        new ImageLoadingListener(
                            loadingStarted: delegate
                            {
                                //spinner.Visibility = ViewStates.Visible;
                            },
                            loadingComplete: (imageUri, view, loadedImage) =>
                            {
                                if (loadedImage != null)
                                {
                                    //Bitmap loadedImage = e.LoadedImage;
                                    // Do whatever you want with Bitmap (80x50)
                                    imageView.SetImageBitmap(loadedImage);
                                    //spinner.Visibility = ViewStates.Gone;
                                }
                            },
                            loadingFailed: (imageUri, view, failReason) =>
                            {
                                string message;
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

                                //spinner.Visibility = ViewStates.Gone;
                            }));

                }
            }
            catch (Java.Lang.OutOfMemoryError er)
            {
                Console.WriteLine(String.Format("Java ran out of memory {0} {1}", er.Message, er.StackTrace));
                MetricsManager.TrackEvent(String.Format("Java ran out of memory {0} {1}", er.Message, er.StackTrace));
            }
            catch (OutOfMemoryException memEx)
            {
                Console.WriteLine(String.Format("Application has run out of memory {0} {1}", memEx.Message,
                    memEx.StackTrace));
                MetricsManager.TrackEvent(String.Format("Application has run out of memory {0} {1}", memEx.Message,
                    memEx.StackTrace));
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("A general exception has occurred {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent(String.Format("A general exception has occurred {0} {1}", ex.Message,
                    ex.StackTrace));
            }

            //return the fragment view
            return _fragmentView;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                //@TODO add out of memory  exception catch, if it occurs clear the cache
                // re-use an existing view, if one is available otherwise create a new one
                _fragmentView = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.list_item_row, null);

                var item = GetItem(position);
                var imageView = _fragmentView.FindViewById<ImageView>(Resource.Id.Thumbnail);
                var spinner = _fragmentView.FindViewById<ProgressBar>(Resource.Id.loading);

                spinner.Visibility = ViewStates.Gone;
                var imageBaseUrl = item.Item5;
                if (imageBaseUrl != null)
                {
                    var imageUrl = imageBaseUrl.AbsoluteUri;

                    //ImageLoader.Instance.DisplayImage(imageUrl, imageView, HelperClass.ImageDownloaderOptions, animateFirstListener);
                    ImageSize targetSize = new ImageSize(360, 240);
                    _imageLoader.LoadImage(
                        imageUrl,
                        targetSize,
                        HelperClass.ImageDownloaderOptions,
                        new ImageLoadingListener(
                            loadingStarted: delegate
                            {
                                imageView.SetImageResource(Resource.Drawable.car_placeholder);
                            },
                            loadingComplete: (imageUri, view, loadedImage) =>
                            {
                                if (loadedImage != null)
                                {
                                    //Bitmap loadedImage = e.LoadedImage;
                                    // Do whatever you want with Bitmap (80x50)
                                    imageView.SetImageBitmap(loadedImage);
                                    //recycle the loaded image
                                }
                            },
                            loadingFailed: (imageUri, view, failReason) =>
                            {
                                string message = "Unknown error";
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
                                Console.WriteLine(message);
                                //clear teh memory
                                _imageLoader.ClearMemoryCache();
                               
                                //Toast.MakeText(view.Context, message, ToastLength.Short).Show();
                            }), 
                        new ImageLoadingProgressListener(
                                progressUpdate: (imageUri, view, current, total) =>
                                {
                                    var progress = (int) (100.0f*current/total);
                                    Console.WriteLine("Downloading progress " + progress);
                                }));
                }
            }
            catch (Java.Lang.OutOfMemoryError er)
            {
                Console.WriteLine(String.Format("Java ran out of memory {0} {1}", er.Message, er.StackTrace));
                MetricsManager.TrackEvent(String.Format("Java ran out of memory {0} {1}", er.Message, er.StackTrace));
            }
            catch (OutOfMemoryException memEx)
            {
                Console.WriteLine(String.Format("Application has run out of memory {0} {1}", memEx.Message,
                    memEx.StackTrace));
                MetricsManager.TrackEvent(String.Format("Application has run out of memory {0} {1}", memEx.Message,
                    memEx.StackTrace));
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("A general exception has occurred {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent(String.Format("A general exception has occurred {0} {1}", ex.Message,
                    ex.StackTrace));
            }

            //return the fragment view
            return _fragmentView;
        }

        private class AnimateFirstDisplayListener : SimpleImageLoadingListener
        {
            internal static readonly List<string> DisplayedImages = new List<string>();

            public override void OnLoadingComplete(string imageUri, View view, Bitmap loadedImage)
            {
                if (loadedImage != null)
                {
                    lock (DisplayedImages)
                    {
                        ImageView imageView = (ImageView)view;
                        bool firstDisplay = !DisplayedImages.Contains(imageUri);
                        if (firstDisplay)
                        {
                            FadeInBitmapDisplayer.Animate(imageView, 500);
                            DisplayedImages.Add(imageUri);
                        }
                    }
                }
            }
        }
    }

}
