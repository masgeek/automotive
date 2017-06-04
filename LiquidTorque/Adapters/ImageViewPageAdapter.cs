using System;
using System.Collections.Generic;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using HockeyApp;
using liquidtorque.ComponentClasses;
using UK.CO.Senab.Photoview;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;
using UniversalImageLoader.Core.Listener;

namespace liquidtorque.Adapters
{
    class ImageViewPageAdapter : PagerAdapter
    {
        private PhotoViewAttacher _attacher;
       // private PhotoView photoView;
        private List<Uri> _carImages;
        readonly ImageLoader _imageLoader = ImageLoader.Instance;
        public override int Count
        {
            get { return _carImages.Count; }
        }

        
        public ImageViewPageAdapter(List<Uri> carTuples)
        {
            _carImages = carTuples;
        }


        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            try
            {

          
                    var photoView = new PhotoView(container.Context);
            
                //photoView.SetImageResource(Resource.Drawable.car_placeholder);
                //attach listeners for photo tapping
                //attacher = new PhotoViewAttacher(photoView);
                var imageBaseUrl = _carImages[position];

                #region Image downloader

                if (imageBaseUrl != null)
                {
                    var imageUrl = imageBaseUrl.AbsoluteUri;
                    //ImageSize targetSize = new ImageSize(640, 480);
                    _imageLoader.LoadImage(
                        imageUrl,
                        //targetSize,
                        HelperClass.ImageDownloaderOptions,
                        new ImageLoadingListener(
                            loadingStarted: delegate
                            {
                                //placeholder for loading image
                                //photoView.SetImageResource(Resource.Drawable.car_placeholder);
                            },
                            loadingComplete: (imageUri, view, loadedImage) =>
                            {
                                if (loadedImage != null)
                                {
                                    photoView.SetImageBitmap(loadedImage);
                                    // Now just add PhotoView to ViewPager and return it
                                    //set tag
                                    //photoView.Tag = position;
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
                            }));

                }

                #endregion Image downloader
                container.AddView(photoView, ViewPager.LayoutParams.MatchParent, ViewPager.LayoutParams.MatchParent);
                //attacher.PhotoTap += ImageTapped;

                return photoView;
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

            return null;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object objectValue)
        {
            container.RemoveView((View)objectValue);
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
        {
            return view == objectValue;
        }
    }
}