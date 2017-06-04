// 2016
// 10

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Widget;

// ReSharper disable once CheckNamespace
namespace liquidtorque
{
    public class SharedImagesCache
    {
        private static List<Tuple<string, string, string, Android.Net.Uri>> _imagesToCacheList;
        //private static List<string> otherVehicleImages;
        //static nint cacheMaxCacheAge = 60 * 60 * 24 * 7; // 1 week

		/// <summary>
		/// Downloads the and cache home images.
		/// </summary>
		/// <returns>The and cache home images.</returns>
		/// <param name="vehcileImages">Vehcile images.</param>
        public static async Task<bool> DownloadAndCacheHomeImages(List<Tuple<string, string, string, Android.Net.Uri>> vehcileImages)
		{
		   bool homepageCacheSuccessfull = false;
		    try
		    {
		        _imagesToCacheList = vehcileImages;

		        // This method runs asynchronously.
		        homepageCacheSuccessfull = await Task.Run(() => CacheHomeImages(_imagesToCacheList));

		        //now call parallel function to download the other images
		        //no need to implment this one for now
		        //await Task.Run(() => GetOherImages());
		    }
		    catch (Exception ex)
		    {
                Console.WriteLine("Cache creation error "+ex.Message+ex.StackTrace);
		    }
		    return homepageCacheSuccessfull;
        }

        private static bool CacheHomeImages(List<Tuple<string, string, string, Android.Net.Uri>> homeImagesCache)
        {

            try
            {
                Parallel.ForEach(homeImagesCache, img =>
                {
						var existsInCache = ExistsInCache(img.Item4);
						Console.WriteLine("Does it exist in cache "+existsInCache);
						if(existsInCache){
							Console.WriteLine("It exists in the cache ");
						}else{
                        //@TODO Add independent image downloader here
							/*SDWebImageDownloader.SharedDownloader.DownloadImage(
								url: new NSUrl(img.Item4.AbsoluteUri),
								options: SDWebImageDownloaderOptions.LowPriority,
									progressBlock: (size, expectedSize) =>
									{
										Console.WriteLine("Current size {0} expected size {1}", size, expectedSize);
									},

									completedBlock: (image, data, error, finished) =>
									{
										if (image != null && finished)
										{

											//add the image to the cache with the object id as the key and the image URL also, also cache to disk
											SDImageCache.SharedImageCache.StoreImage(image: image, key: img.Item4.AbsoluteUri, toDisk: true);
											Console.WriteLine("Image cached successfully with key "+img.Item4.AbsoluteUri);
										}
								});
                                */
							}
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Image downloading exception " + ex.Message + ex.StackTrace);
            }

            return false;
        }

        /// <summary>
        /// Checks if the image already exists in the disk cache
        /// Requires no checking of android
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
         private static bool ExistsInCache(Android.Net.Uri imageUrl)
		{
            //@TODO replace with image component for android
            return false;
		}

		/// <summary>
		/// Gets the image from the disk cache, if it doesnt exist we will download it from parse
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="imageUrl">Image URL.</param>
        public static ImageView GetImage(Android.Net.Uri imageUrl)
		{
		    ImageView myCache = null;

		    try
		    {
                //@TODO android image downloader equivalent
		       // myCache = SDImageCache.SharedImageCache.ImageFromDiskCache(imageUrl.AbsoluteUri);

		        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
		        if (myCache == null)
		        {
		            Console.WriteLine("Image not in cache downloading it now");
		            myCache = DownloadUncachedImage(imageUrl);
		        }

		        Console.WriteLine("Returning cached image back");
		    }
		    catch (Exception ex)
		    {
                Console.WriteLine("Image cache exception due to unfinshed uploading "+ex.Message+ex.StackTrace);
		    }
		    return myCache;
        }

		/// <summary>
		/// Downloads the uncached image.
		/// </summary>
		/// <returns>The uncached image.</returns>
		/// <param name="imageUrl">Image URL.</param>
		private static ImageView DownloadUncachedImage(Android.Net.Uri imageUrl)
		{
            ImageView myCache = null;
			try {
                //@TODO Android image downloader equivalent
                myCache.SetImageURI(imageUrl);
                /*
				SDWebImageDownloader.SharedDownloader.DownloadImage (
					url: new NSUrl (imageUrl.AbsoluteUri),
					options: SDWebImageDownloaderOptions.HighPriority,
					progressBlock: (size, expectedSize) => {
						Console.WriteLine ("Current size {0}KB expected size {1}KB", size, expectedSize);
					},

					completedBlock: (image, data, error, finished) => {
						if (image != null && finished) {

							//add the image to the cache with the object id as the key and the image URL also then cache to disk
							SDImageCache.SharedImageCache.StoreImage (image: image, key: imageUrl.AbsoluteUri, toDisk: true);
							Console.WriteLine ("Image cached successfully with key " + imageUrl.AbsoluteUri);
						}
					});

				myCache = SDImageCache.SharedImageCache.ImageFromDiskCache (imageUrl.AbsoluteUri);
                */

			} catch (Exception downloadEx) {
				Console.WriteLine ("Image downloading issue " + downloadEx.Message + downloadEx.StackTrace);
			}

			return myCache;
		}
    }
}