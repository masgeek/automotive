namespace liquidtorque.DataAccessLayer
{
	public class OwnerProfile
	{
		public static bool isOwner{ get; set;}

		public static string objectId { get; set; }
		public static string userType { get; set;}
		public static string email { get; set; }
		public static string username { get; set; }

		public static string firstName { get; set; }
		public static string lastName { get; set; }
        //@FIX to contain android image element
        public static object profilePicture { get; set; }

		public static string address { get; set; }
		public static string zipCode { get; set;}
		public static string postalCode { get; set;}
		public static string city { get; set; }
		public static string state { get; set; }
		public static string country { get; set; }
		public static string phone { get; set; }
		public static string phoneVerified { get; set; }
		public static string facebookUsername { get; set; }
		public static string linkedinUsername { get; set; }
		public static string instagramUsername { get; set; }
		public static string twitterUsername { get; set; }
		public static bool isBlocked { get; set; }
		public static string macAddress { get; set; }
		public static string ipAddress { get; set; }
		public static string currentUserSMSVerificationCode { get; set; }
		public static string currentUserEmailVerificationCode { get; set; }
	}
}
