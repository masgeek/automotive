using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Provider;
using Android.Telephony;
using Android.Widget;
using HockeyApp;
using Parse;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;

namespace liquidtorque.ComponentClasses
{
    [SuppressMessage("ReSharper", "ConvertPropertyToExpressionBody")]
    public static class HelperClass
    {
        /// <summary>
        /// Return range of years upto 1930
        /// I.E current year upto 1930
        /// happy usage
        /// </summary>
        public static List<string> ListYears
        {
            get { return GenerateYearRange(); }
        }

        public static string CurrentUser
        {
            get { return GetCurrentUser(); }
        }

        public static string BuildNumber
        {
            get { return GetPackageInfo(); }
        }

        public static DisplayImageOptions ImageDownloaderOptions
        {
            get { return SetImageOptions(); }
        }

        public static string LoggedInUser {
            get { return GetCurrentUser();}
        }
        private static DisplayImageOptions SetImageOptions()
        {
            var options = new DisplayImageOptions.Builder()
                .ShowImageForEmptyUri(Resource.Drawable.car_placeholder)
                .ShowImageOnLoading(Resource.Drawable.car_placeholder)
                .ShowImageOnFail(Resource.Drawable.notification_template_icon_bg)
                .ResetViewBeforeLoading(false)
                .CacheOnDisk(true)
                .CacheInMemory(false)
                .ImageScaleType(ImageScaleType.InSamplePowerOf2)
                .BitmapConfig(Bitmap.Config.Rgb565)
                .ConsiderExifParams(false)
                //.Displayer(new FadeInBitmapDisplayer(500))
                .Build();
            /*            options = new DisplayImageOptions.Builder()
                .ShowImageOnLoading(Resource.Drawable.car_placeholder)
                .ShowImageForEmptyUri(Resource.Drawable.ic_facebook)
                .ShowImageOnFail(Resource.Drawable.lt_icon)
                .CacheInMemory(false)
                .CacheOnDisk(true)
                .ConsiderExifParams(false)
                .Displayer(new RoundedBitmapDisplayer(5))
                .Build();
                */
            return options;
        }

        /// <summary>
        /// Generate a list of years
        /// sort by order of current year to oldest year i.e 1930
        /// </summary>
        /// <returns>string collection</returns>
        public static List<string> GenerateYearRange(bool isFilterScenario = false)
        {
            List<string> modelYears = new List<string> {isFilterScenario ? "All" : "Select year"};


            List<int> listYears = Enumerable.Range(1930, DateTime.Now.Year - 1930 + 1).ToList();

            var sortedYears = listYears.OrderByDescending(p => p).ToList();

            foreach (var year in sortedYears)
            {
               modelYears.Add(year.ToString());
            }
            return modelYears;
        }

        private static string GetCurrentUser()
        {
            var currentUser = ParseUser.CurrentUser;

            return currentUser.Username;
        }

        /// <summary>
        /// Gets the path of the locally picked image
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetPathToImage(Android.Net.Uri uri)
        {
            string path = null;
            try
            {
                ICursor cursor = Application.Context.ContentResolver.Query(uri, null, null, null, null);
                cursor.MoveToFirst();
                string documentId = cursor.GetString(0);
                try
                {
                    documentId = documentId.Split(':')[1];
                }
                catch (Exception ex)
                {
                    documentId = documentId.Split(':')[0];
                    Console.WriteLine("Error splitting the string fallback {0}", ex.Message);
                }
                cursor.Close();

                cursor = Application.Context.ContentResolver.Query(
                    MediaStore.Images.Media.ExternalContentUri,
                    null, MediaStore.Images.Media.InterfaceConsts.Id + " = ? ", new[] {documentId}, null);
                cursor.MoveToFirst();
                path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
                cursor.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exeption {0} {1}", ex.Message, ex.StackTrace);
            }

            return path;
        }

