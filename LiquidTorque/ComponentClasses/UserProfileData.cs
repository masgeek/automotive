using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HockeyApp;
using Parse;

namespace liquidtorque.ComponentClasses
{
    public class UserProfileData
    {
        private Dictionary<string,object> profileData;
        private Dictionary<string,object> dealerProfileData;
        public static UserProfileData userProfile;
        private static object myObject = new object();
        public static UserProfileData GetInstance()
        {
            lock (myObject)
            {
                if (userProfile == null)
                {
                    userProfile = new UserProfileData();

                }
                return userProfile;
            }
        }
        public UserProfileData()
        {
            profileData = new Dictionary<string, object>();
            dealerProfileData = new Dictionary<string, object>();
        }

        public async Task<Dictionary<string, object>> FetchUserProfileData(string userName)
        {
            try
            {
                IEnumerable<ParseObject> results = null;
                var query = ParseUser.Query.WhereEqualTo("username", userName);

                if (profileData.Count > 0)
                {
                    profileData.Clear();
                }
                results = await query.FindAsync();
                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        profileData.Add("objectid", parseObject.ObjectId);

                        if (parseObject.ContainsKey("firstName"))
                        {
                            var firstName = parseObject.Get<string>("firstName") ?? "N/A";
                            profileData.Add("firstname", firstName);
                        }

                        if (parseObject.ContainsKey("lastName"))
                        {
                            var lastName = parseObject.Get<string>("lastName") ?? "N/A";
                            profileData.Add("lastname", lastName);
                        }

                        if (parseObject.ContainsKey("email"))
                        {
                            var email = parseObject.Get<string>("email") ?? "N/A";
                            profileData.Add("email", email);
                        }

                        if (parseObject.ContainsKey("phone"))
                        {
                            var phone = parseObject.Get<string>("phone") ?? "N/A";
                            profileData.Add("phone", phone);
                        }

                        
                        if (parseObject.ContainsKey("address"))
                        {
                            var address = parseObject.Get<string>("address") ?? "N/A";
                            profileData.Add("address", address);
                        }

                        if (parseObject.ContainsKey("city"))
                        {

                            var city = parseObject.Get<string>("city") ?? "N/A";
                            profileData.Add("city", city);
                        }

                        if (parseObject.ContainsKey("country"))
                        {
                            var country = parseObject.Get<string>("country") ?? "N/A";
                            profileData.Add("country", country);

                        }


                        if (parseObject.ContainsKey("isBlocked"))
                        {
                            var isBlocked = parseObject.Get<bool>("isBlocked");
                            profileData.Add("blocked", isBlocked);
                        }


                        if (parseObject.ContainsKey("facebookUsername"))
                        {
                            var facebookUsername = parseObject.Get<string>("facebookUsername") ?? "N/A";
                            profileData.Add("facebook", facebookUsername);
                        }


                        if (parseObject.ContainsKey("instagramUsername"))
                        {
                            var instagramUsername = parseObject.Get<string>("instagramUsername") ?? "N/A";
                            profileData.Add("instagram", instagramUsername);
                        }


                        if (parseObject.ContainsKey("twitterUsername"))
                        {
                            var twitterUsername = parseObject.Get<string>("twitterUsername") ?? "N/A";
                            profileData.Add("twitter", twitterUsername);
                        }

                        if (parseObject.ContainsKey("linkedinUsername"))
                        {
                            var linkedinUsername = parseObject.Get<string>("linkedinUsername") ?? "N/A";
                            profileData.Add("linkedin", linkedinUsername);

                        }

                        if (parseObject.ContainsKey("ipAddress"))
                        {
                            var ipAddress = parseObject.Get<string>("ipAddress") ?? "N/A";
                            profileData.Add("ip", ipAddress);

                        }

                        if (parseObject.ContainsKey("macAddress"))
                        {
                            var macAddress = parseObject.Get<string>("macAddress") ?? "N/A";
                            profileData.Add("mac", macAddress);

                        }

                        if (parseObject.ContainsKey("zipCode"))
                        {
                            var zipCode = parseObject.Get<string>("zipCode") ?? "N/A";
                            profileData.Add("zipcode", zipCode);
                        }


                        if (parseObject.ContainsKey("state"))
                        {
                            var state = parseObject.Get<string>("state") ?? "N/A";
                            profileData.Add("state", state);
                        }



                        if (parseObject.ContainsKey("userType"))
                        {
                            var userType = parseObject.Get<string>("userType") ?? "N/A";
                            profileData.Add("usertype", userType);
                        }

                        if (parseObject.ContainsKey("appAccessStartDate"))
                        {
                            var appAccessStartDate = parseObject.Get<DateTime?>("appAccessStartDate");
                            profileData.Add("accesstartdate", appAccessStartDate);
                        }

                        if (parseObject.ContainsKey("appAccessEndDate"))
                        {
                            var appAccessEndDate = parseObject.Get<DateTime?>("appAccessEndDate");
                            profileData.Add("accessenddate", appAccessEndDate);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User profile setting error " + ex.Message + ex.StackTrace);
            }

            return profileData;
        }

        public async Task<Dictionary<string, object>> FetchDealerProfile(string userName)
        {
            try
            {
                IEnumerable<ParseObject> results = null;

                if (dealerProfileData.Count > 0)
                {
                    dealerProfileData.Clear();
                }
                var query = ParseObject.GetQuery("DealerProfile")
                    .WhereEqualTo("username", userName);

                results = await query.FindAsync();
                if (results != null)
                {
                    //populate the fields
                    /*
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
                        public ImageView dealerLicense { get; set; }
                    */
                    foreach (var parseObject in results)
                    {


                        if (parseObject.ContainsKey("username"))
                        {
                            var username = parseObject.Get<string>("username") ?? "N/A";
                            dealerProfileData.Add("username", username);
                        }
                        if (parseObject.ContainsKey("officerFirstname"))
                        {
                            var firstname = parseObject.Get<string>("officerFirstname") ?? "N/A";
                            dealerProfileData.Add("firstname", firstname);
                        }
                        if (parseObject.ContainsKey("officerLastname"))
                        {
                            var lastname = parseObject.Get<string>("officerLastname") ?? "N/A";
                            dealerProfileData.Add("lastname", lastname);
                        }

                        if (parseObject.ContainsKey("companyName"))
                        {
                            var company = parseObject.Get<string>("companyName") ?? "N/A";
                            dealerProfileData.Add("company", company);
                        }
                        if (parseObject.ContainsKey("dealerLicense"))
                        {
                            var license = parseObject.Get<Uri>("dealerLicense");
                            dealerProfileData.Add("license", license);
                        }
                        if (parseObject.ContainsKey("city"))
                        {
                            var city = parseObject.Get<string>("city") ?? "N/A";
                            dealerProfileData.Add("city", city);
                        }
                        if (parseObject.ContainsKey("zipCode"))
                        {
                            var zipcode = parseObject.Get<string>("zipCode") ?? "N/A";
                            dealerProfileData.Add("zipcode", zipcode);
                        }
                        if (parseObject.ContainsKey("country"))
                        {
                            var country = parseObject.Get<string>("country") ?? "N/A";
                            dealerProfileData.Add("country", country);
                        }
                        if (parseObject.ContainsKey("companyAddress"))
                        {
                            var address = parseObject.Get<string>("companyAddress") ?? "N/A";
                            dealerProfileData.Add("address", address);
                        }

                        //end the foreach loop
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error fetching dealer profile {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent(string.Format("Error fetching dealer profile {0} {1}", ex.Message,
                    ex.StackTrace));
            }

            return dealerProfileData;
        }
    }
}
 