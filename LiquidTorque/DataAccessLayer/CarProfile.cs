using System;
using System.Collections.Generic;
using Android.Widget;

namespace liquidtorque.DataAccessLayer
{
    /// <summary>
    /// Returns data for the vehicle profile when the vehicle cell is double tapped
    /// </summary>
    public static class CarProfile
    {
        /// <summary>
        /// Ges/sets the car object id from the vehicle profile object
        /// </summary>
        public static string CarObjectId { get; set; }
        public static string status { get; set; }
        public static string make { get; set; }
        public static string model { get; set; }
        public static string modelVariant { get; set; }
        public static string year { get; set; }
        public static int mileage { get; set; }
        public static string milesData { get; set; }
        public static string exteriorColor { get; set; }
        public static string interiorColor { get; set; }
        public static string driveTrain { get; set; }
        public static string options { get; set; }
        public static string engine { get; set; }
        public static string fuelType { get; set; }
        public static string transmission { get; set; }
        public static string listPrice { get; set; }
        public static string carType { get; set; }
        public static string vin { get; set; }
        public static string ownerUsername { get; set; }
        public static string price { get; set; }
		public static string description { get; set; }
        public static bool vat { get; set; }
        public static bool priceOnRequest { get; set; }
        public static string condition { get; set; }

        public static Uri ImageUrl { get; set; }

        public static List<Uri> CarsForSale;

        /// <summary>
        /// Holde the images in the scrollview for the invrntory details after clicking main image
        /// </summary>
        public static List<Uri> InventoryCarImages;
        static CarProfile()
        {
            //allocate the list
            CarsForSale = new List<Uri>();
            InventoryCarImages = new List<Uri>();
        }
        /// <summary>
        /// load the image from the url source
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static ImageView FromUrl(string uri)
        { /*

            using (var url = new androidhtt(uri))
            using (var data = NSData.FromUrl(url))
                return UIImage.LoadFromData(data);
                */

            return null;
        }

        public static void VehicleInventoryImages(Uri imgUrl)
        {
            InventoryCarImages.Add(imgUrl);
        }

        public static void VehicleImages(Uri imgUrl)
        {
            CarsForSale.Add(imgUrl);
        }

        public static void ClearInventoryImages()
        {
			if (InventoryCarImages != null)
            {
                var remove = Math.Max(0, InventoryCarImages.Count);
                InventoryCarImages.RemoveRange(0, remove);
            }
        }
        public static void ResetFields()
        {

            //clear al the previous values
            CarObjectId = null;
            status = null;
            make = null;
            model = null;
            modelVariant = null;
            year = null;
            mileage = 0;
            exteriorColor = null;
            interiorColor = null;
            driveTrain = null;
            options = null;
            engine = null;
            fuelType = null;
            transmission = null;
            listPrice = null;
            carType = null;
            vin = null;
            ownerUsername = null;
            price = null;
            vat = false;
            priceOnRequest = false;
            condition = null;
			description = null;

            if (CarsForSale != null)
            {
                var remove = Math.Max(0, CarsForSale.Count);
                CarsForSale.RemoveRange(0, remove);
            }
        }
    }
}
