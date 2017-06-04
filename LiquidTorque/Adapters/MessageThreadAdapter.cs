using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using DataAccessLayer;
using liquidtorque.ComponentClasses;
using Object = Java.Lang.Object;

namespace liquidtorque.Adapters
{
    public class MessageThreadAdapter : BaseAdapter
    {

        private readonly LayoutInflater _inflater;
        private readonly List<ChatMessage> _messageThread;


        public MessageThreadAdapter(Context context, List<ChatMessage> messageThreadList)
        {
            _inflater = LayoutInflater.From(context);
            _messageThread = messageThreadList;
        }

        public override int Count
        {
            get { return _messageThread.Count; }
        }

        public override Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //View view = convertView;
            View view;
            ViewHolder holder;
            var item = _messageThread.ElementAt(position);

            //inflat layout based on sender or reciever

            view = _inflater.Inflate(item.senderUserID.Equals(HelperClass.CurrentUser) ? Resource.Layout.chat_right : Resource.Layout.chat_left, parent, false);

            holder = new ViewHolder
                {
                    mainText = view.FindViewById<TextView>(Resource.Id.msgr),
                    subText = view.FindViewById<TextView>(Resource.Id.timeText),
                };
                view.Tag = holder;


            var localTime = item.readDate.ToLocalTime();
            holder.mainText.Text = item.message;
            holder.subText.Text = localTime.ToString(CultureInfo.InvariantCulture);

            return view;
        }
    }
}