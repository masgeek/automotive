using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DataAccessLayer;
using HockeyApp;
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;

namespace liquidtorque.Activities
{
    [Activity(Label = "MessageThreadActivity")]
    public class MessageThreadActivity : BaseActivity
    {
        private string _username;
        private string _conversationId;
        private string _recipient;
        private string chatMessage;

        readonly DataManager _dataManager = DataManager.GetInstance();

        private ListView chatList;
        private EditText chatMessageText;
        private Button btnSend;
        private List<ChatMessage> _messageThread;
        protected override int LayoutResource
        {
            get { return Resource.Layout.chat_thread; }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            if (Intent != null)
            {
                _username = Intent.GetStringExtra("username");
                _recipient = Intent.GetStringExtra("recipient");
                _conversationId = Intent.GetStringExtra("conversationId");
            }
            //get the elements
            chatList = FindViewById<ListView>(Resource.Id.chatList);
            chatMessageText = FindViewById<EditText>(Resource.Id.chatMessage);
            btnSend = FindViewById<Button>(Resource.Id.btnSend);

            //add click actions
            btnSend.Click += BtnSendMessageClick;
        }

        private async void BtnSendMessageClick(object sender, EventArgs e)
        {
            try
            {
                chatMessage = chatMessageText.Text.Trim();

                if (!string.IsNullOrEmpty(chatMessage) && !string.IsNullOrEmpty(_conversationId) && chatMessage.Length > 0)
                {
                    //send if not null or empty
                    var msg = new ChatMessage
                    {
                        message = chatMessage,
                        senderUserID = _username,
                        receipientUserID = _recipient,
                        conversationID = _conversationId,
                        sendDate = DateTime.UtcNow, //get the utc time code
                        hasBeenRead = false,
                        rightBubble = true //always show on the right bubble
                    };
                    //send it to the backend
                     _dataManager.SaveUserMessage(msg);

                    //update the conversation table
                    _dataManager.UpdateConversationDates(_conversationId);

                    //after sending add it to the list view
                    _messageThread.Add(msg);
                    DisplayMessages(_messageThread);
                    //empty the chat message
                    chatMessage = null;
                    chatMessageText.Text = null;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Unable to send message {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
        }

        protected override async void OnResume()
        {
            base.OnResume();
            var convThreadExists = await ConversationExists(_username, _recipient);
            if (!convThreadExists)
            {
                //create conversation if it doesnt exist
                _conversationId = await CreateConversationThread(_username, _recipient);
            }
            if (string.IsNullOrEmpty(_conversationId))
            {
                _conversationId = await GetConversationID(_username, _recipient);
            }
            FetchUserMessages(_username,_conversationId);
        }

        private async void FetchUserMessages(string username,string conversationId)
        {
            _messageThread = await _dataManager.FetchMessages(username, conversationId);
            DisplayMessages(_messageThread);
        }

        void DisplayMessages(List<ChatMessage> chat)
        {
            if (chat != null)
            {
                chatList.Adapter = new MessageThreadAdapter(this, chat);
                //// Select the last row so it will scroll into view...
                //myListView.setSelection(myListAdapter.getCount() - 1);
                chatList.SetSelection(chat.Count - 1);
            }
        }
        /// <summary>
        /// Check if a conversation exists before seinf the message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <returns></returns>
        async Task<bool> ConversationExists(string sender,string recipient)
        {
            //if the conversation does not exists create a new
            var conversationId = await _dataManager.CheckConversationAlreadyExists(sender, recipient);

            return !string.IsNullOrEmpty(conversationId);
        }

        async Task<string> GetConversationID(string sender, string recipient)
        {
            //if the conversation does not exists create a new
            var conversationId = await _dataManager.CheckConversationAlreadyExists(sender, recipient);

            return conversationId;
        }

        /// <summary>
        /// Create conversation thread is for the two people if it doesnt exist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <returns></returns>
        async Task<string> CreateConversationThread(string sender, string recipient)
        {
            var convThread = new Conversation
            {
                firstMessageDate = DateTime.UtcNow,
                lastMessageDate = DateTime.UtcNow,
                receipientUserID = recipient,
                senderUserID = sender
            };

            var conversationId = await _dataManager.CreateConversationThread(convThread);

            return conversationId;
        }
    }
}