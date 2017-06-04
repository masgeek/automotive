using System;

namespace DataAccessLayer
{
	public class ChatMessage
	{
        public string conversationID { get; set; }
        public string messageID { get; set; }
		public string senderUserID { get; set; }
		public string receipientUserID { get; set; }
		public string message { get; set; }
		public bool hasBeenRead { get; set; }
		public DateTime sendDate { get; set; }
		public DateTime readDate { get; set; }
		public string conversationRowID { get; set; }
		public bool rightBubble { get; set; }
		public bool leftBubble { get; set; }
	}
}

