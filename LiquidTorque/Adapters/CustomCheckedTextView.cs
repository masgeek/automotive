using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using liquidtorque.ComponentClasses;

namespace liquidtorque.Adapters
{
    class CustomCheckedTextView : ArrayAdapter<List<string>>
    {
        private View _view;
        Context _context;
        public CustomCheckedTextView(Context context, int textViewResourceId, IList<List<string>> objects) : base(context, Android.Resource.Id.Text1, objects)
        {
            _context = context;
        }


        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.list_item_row, null);
            _view =  GetView(position, convertView, parent);

            var text = _view.FindViewById<TextView>(Resource.Id.text);
            text.Text = "hello";
            text.Typeface = FontClass.BoldTypeface;

            return _view;
        }
    }
}