using System;
using System.Collections.Generic;
namespace DataAccessLayer
{
	public class Conversation
	{

		public string objectId { get; set; }
		public string senderUserID { get; set; }
		public string receipientUserID { get; set; }
		public DateTime firstMessageDate { get; set; }
		public DateTime lastMessageDate { get; set; }
        
		public List<ChatMessage> senderMessages;
		public List<ChatMessage> receipientMessages;
		public Conversation ()
		{
			senderMessages = new List<ChatMessage> ();
			receipientMessages = new List<ChatMessage> ();
		}
	}
}

