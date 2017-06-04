using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using HockeyApp;

namespace liquidtorque.Adapters
{
    /// <summary>
    /// Shows a list of the currenly logged in user's requests
    /// </summary>
    public class RequestsListViewAdapter : ArrayAdapter<Tuple<string, string,string,string>>
    {
        readonly Activity _context;
        private View _view;
        private readonly IList<Tuple<string, string, string, string>> _requests;
        internal static readonly List<string> DisplayedImages = new List<string>();

        /// <summary>
        /// Initialize the requests constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requests"></param>
        public RequestsListViewAdapter(Activity context,
            IList<Tuple<string, string, string, string>> requests)
            : base(context,Android.Resource.Id.Text1, requests)
        {
            _context = context;
            _requests = requests;

        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                //view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
                _view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem2, null);

                var item = GetItem(position);
                var objectId = !string.IsNullOrEmpty(item.Item1) ? item.Item1 : "N/A";
                var makeName = !string.IsNullOrEmpty(item.Item1) ? item.Item2 : "N/A";
                var modelName = !string.IsNullOrEmpty(item.Item2) ? item.Item3 : "N/A";
                var year = !string.IsNullOrEmpty(item.Item3) ? item.Item4 : "N/A";

                var modelYear = string.Format(" {0} ({1})", modelName, year);
                _view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = makeName;
                _view.FindViewById<TextView>(Android.Resource.Id.Text2).Text =modelYear;
                //view.FindViewById<ImageView>(Android.Resource.Id.Icon).SetImageResource(item.ImageResourceId);// only use with ActivityListItem
            }
            catch (Exception ex)
            {
                var message = string.Format("A general exception has occurred in requests list adpater {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

            return _view;
        }

        /// <summary>
        /// Return the number of items in the requests list
        /// </summary>
        public override int Count
        {
            get { return _requests.Count; }
        }

        //get the unique item id
        public override long GetItemId(int position)
        {
            return position;
        }
    }
}