        /// <summary>
        /// Gets the bitmap image from the image URL remote or local
        /// </summary>
        /// <param name="url">url value of the image</param>
        /// <returns></returns>
        public static Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;
            Bitmap resizeImg = null;
            try
            {
                if (!string.IsNullOrEmpty(url))
                {

                    using (var webClient = new WebClient())
                    {
                        var imageBytes = webClient.DownloadData(url);
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                        }
                    }
                }
                if (imageBitmap != null)
                {
                    resizeImg = ResizeImage(imageBitmap);
                    if (imageBitmap != null)
                    {
                        imageBitmap.Recycle();
                    }
                }
            }
            catch (Java.Lang.OutOfMemoryError ex)
            {
                Console.WriteLine(string.Format("Memory error {0} {1}", ex.Message, ex.StackTrace));
                Toast.MakeText(Application.Context,
                    string.Format("Memory error {0}", ex.Message), ToastLength.Short).Show();

                MetricsManager.TrackEvent("Image selection error " + ex.Message + ex.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine("Image resizing formatting issue " + e.Message + e.StackTrace);
            }
            return resizeImg;

        }

        public static Bitmap ResizeImageTest(Bitmap bm, int newWidth = 1280, int newHeight = 720)
        {
            var scaledBitmap = Bitmap.CreateScaledBitmap(bm, newWidth, newHeight, true);
            return scaledBitmap;
        }


        /// <summary>
        /// Resize a bitmap image
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static Bitmap ResizeImage(Bitmap bm, float percent = 80)
        {
            float resizePercentage = percent/100;

            Bitmap scaledBitmap = null;
            int width = bm.Width;
            int height = bm.Height;
            try
            {
                int scaleWidth = (int)(resizePercentage*width);
                int scaleHeight = (int)(resizePercentage*height);
                // CREATE A MATRIX FOR THE MANIPULATION
                Matrix matrix = new Matrix();
                // RESIZE THE BIT MAP
                matrix.PostScale(scaleWidth, scaleHeight);

                // "RECREATE" THE NEW BITMAP
                /* Bitmap resizedBitmap = Bitmap.createBitmap(
                 bm, 0, 0, width, height, matrix, false);*/
            //scaledBitmap = Bitmap.CreateBitmap(bm, 0, 0, width, height, matrix, true);
            //CreateScaledBitmap(bm, width, height, true);
            scaledBitmap = Bitmap.CreateScaledBitmap(bm, scaleWidth, scaleHeight, true);
                //bm.Recycle(); //clear from memory
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error resizing bitmap {0} {1}", ex.Message, ex.StackTrace);
            }
            return scaledBitmap;
        }

        /// <summary>
        /// Here we convert the currency to a standard number format and so on
        /// All non numerical values wil be stripped, so do not add 0.00 at the end
        /// or the price will change and be huge :-) hehe
        /// we will cater for decimal values
        /// </summary>
        /// <param name="listPrice"></param>
        /// <param name="currency">$</param>
        /// <returns>Currency in string format</returns>
        public static string CleanUpPrice(string listPrice, string currency = "$")
        {
            string formattedPrice = null;
            try
            {
                if (!string.IsNullOrEmpty(listPrice))
                {
                    //first remove all non numeric values
                    string result = Regex.Replace(listPrice, @"[^\d]", "");
                    //next convert to number with thousands separator

                    double priceDouble = double.Parse(result);
                    formattedPrice = string.Format("{0} {1:N2}", currency, priceDouble);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Currency formatting issue " + e.Message + e.StackTrace);
                formattedPrice = listPrice;
            }

            return formattedPrice;
        }

        private static string GetPackageInfo()
        {
            Context context = Application.Context;
            string name = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
            //int code = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
            return name;
        }

        private static string GetLoggedInUser()
        {
            string username = null;
            ParseUser currentUser = ParseUser.CurrentUser;
            if (currentUser != null)
            {
                username = currentUser.Username;
            }

            return username;
        }
    }
}