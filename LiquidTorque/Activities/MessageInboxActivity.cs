using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;

namespace liquidtorque.Activities
{
    [Activity(Label = "Inbox")]
    public class MessageInboxActivity : BaseActivity
    {
        private string _username;
        DataManager _dataManager = DataManager.GetInstance();
        private ListView _messagesList;
        private List<ConversationList> _messageThread;
        protected override int LayoutResource
        {
            get { return Resource.Layout.message_inbox; }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetSupportActionBar(Toolbar);

            if (Intent != null)
            {
                _username = Intent.GetStringExtra("username");
            }

            _messagesList = FindViewById<ListView>(Resource.Id.messagesList);

            //click events
            _messagesList.ItemClick += ChatListViewItemClick;
        }

        protected override void OnResume()
        {
            base.OnResume();
            FetchUserMessages(_username);
        }

        #region click actions
        private void ChatListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                var chatPosition = e.Position;
                var conversationId = _messageThread[chatPosition].ConversationID;
                var recipient = _messageThread[chatPosition].RecipientId;
                var intent = new Intent(this, typeof(MessageThreadActivity));
                intent.PutExtra("username", _username);
                intent.PutExtra("conversationId", conversationId);
                intent.PutExtra("recipient", recipient);
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Chat loading exception {0} {1}", ex.Message, ex.StackTrace));
                Toast.MakeText(Application.Context, "Unable to load chat, please try again later",
                    ToastLength.Short).Show();
            }
        }
        //handles the back arrow on the action bar
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                Finish();

            return base.OnOptionsItemSelected(item);
        }
        #endregion
        private async void FetchUserMessages(string username)
        {
             _messageThread = await _dataManager.FetchConversation(username); //FetchMessages(username);
            if (_messageThread != null && _messageThread.Count > 0)
            {
                _messagesList.Adapter = new MessageListAdapter(this, _messageThread);
            }
        }
    }
}