using System.Collections.Generic;

namespace DataAccessLayer
{
	public class RequestsInbox
	{
		private List<UserRequest> requestsList;

		public RequestsInbox ()
		{
			requestsList = new List<UserRequest> ();
		}

		public List<UserRequest> getNumberOfRequests()
		{
			return requestsList;

		}
		public void addRequest(UserRequest request)
		{
			requestsList.Add (request);
		}

		public void ClearRequests()
		{
			requestsList.Clear ();
		}

		public void removeRequest(string objectId)
		{

		}

		public List<UserRequest> requests 
		{
			get{ return requestsList;}

		}
	}
}

