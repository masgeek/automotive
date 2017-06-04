using System.Collections.Generic;

namespace DataAccessLayer
{
	public class MessageInbox
	{
		List<Conversation> conversationList;

		public MessageInbox ()
		{
			conversationList = new List<Conversation> ();
		}

		public void addConversation(Conversation conversation)
		{
			conversationList.Add (conversation);
		}

		public Conversation getConversation(string conversationId)
		{
			Conversation conversation = new Conversation ();
			foreach (Conversation conv in conversationList) {

				if (conv.objectId.Equals (conversationId)) {
					conversation = conv;
				}
			}
			return conversation;
		}

		public int getNumberOfConversation()
		{
			return conversationList.Count;
		}

		public void ClearConversation()
		{
			conversationList.Clear ();
		}

		public void deleteConversation(string conversationId)
		{
			int i = 0;
			foreach (Conversation conv in conversationList) {
				
				if (conv.objectId.Equals (conversationId)) {
					conversationList.RemoveAt (i);
					break;
				}
				i++;
			}

		}
		public List<Conversation> conversation
		{
			get { return conversationList; }
		}
	}
}

