using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Widget;
using Java.Util;

namespace liquidtorque.DataAccessLayer
{
	public class Car
	{
		public string objectId { get; set; }
		public string status { get; set; }
		public string make { get; set; }
		public string model { get; set; }
		public string modelVariant { get; set; }
		public string year { get; set; }
		public int mileage { get; set; }
		public string milesData { get; set; }
		public string exteriorColor { get; set; }
		public string interiorColor { get; set; }
		public string driveTrain { get; set; }
		public string options { get; set; }
		public string engine { get; set; }
		public string fuelType { get; set; }
		public string transmission { get; set; }
		public string listPrice { get; set; }
		public string carType { get; set; }
		public string vin { get; set; }
		public string ownerUsername { get; set; }
		public string price { get; set; }
		public bool vat { get; set; }
		public bool priceOnRequest { get; set; }
		public string condition { get; set; }
		public string description { get; set; }

        public string stockNumber { get; set; }
        //@TODI Image element for android
        public ImageView frontView { get; set;}




	
		private static List<Bitmap> homePageImages;


		private static ArrayList images;


		private static List<Bitmap> carsForSale;

		private static List<Bitmap> userRequests;

	    private static List<string> selectedImageList;

		public Car()
		{
			images = new ArrayList();

			carsForSale = new List<Bitmap>();

			userRequests = new List<Bitmap>();

			homePageImages = new List<Bitmap>();

            selectedImageList = new List<string>();

		}
			
		public ImageView  Image { get; set; }

		public Uri ImageUrl { get; set; }

		public static string Name { get; set; }

		public Uri Url { get; set; }

        //@FIX Android images thumbnails holder list
        public static ArrayList AllThumbnails
        {
            get { return images; }
        }

	    //@FIX Android images
        public List<Bitmap> MainImages
        {
            get { return homePageImages; }
        }

	    //@FIX Android images
		public List<Bitmap> CarListingImages
		{
		    get { return carsForSale; }
		}

	    //@FIX Android images
        public List<Bitmap> RequestImages
        {
            get { return userRequests; }
        }

	    public List<string> SelectedImagePaths
	    {
	        get { return selectedImageList; }
	    }
	}
}

