using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using Object = Java.Lang.Object;

namespace liquidtorque.Adapters
{
    public class MessageListAdapter : BaseAdapter
    {
        private readonly LayoutInflater _inflater;
        private readonly List<ConversationList> _conversation;


        public MessageListAdapter(Context context, List<ConversationList> conversationList)
        {
            _inflater = LayoutInflater.From(context);
            _conversation = conversationList;
        }


        public override int Count
        {
            get { return _conversation.Count;}
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
            View view = convertView;
            ViewHolder holder;
            if (convertView == null)
            {
                view = _inflater.Inflate(Resource.Layout.messages_row, parent, false);
                holder = new ViewHolder
                {
                    mainText = view.FindViewById<TextView>(Resource.Id.usernameText),
                    subText = view.FindViewById<TextView>(Resource.Id.dateText),
                    image = view.FindViewById<ImageView>(Resource.Id.Thumbnail)
                };
                view.Tag = holder;


            }
            else
            {
                holder = (ViewHolder)view.Tag;
            }
            //get the item index position
            var item = _conversation.ElementAt(position);
            var currentUser = HelperClass.CurrentUser;
            var sender = item.SenderId;
            var recipient = item.RecipientId;

            holder.mainText.Text = currentUser.Equals(recipient) ? sender : recipient;
            var localTime = item.UpdatedAt.ToLocalTime();

            holder.subText.Text = localTime.ToString(CultureInfo.InvariantCulture);
     
            return view;
        }
    }
}