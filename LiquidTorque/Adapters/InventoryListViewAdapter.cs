using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Widget;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;
using UniversalImageLoader.Core.Display;

namespace liquidtorque.Adapters
{
    public class InventoryListViewAdapter : ArrayAdapter<Tuple<string, string, string, string, Uri>>
    {
        readonly Activity _context;
        private readonly IList<Tuple<string, string, string, string, Uri>> _cars;

        // Get singleton instance of the image loader
        readonly ImageLoader _imageLoader = ImageLoader.Instance;

        private readonly DisplayImageOptions _options;
        //private View _fragmentView;
        internal static readonly List<string> DisplayedImages = new List<string>();


        public InventoryListViewAdapter(Activity context, int resource,
            IList<Tuple<string, string, string, string, Uri>> cars)
            : base(context, resource, Android.Resource.Id.Text1, cars)
        {
            _context = context;
            _cars = cars;
            _options = new DisplayImageOptions.Builder()
                .ShowImageForEmptyUri(Resource.Drawable.car_placeholder)
                .ShowImageOnLoading(Resource.Drawable.car_placeholder)
                .ShowImageOnFail(Resource.Drawable.notification_template_icon_bg)
                .ResetViewBeforeLoading(true)
                .CacheOnDisk(true)
                .CacheInMemory(false)
                .ImageScaleType(ImageScaleType.Exactly)
                .BitmapConfig(Bitmap.Config.Rgb565)
                .ConsiderExifParams(false)
                .Displayer(new FadeInBitmapDisplayer(1000))
                .Build();
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
    }
}