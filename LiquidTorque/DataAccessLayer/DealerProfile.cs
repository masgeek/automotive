using Android.Widget;

namespace liquidtorque.DataAccessLayer
{
	public class DealerProfile
	{
		public UserProfile userProfile { get; set; }
		public string objectId { get; set; }
		public string companyName { get; set; }
		public string companyAddress { get; set; }
		public string companyCity { get; set; }
		public string userName { get; set; }
		public string country { get; set; }
		public string zipCode { get; set; }

        public ImageView profilePicture { get; set; }

		public string officerFirstName { get; set; }
		public string officerLastName { get; set; }
		public string dealerLicense { get; set; }
	}
}

