using System;
using System.Collections.Generic;
using Android.Graphics;

namespace liquidtorque.DataAccessLayer
{
	public class UserProfile
	{
		public string objectId { get; set; }
		public string userType { get; set;}
		public string email { get; set; }
		public string username { get; set; }
		public string password { get; set; }
		public string firstName { get; set; }
		public string lastName { get; set; }
        //@FIX to contain android image element
        public Bitmap profilePicture { get; set; }
        public bool emailVerified { get; set; }
		public string address { get; set; }
		public string zipCode { get; set;}
		public string postalCode { get; set;}
		public string city { get; set; }
		public string state { get; set; }
		public string country { get; set; }
		public string phone { get; set; }
		public string phoneVerified { get; set; }
		public string facebookUsername { get; set; }
		public string linkedinUsername { get; set; }
		public string instagramUsername { get; set; }
		public string twitterUsername { get; set; }
		public bool isBlocked { get; set; }
		public string macAddress { get; set; }
		public string ipAddress { get; set; }
		public string currentUserSMSVerificationCode { get; set; }
		public string currentUserEmailVerificationCode { get; set; }

		public DateTime? appAccessStartDate { get; set; }
		public DateTime? appAccessEndDate { get; set; }


		public List<Subscription> Subscriptions{ get; set;}

		public void AddSubscription(Subscription s)
		{
			if (Subscriptions == null) {
				Subscriptions = new List<Subscription> ();

			}
			Subscriptions.Add (s);
		}
	}
}

