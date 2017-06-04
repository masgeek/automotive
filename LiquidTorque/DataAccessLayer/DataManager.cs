using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using DataAccessLayer;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.OffLineData;
using Parse;
using Console = System.Console;

namespace liquidtorque.DataAccessLayer
{
    public class DataManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public enum userTypes
        {
            PrivateParty,
            Dealer
        };

        private static DataManager dataManager;
        private VehicleDataCache vCache;
        private MessageBus messageBus;
        private SqlLiteDataStore offlineData;
        private GeoLocation geoLocation;
        private static object myObject = new object();
        private ParseStorage parseStorage;
        public UserProfile userProfile;
        public DealerProfile dealerProfile;
        public UserInventory inventory;
        public RequestsInbox requestsInbox;
        public MessageInbox messageInbox;
        public bool inboxRefreshed = false;
        public Car car;
        private VehicleData vehicleData;

        public bool successLogin { get; set; }
        public bool usernameExists { get; set; }
        public bool emailExists { get; set; }
        public bool companyExists { get; set; }
        public bool addressExists { get; set; }
        public bool countryExists { get; set; }
        public string currentUser { get; set; }
        public string companyName { get; set; }
        public string accountType { get; set; }

        /// <summary>
        /// This flags if the car data has been saved and prevents multiple entries of the same vehicle
        /// default value is false;
        /// </summary>
        public bool CarIsListed { get; set; }

        public InAppPurchaseManager PurchaseManager { get; set; }


        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool _purchaseComplete;

        public bool purchaseComplete
        {
            get { return _purchaseComplete; }
            set
            {
                _purchaseComplete = value;
                NotifyPropertyChanged();
            }
        }


        private DataManager()
        {
            try
            {
                parseStorage = new ParseStorage();
                geoLocation = new GeoLocation();
                offlineData = SqlLiteDataStore.GetInstance();
                vCache = VehicleDataCache.GetInstance();
                messageBus = new MessageBus();
                vehicleData = new VehicleData();
                userProfile = new UserProfile();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting mesage bus " + ex.Message + ex.StackTrace);
            }
        }

        public void setPurchaseManager(InAppPurchaseManager inAppPurchaseManager)
        {
            this.PurchaseManager = inAppPurchaseManager;
            //PurchaseManager.SimulateiTunesAppStore = false;
            //PurchaseManager.PublicKey = DataUtils.purchaseManagerKey;
        }

        public static DataManager GetInstance()
        {
            lock (myObject)
            {
                if (dataManager == null)
                {
                    dataManager = new DataManager();

                }
                return dataManager;
            }
        }

        public async Task<bool> LoginUser(string userName, string password)
        {
            try
            {
                //await parseStorage.LoginUser(userName, password);
                await ParseUser.LogInAsync(userName, password);
                //successLogin = parseStorage.isLoginSuccessful;

                //instantiate the other classes
                userProfile = new UserProfile();
                dealerProfile = new DealerProfile();
                inventory = new UserInventory();
                requestsInbox = new RequestsInbox();
                messageInbox = new MessageInbox();
                vehicleData = new VehicleData();
                return true;
            }
            catch (ParseException ex)
            {
                Console.WriteLine("Login Unsuccessful" + ex.Message + ex.StackTrace);
            }
            catch (Exception gEx)
            {
                Console.WriteLine("Login Unsuccessful" + gEx.Message + gEx.StackTrace);
            }
            return false;
        }

        public void logoutUser()
        {
            try
            {
                this.userProfile = null;
                this.dealerProfile = null;
                this.currentUser = "";
                ParseUser.LogOut();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        public async Task<bool> signUpUser(UserProfile profile)
        {
            try
            {

                ParseUser parseObject = new ParseUser();

                parseObject.Username = profile.username;
                parseObject.Email = profile.email;
                parseObject.Password = profile.password;
                parseObject["userType"] = profile.userType;
                parseObject["phone"] = profile.phone;
                //parseObject["facebookUsername"] = profile.facebookUsername;
                //				parseObject["linkedinUsername"] = profile.linkedinUsername;
                //				parseObject["instagramUsername"] = profile.instagramUsername;
                //				parseObject["twitterUsername"] = profile.twitterUsername;
                parseObject["firstName"] = profile.firstName;
                parseObject["lastName"] = profile.lastName;
                //parseObject["address"] = profile.address;
                parseObject["zipCode"] = profile.zipCode;

                var geoData = await geoLocation.GetCityFromCode(profile.country, profile.zipCode);

                if (geoData.Count != 0)
                {
                    parseObject["city"] = geoData["cityName"];
                    parseObject["state"] = geoData["countyName"];
                }
                //parseObject["postalCode"] = profile.postalCode;
                //parseObject["city"] = profile.city;
                parseObject["country"] = profile.country;
                //parseObject["state"] = profile.state;
                //parseObject["macAddress"] = profile.macAddress;
                //parseObject["ipAddress"] = profile.ipAddress;
                //parseStorage.SignUpUser(parseObject);
                await parseObject.SignUpAsync();

                return true;
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

            return false;
        }

        /// <summary>
        /// Sign up a private party profile
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public async Task<bool> signUpPrivateParty(UserProfile profile)
        {
            try
            {
                ParseUser parseObject = new ParseUser();

                parseObject.Username = profile.username;
                parseObject.Email = profile.email;
                parseObject.Password = profile.password;
                parseObject["userType"] = profile.userType;
                parseObject["phone"] = profile.phone;
                //parseObject["facebookUsername"] = profile.facebookUsername;
                //				parseObject["linkedinUsername"] = profile.linkedinUsername;
                //				parseObject["instagramUsername"] = profile.instagramUsername;
                //				parseObject["twitterUsername"] = profile.twitterUsername;
                parseObject["firstName"] = profile.firstName;
                parseObject["lastName"] = profile.lastName;
                //parseObject["address"] = profile.address;
                parseObject["zipCode"] = profile.zipCode;

                var geoData = await geoLocation.GetCityFromCode(profile.country, profile.zipCode);

                if (geoData.Count != 0)
                {
                    parseObject["city"] = geoData["cityName"];
                    parseObject["state"] = geoData["countyName"];
                }
                //parseObject["postalCode"] = profile.postalCode;
                //parseObject["city"] = profile.city;
                parseObject["country"] = profile.country;
                //parseObject["state"] = profile.state;
                //parseObject["macAddress"] = profile.macAddress;
                //parseObject["ipAddress"] = profile.ipAddress;
                //parseStorage.SignUpUser(parseObject);
                await parseObject.SignUpAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

            return false;
        }


        /// <summary>
        /// Sign up a dealer profile
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public async Task<bool> signUpDealer(DealerProfile profile,string licensePath)
        {
            //@TODO consider returning a message of what actually happened to the user, not just a boolean value
            //@FIX Very urgent
            try
            {
                var h = profile.userProfile;
                var signupResp = await signUpUser(profile.userProfile);
                if (signupResp)
                {
                    Console.WriteLine("User signup status " + signupResp);


                    ParseObject parseObject = new ParseObject("DealerProfile");
                    parseObject["companyName"] = profile.companyName;
                    parseObject["companyAddress"] = profile.companyAddress;
                    parseObject["officerFirstname"] = profile.officerFirstName;
                    parseObject["officerLastname"] = profile.officerLastName;
                    parseObject["country"] = profile.country;
                    parseObject["zipCode"] = profile.zipCode;
                    parseObject["username"] = profile.userName;

                    //let us get the city based on the zip code
                    var geoData = await geoLocation.GetCityFromCode(profile.country, profile.zipCode);

                    if (geoData.Count != 0)
                    {
                        parseObject["city"] = geoData["cityName"];
                        parseObject["state"] = geoData["countyName"];
                    }

                    if (licensePath != null)
                    {
                        try
                        {
                            var dealerLicenseBitmap = HelperClass.GetImageBitmapFromUrl(licensePath);
                            //first let us save the license image first
                            var savedImage = await SaveDealerLicense(dealerLicenseBitmap, profile.userName);
                            if (savedImage != null)
                            {
                                parseObject["dealerLicense"] = savedImage;
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = "Error saving dealer license" + ex.Message + ex.StackTrace;
                            Console.WriteLine(message);
                            MetricsManager.TrackEvent(message);
                        }
                    }
                    await parseObject.SaveAsync();
                    return true;
                }

            }
            catch (ParseException ex)
            {
                Console.WriteLine("Dealer sign up issues " + ex.Message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dealer signup issues " + ex.Message + ex.StackTrace);
            }
            return false;
        }

        public async Task<bool> checkUsernameAlreadyExists(string username)
        {
            string matchingUser = null; //this will contain the username if if exists

            try
            {
                var userQuery = await (from user in ParseUser.Query
                    where user.Get<string>("username") == username
                    select user).FindAsync();



                if (userQuery != null)
                {
                    foreach (var user in userQuery)
                    {
                        matchingUser = user.Username;
                    }
                }

            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception gEx)
            {
                Console.WriteLine(gEx.Message + gEx.StackTrace);
            }

            //check if we have an actual result from the query object returned
            usernameExists = !string.IsNullOrEmpty(matchingUser);

            return usernameExists;
        }

        public async Task<bool> checkEmailAlreadyExists(string email)
        {
            string matchingEmail = null; //this will contain the email if it exists

            try
            {

                var emailQuery = await (from user in ParseUser.Query
                    where user.Get<string>("email") == email
                    select user).FindAsync();

                if (emailQuery != null)
                {
                    foreach (var user in emailQuery)
                    {
                        matchingEmail = user.Email;
                    }
                }

            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception gEx)
            {
                Console.WriteLine(gEx.Message + gEx.StackTrace);
            }

            //check if we have an actual result from the query object returned
            emailExists = !string.IsNullOrEmpty(matchingEmail);

            return emailExists;
        }

        public async Task<bool> checkCompanyNameAlreadyExists(string company)
        {
            string matchingCompany = null; //this will contain the email if it exists

            try
            {
                var query = ParseObject.GetQuery("DealerProfile").WhereEqualTo("companyName", company);

                IEnumerable<ParseObject> results = await query.FindAsync();


                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        matchingCompany = parseObject.Get<string>("companyName");
                    }
                }

            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception gEx)
            {
                Console.WriteLine(gEx.Message + gEx.StackTrace);
            }

            //check if we have an actual result from the query object returned
            if (!string.IsNullOrEmpty(matchingCompany))
            {

                companyExists = true;
            }
            else
            {
                companyExists = false;

            }

            return companyExists;
        }

        public void resetPassword(string userEmail)
        {
            try
            {
                parseStorage.ResetPassword(userEmail);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }



        public async Task insertUserRequest(UserRequest request)
        {
            try
            {
                ParseObject parseObject = new ParseObject("Requests");
                parseObject["make"] = request.make;
                parseObject["model"] = request.model;
                parseObject["year"] = request.year;
                parseObject["username"] = request.userName;
                //parseStorage.SaveParseObject("Requests", parseObject);
                await parseObject.SaveAsync();
            }
            catch (ParseException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Update a users request
        /// </summary>
        /// <param name="request">UserRequest User Request class</param>
        public async Task updateUserRequest(UserRequest request)
        {
            try
            {
                var updateObject = ParseObject.CreateWithoutData("Requests", request.objectId);

                updateObject["make"] = request.make;
                updateObject["model"] = request.model;
                updateObject["year"] = request.year;
                updateObject["username"] = request.userName;
                //parseStorage.SaveParseObject("Requests", updateObject);
                await updateObject.SaveAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }
        }

        public async Task DeleteUserRequest(string objectId)
        {
            try
            {

                var query = ParseObject.GetQuery("Requests").WhereEqualTo("objectId", objectId);

                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        await parseObject.DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        public void saveUserProfile(UserProfile profile)
        {
            try
            {
                ParseUser parseObject = new ParseUser();
                parseObject["username"] = profile.username;
                parseObject["email"] = profile.email;
                parseObject["phone"] = profile.phone;
                parseObject["userType"] = profile.userType;
                parseObject["facebookUsername"] = profile.facebookUsername;
                parseObject["linkedinUsername"] = profile.linkedinUsername;
                parseObject["instagramUsername"] = profile.instagramUsername;
                parseObject["twitterUsername"] = profile.twitterUsername;
                parseObject["firstName"] = profile.firstName;
                parseObject["lastName"] = profile.lastName;
                parseObject["address"] = profile.address;
                parseObject["zipCode"] = profile.zipCode;
                parseObject["postalCode"] = profile.postalCode;
                parseObject["city"] = profile.city;
                parseObject["country"] = profile.country;
                parseObject["password"] = profile.password;
                parseObject["state"] = profile.state;
                parseObject["macAddress"] = profile.macAddress;
                parseObject["ipAddress"] = profile.ipAddress;


                parseStorage.SaveParseUser("User", parseObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Update the User profile and also optionally social media links
        /// </summary>
        /// <param name="profile">User profile data class</param>
        /// <param name="updateSocialOnly">boolean This is an optional parameter whose flag is set only ifrom the settings page</param>
        /// <returns>It is a void function</returns>
        public async Task updateUserProfile(UserProfile profile, bool updateSocialOnly = false)
        {
            try
            {
                ParseUser loggedInUser = ParseUser.CurrentUser; //get the currently logged in user

                if (updateSocialOnly)
                {
                    loggedInUser["facebookUsername"] = profile.facebookUsername;
                    loggedInUser["linkedinUsername"] = profile.linkedinUsername;
                    loggedInUser["instagramUsername"] = profile.instagramUsername;
                    loggedInUser["twitterUsername"] = profile.twitterUsername;
                }
                else
                {
                    //loggedInUser["objectId"] = profile.objectId;
                    //loggedInUser["username"] = profile.username;
                    loggedInUser["email"] = profile.email;
                    loggedInUser["phone"] = profile.phone;
                    //parseObject["userType"] = profile.userType;

                    //we will need to check the section where the function is being called, 
                    //if it is updating social media lets perfom only data t0 prevent accidental data overrides with blank values

                    loggedInUser["firstName"] = profile.firstName;
                    loggedInUser["lastName"] = profile.lastName;
                    loggedInUser["address"] = profile.address;
                    loggedInUser["zipCode"] = profile.zipCode;
                    loggedInUser["postalCode"] = profile.postalCode;

                    loggedInUser["city"] = profile.city;
                    loggedInUser["country"] = profile.country;
                    /*
                     * only look for city during registration
                     * the user is free to change it
                    var geoData = await geoLocation.GetCityFromCode(profile.country, profile.zipCode);

                    if (geoData.Count != 0)
                    {
                        loggedInUser["city"] = geoData["cityName"];
                        loggedInUser["state"] = geoData["countyName"];
                    }
                     * */
                    //parseObject["password"] = profile.password;
                    //parseObject["state"] = profile.state;
                    //parseObject["macAddress"] = profile.macAddress;
                    //parseObject["ipAddress"] = profile.ipAddress;

                    //next save the profile image of the user
                    var resp = await UpdateProfileImage(profile.profilePicture, loggedInUser.ObjectId);

                    if (resp)
                    {
                        //profile image saving was successfull
                    }
                }
                await loggedInUser.SaveAsync();
                //parseStorage.saveParseUser ("User", parseObject);
                await ParseUser.CurrentUser.FetchAsync(); //refresh teh user cache from the database

            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception gEx)
            {
                Console.WriteLine(gEx.Message + gEx.StackTrace);
            }
        }

        /// <summary>
        /// Save the users profile image
        /// </summary>
        /// <param name="image">UImage</param>
        /// <param name="userObjectId">String</param>
        /// <returns>True|False</returns>
        public async Task<bool> UpdateProfileImage(Bitmap image, string userObjectId)
        {
            //@TODO pass a bitmap instad of an image view
            try
            {
                Guid guid = Guid.NewGuid(); //generate uniqe guid to prevent file conflicts
                byte[] imageData;

                string fileName = userObjectId + guid + ".png";

                //first check if the image object is null
                if (image == null) return false; //end execution if image is null;
                //if not null check if the image exists in the parse tables
                //we will return the object id
                var imageObjectId = await CheckUserProfileImage(ParseUser.CurrentUser.Username);

                if (imageObjectId == null)
                {
                    //image does not exist
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Compress(Bitmap.CompressFormat.Png, 100, stream);
                        imageData = stream.ToArray();
                        image.Recycle(); //clear the data from memory
                    }


                    ParseFile imgfile = new ParseFile(fileName, imageData);


                    await imgfile.SaveAsync(new Progress<ParseUploadProgressEventArgs>(
                        e =>
                        {
                            var progress = (int) (e.Progress*100);
                            Console.WriteLine(String.Format("Upload progress {0}", progress));
                        }));

                    var imageObject = new ParseObject("ProfileImages");
                    imageObject["image"] = imgfile;
                    imageObject["username"] = ParseUser.CurrentUser.Username;
                    //save the user pointer id
                    imageObject.Add("userObjectID", ParseUser.CurrentUser);

                    await imageObject.SaveAsync();
                }
                else //image already exists update the record
                {
                    //imageData = new byte[image.Width*image.Height*4];
                    //MemoryStream stream = new MemoryStream(imageData);
                    // image.Compress(Bitmap.CompressFormat.Png, 100, stream);
                    //flush the strem
                    //stream.Flush();

                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Compress(Bitmap.CompressFormat.Png, 100, stream);
                        imageData = stream.ToArray();
                        image.Recycle(); //clear the data from memory
                    }

                    ParseFile updateImgFile = new ParseFile(fileName, imageData);

                    await updateImgFile.SaveAsync(new Progress<ParseUploadProgressEventArgs>(
                        e =>
                        {
                            var progress = (int) (e.Progress*100);
                            Console.WriteLine(String.Format("Upload progress {0}", progress));
                        }));

                    var updateObject = ParseObject.CreateWithoutData("ProfileImages", imageObjectId);
                    updateObject["image"] = updateImgFile;

                    await updateObject.SaveAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            return false;
        }

        public async Task<string> CheckUserProfileImage(string username)
        {
            try
            {
                var userImageQuery = ParseObject.GetQuery("ProfileImages")
                    .WhereEqualTo("username", username);

                var profileData = await userImageQuery.FirstAsync();

                return profileData.ObjectId;
            }
            catch (Exception ex)
            {
                //error occured
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Fetch a user's profile image
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<Uri> FetchUserProfileImage(string username)
        {
            Uri imgUrl = null;
            try
            {

                var userImageQuery = ParseObject.GetQuery("ProfileImages")
                    .WhereEqualTo("username", username);

                var profileData = await userImageQuery.FirstAsync();
                var imageFile = profileData.Get<ParseFile>("image");
                imgUrl = imageFile.Url; //get the absolute path

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return imgUrl;
        }

        public void saveUserSettings(UserSettings settings)
        {
            try
            {
                ParseObject parseObject = new ParseObject("UserSettings");
                parseStorage.SaveParseObject("UserSettings", parseObject);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public UserProfile getUserProfile()
        {
            return this.userProfile;
        }

        //function to get the owner profile
        public async Task<IEnumerable<ParseUser>> GetOwnerProfile(string ownerName)
        {
            try
            {
                var ownerQuery = ParseUser.Query.WhereEqualTo("username", ownerName);

                var ownerData = await ownerQuery.FindAsync();

                return ownerData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public void setFacebookUserName(string username)
        {

            this.userProfile.facebookUsername = username;
        }

        public void setInstagramUserName(string username)
        {

            this.userProfile.instagramUsername = username;
        }

        public void setTwitterUserName(string username)
        {
            this.userProfile.twitterUsername = username;

        }

        public void setLinkedInUserName(string username)
        {

            this.userProfile.linkedinUsername = username;
        }

        public async void setUserProfile(string userName)
        {
            try
            {
                IEnumerable<ParseObject> results = null;
                var query = ParseUser.Query.WhereEqualTo("username", userName);

                results = await query.FindAsync();
                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {

                        this.userProfile.objectId = parseObject.ObjectId;

                        if (parseObject.ContainsKey("firstName"))
                        {
                            this.userProfile.firstName = parseObject.Get<string>("firstName");
                        }

                        if (parseObject.ContainsKey("lastName"))
                        {
                            this.userProfile.lastName = parseObject.Get<string>("lastName");
                        }

                        if (parseObject.ContainsKey("email"))
                        {
                            this.userProfile.email = parseObject.Get<string>("email");
                        }

                        if (parseObject.ContainsKey("phone"))
                        {
                            this.userProfile.phone = parseObject.Get<string>("phone");
                        }


                        if (parseObject.ContainsKey("address"))
                        {
                            this.userProfile.address = parseObject.Get<string>("address");
                        }

                        if (parseObject.ContainsKey("city"))
                        {

                            this.userProfile.city = parseObject.Get<string>("city");
                        }

                        if (parseObject.ContainsKey("country"))
                        {
                            this.userProfile.country = parseObject.Get<string>("country");

                        }


                        if (parseObject.ContainsKey("isBlocked"))
                        {
                            this.userProfile.isBlocked = parseObject.Get<bool>("isBlocked");

                        }


                        if (parseObject.ContainsKey("facebookUsername"))
                        {
                            this.userProfile.facebookUsername = parseObject.Get<string>("facebookUsername");

                        }


                        if (parseObject.ContainsKey("instagramUsername"))
                        {
                            this.userProfile.instagramUsername = parseObject.Get<string>("instagramUsername");

                        }


                        if (parseObject.ContainsKey("twitterUsername"))
                        {
                            this.userProfile.twitterUsername = parseObject.Get<string>("twitterUsername");

                        }

                        if (parseObject.ContainsKey("linkedinUsername"))
                        {
                            this.userProfile.linkedinUsername = parseObject.Get<string>("linkedinUsername");

                        }

                        if (parseObject.ContainsKey("ipAddress"))
                        {
                            this.userProfile.ipAddress = parseObject.Get<string>("ipAddress");

                        }

                        if (parseObject.ContainsKey("macAddress"))
                        {
                            this.userProfile.macAddress = parseObject.Get<string>("macAddress");

                        }

                        if (parseObject.ContainsKey("zipCode"))
                        {
                            this.userProfile.zipCode = parseObject.Get<string>("zipCode");

                        }


                        if (parseObject.ContainsKey("password"))
                        {
                            this.userProfile.password = parseObject.Get<string>("password");

                        }

                        if (parseObject.ContainsKey("state"))
                        {
                            this.userProfile.state = parseObject.Get<string>("state");

                        }

                        if (parseObject.ContainsKey("username"))
                        {
                            this.userProfile.username = parseObject.Get<string>("username");

                        }

                        if (parseObject.ContainsKey("userType"))
                        {
                            this.userProfile.userType = parseObject.Get<string>("userType");

                        }

                        if (parseObject.ContainsKey("appAccessStartDate"))
                        {
                            this.userProfile.appAccessStartDate = parseObject.Get<DateTime?>("appAccessStartDate");

                        }

                        if (parseObject.ContainsKey("appAccessEndDate"))
                        {
                            this.userProfile.appAccessEndDate = parseObject.Get<DateTime?>("appAccessEndDate");

                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User profile setting error " + ex.Message + ex.StackTrace);
            }
        }

        public void insertNewInventory(UserInventory inventory)
        {
            try
            {
                ParseObject parseObject = new ParseObject("VehicleProfile");
                parseStorage.SaveParseObject("VehicleProfile", parseObject);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task<string> insertNewCar(Car car)
        {
            try
            {
                if (car != null)
                {
                    ParseObject parseObject = new ParseObject("VehicleProfile");
                    parseObject["model"] = car.model;
                    parseObject["year"] = car.year;
                    parseObject["mileage"] = car.mileage;
                    parseObject["distanceUnit"] = car.milesData;
                    parseObject["carType"] = car.carType;
                    parseObject["condition"] = car.condition;
                    parseObject["driveTrain"] = car.driveTrain;
                    parseObject["engine"] = car.engine;
                    parseObject["exteriorColor"] = car.exteriorColor;
                    parseObject["fuelType"] = car.fuelType;
                    parseObject["interiorColor"] = car.interiorColor;
                    parseObject["listPrice"] = car.listPrice;
                    parseObject["modelVariant"] = car.modelVariant;
                    parseObject["options"] = car.options;
                    parseObject["username"] = car.ownerUsername;
                    parseObject["priceOnRequest"] = car.priceOnRequest;
                    parseObject["status"] = car.status;
                    parseObject["transmission"] = car.transmission;
                    parseObject["vat"] = car.vat;
                    parseObject["vin"] = car.vin;
                    parseObject["description"] = car.description;
                    parseObject["make"] = car.make;


                    await parseObject.SaveAsync();

                    //return the car object id
                    return parseObject.ObjectId;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                return null;
            }
        }

        public async Task<Car> retrieveCar(string carId)
        {
            Car retrieveCar = new Car();

            try
            {

                var query = ParseObject.GetQuery("VehicleProfile").WhereEqualTo("objectId", carId);

                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        retrieveCar.objectId = parseObject.ObjectId;
                        retrieveCar.make = parseObject.Get<string>("make");
                        retrieveCar.model = parseObject.Get<string>("model");
                        retrieveCar.year = parseObject.Get<string>("year"); // string in table
                        retrieveCar.mileage = parseObject.Get<int>("mileage");
                        retrieveCar.carType = parseObject.Get<string>("carType");
                        //car.condition = parseObject.Get<string> ("condition"); // not on UI
                        retrieveCar.driveTrain = parseObject.Get<string>("driveTrain");
                        retrieveCar.engine = parseObject.Get<string>("engine");
                        retrieveCar.exteriorColor = parseObject.Get<string>("exteriorColor");
                        //car.fuelType = parseObject.Get<string> ("fuelType"); // not on UI
                        retrieveCar.interiorColor = parseObject.Get<string>("interiorColor");
                        retrieveCar.listPrice = parseObject.Get<string>("listPrice"); // price in table
                        retrieveCar.modelVariant = parseObject.Get<string>("modelVariant");
                        //car.options = parseObject.Get<string> ("options"); // not on UI
                        retrieveCar.ownerUsername = parseObject.Get<string>("username"); // username in table
                        retrieveCar.priceOnRequest = parseObject.Get<bool>("priceOnRequest");
                        //car.status = parseObject.Get<string> ("status"); // not on UI
                        retrieveCar.transmission = parseObject.Get<string>("transmission");
                        retrieveCar.vat = parseObject.Get<bool>("vat");
                        retrieveCar.vin = parseObject.Get<string>("vin");
                    }
                }

            }
            catch (ParseException e)
            {
                Console.WriteLine(e.Message + e.StackTrace);

            }

            return retrieveCar;

        }

        public async Task deleteCar(string objectId)
        {
            try
            {

                var query = ParseObject.GetQuery("VehicleProfile").WhereEqualTo("objectId", objectId);

                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        await parseObject.DeleteAsync();
                    }
                }

            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task<bool> updateCar(Car updateCar)
        {
            try
            {
                var parseObject = ParseObject.CreateWithoutData("VehicleProfile", updateCar.objectId);


                parseObject["make"] = updateCar.make;
                parseObject["model"] = updateCar.model;
                parseObject["year"] = updateCar.year;
                parseObject["mileage"] = updateCar.mileage;
                parseObject["distanceUnit"] = updateCar.milesData;
                parseObject["carType"] = updateCar.carType;
                //parseObject["condition"] = updateCar.condition; // not on UI
                parseObject["driveTrain"] = updateCar.driveTrain;
                parseObject["engine"] = updateCar.engine;
                parseObject["exteriorColor"] = updateCar.exteriorColor;
                //parseObject["fuelType"] = updateCar.fuelType; // not on UI
                parseObject["interiorColor"] = updateCar.interiorColor;
                parseObject["listPrice"] = updateCar.listPrice; // price in table
                parseObject["modelVariant"] = updateCar.modelVariant;
                //parseObject["options"] = updateCar.options;  // not on UI
                parseObject["priceOnRequest"] = updateCar.priceOnRequest;
                //parseObject["status"] = updateCar.status; // not on UI
                parseObject["transmission"] = updateCar.transmission;
                parseObject["vat"] = updateCar.vat;
                parseObject["vin"] = updateCar.vin;
                parseObject["description"] = updateCar.description;

                //parseStorage.saveParseObject("VehicleProfile", parseObject);
                await parseObject.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                var message = string.Format("Error updating vehicle details {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
            return false;

        }

        public async Task saveDealerProfile(DealerProfile dealerProfile)
        {
            try
            {
                //first we will check if the profile already exists
                var b = dealerProfile.userName;
                var profileObjectId = await CheckIfProfileExists(dealerProfile.userName);

                if (string.IsNullOrEmpty(profileObjectId))
                {
                    //this.saveUserProfile(dealerProfile.userProfile);

                    // call update function instead
                    //var resp = updateUserProfile(dealerProfile.userProfile);

                    ParseObject parseObject = new ParseObject("DealerProfile");
                    parseObject["companyName"] = dealerProfile.companyName;
                    parseObject["companyAddress"] = dealerProfile.companyAddress;
                    parseObject["officerFirstname"] = dealerProfile.officerFirstName;
                    parseObject["officerLastname"] = dealerProfile.officerLastName;
                    parseObject["zipCode"] = dealerProfile.zipCode;
                    parseObject["country"] = dealerProfile.country;
                    parseObject["username"] = dealerProfile.userName;

                    await parseObject.SaveAsync();
                }
                else
                {
                    //it exists run an update instead
                    var updateObject = ParseObject.CreateWithoutData("DealerProfile", profileObjectId);
                    //updateObject["image"] = updateImgFile;

                    updateObject["companyName"] = dealerProfile.companyName;
                    updateObject["companyAddress"] = dealerProfile.companyAddress;
                    updateObject["officerFirstname"] = dealerProfile.officerFirstName;
                    updateObject["officerLastname"] = dealerProfile.officerLastName;
                    updateObject["zipCode"] = dealerProfile.zipCode;
                    updateObject["country"] = dealerProfile.country;
                    updateObject["username"] = dealerProfile.userName;

                    await updateObject.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Error updating dealer details {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
        }

        public async Task<string> CheckIfProfileExists(string dealerUserName)
        {
            try
            {
                var checkDealerProfile = ParseObject.GetQuery("DealerProfile")
                    .WhereEqualTo("username", dealerUserName);

                var results = await checkDealerProfile.FirstAsync();

                return results.ObjectId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public async Task<DealerProfile> getDealerProfile(string username)
        {
            dealerProfile = new DealerProfile();


            try
            {

                this.setUserProfile(username);
                dealerProfile.userProfile = this.userProfile;

                var query = ParseObject.GetQuery("DealerProfile").WhereEqualTo("username", username);

                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {

                    foreach (ParseObject parseObject in results)
                    {

                        dealerProfile.objectId = parseObject.ObjectId;
                        dealerProfile.companyName = parseObject.Get<string>("companyName");
                        dealerProfile.companyAddress = parseObject.Get<string>("companyAddress");
                        dealerProfile.companyCity = parseObject.Get<string>("city");

                        dealerProfile.officerFirstName = parseObject.Get<string>("officerFirstname");
                        dealerProfile.officerLastName = parseObject.Get<string>("officerLastname");
                        dealerProfile.country = parseObject.Get<string>("country");
                        dealerProfile.zipCode = parseObject.Get<string>("zipCode");
                        dealerProfile.userName = parseObject.Get<string>("username");
                    }
                }
            }
            catch (ParseException ex)
            {

                Console.WriteLine("Error setting company city " + ex.Message + ex.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return dealerProfile;

        }

        public async Task<DealerProfile> getVehicleOwnerDealerProfile(string username)
        {
            DealerProfile dealerProfile = new DealerProfile();


            try
            {

                //dealerProfile.userProfile = OwnerProfile.;

                var query = ParseObject.GetQuery("DealerProfile").WhereEqualTo("username", username);

                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {

                    foreach (ParseObject parseObject in results)
                    {

                        dealerProfile.objectId = parseObject.ObjectId;
                        dealerProfile.companyName = parseObject.Get<string>("companyName");
                        dealerProfile.companyAddress = parseObject.Get<string>("companyAddress");
                        dealerProfile.companyCity = parseObject.Get<string>("city");
                        dealerProfile.officerFirstName = parseObject.Get<string>("officerFirstname");
                        dealerProfile.officerLastName = parseObject.Get<string>("officerLastname");
                        dealerProfile.country = parseObject.Get<string>("country");
                        dealerProfile.zipCode = parseObject.Get<string>("zipCode");
                        dealerProfile.userName = parseObject.Get<string>("username");
                    }
                }
            }
            catch (ParseException ex)
            {

                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return dealerProfile;

        }

        public async Task deleteUserMessage(string objectId)
        {
            try
            {
                var query = ParseObject.GetQuery("Messages").WhereEqualTo("conversationID", objectId);

                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        await parseObject.DeleteAsync();
                    }
                }
            }
            catch (ParseException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async void SaveUserMessage(ChatMessage userMessage)
        {
            try
            {
                ParseObject parseObject = new ParseObject("Messages");

                parseObject["message"] = userMessage.message;
                parseObject["readDate"] = userMessage.readDate;
                parseObject["sendDate"] = userMessage.sendDate;
                parseObject["senderUserID"] = userMessage.senderUserID;
                parseObject["receipientUserID"] = userMessage.receipientUserID;
                parseObject["conversationID"] = userMessage.conversationID;
                await parseObject.SaveAsync();
            }
            catch (ParseException e)
            {

                Console.WriteLine(e.Message + e.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }
        }


        public async Task<Conversation> retrieveUserMessages(string conversationId, string sender, string receipient)
        {
            Conversation conversation = new Conversation();
            conversation.senderMessages.Clear();

            try
            {
                var query =
                    ParseObject.GetQuery("Messages").WhereEqualTo("conversationID", conversationId).OrderBy("sendDate");
                IEnumerable<ParseObject> results = await query.FindAsync();
                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        ChatMessage message = new ChatMessage();
                        message.messageID = parseObject.ObjectId;
                        message.receipientUserID = parseObject.Get<string>("receipientUserID");
                        message.senderUserID = parseObject.Get<string>("senderUserID");
                        message.readDate = parseObject.Get<DateTime>("readDate");
                        message.message = parseObject.Get<string>("message");
                        //message.hasBeenRead = parseObject.Get<bool>("hasBeenRead");
                        message.sendDate = parseObject.Get<DateTime>("sendDate");

                        //if(message.senderUserID.Equals(sender))
                        //{
                        conversation.senderMessages.Add(message);

                        //}

                        //if(message.receipientUserID.Equals(receipient))
                        //{
                        //	conversation.receipientMessages.Add(message);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return conversation;
        }

        /// <summary>
        /// fetch messgae thread of a particular reciever and sender
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        public async Task<List<ChatMessage>> FetchMessages(string userName,string conversationId)
        {
            //var messageInbox = new List<Tuple<string, string, string, string, string,string,bool,string>>();
            var inbox = new List<ChatMessage>();
            try
            {
                var senderMessageQuery =
                    ParseObject.GetQuery("Messages")
                        //.WhereEqualTo("senderUserID", userName)
                        .WhereEqualTo("conversationID", conversationId)
                        //.WhereEqualTo("receipientUserID", recipient)
                        .OrderBy("updatedAt");
                        //.OrderByDescending("updatedAt");

                var results = await senderMessageQuery.FindAsync();

                foreach (var result in results)
                {

                    var messageId = result.ObjectId;
                    var recipientId = result.Get<string>("receipientUserID");
                    var senderId = result.Get<string>("senderUserID");
                    //var conversationId = result.Get<string>("conversationID");
                    var conversationRowId = result.ContainsKey("conversationRowID") ? result.Get<string>("conversationRowID") : "0";
                    var created = result.CreatedAt;//Get<DateTime>("createdAt");
                    var updated = result.UpdatedAt;//Get<DateTime>("updatedAt");

                    var messageRead = result.ContainsKey("hasBeenRead") && result.Get<bool>("hasBeenRead");
                    var message = result.Get<string>("message");
                   //add the message to the list
                    var messageThread = new ChatMessage
                    {
                        message = message,
                        conversationID = conversationId,
                        conversationRowID = conversationRowId,
                        hasBeenRead = messageRead,
                        readDate = created.Value,
                        sendDate = updated.Value,
                        messageID = messageId,
                        receipientUserID = recipientId,
                        senderUserID = senderId
                    };

                    if (senderId.Equals(userName))
                    {
                        //show on right side of window
                        messageThread.rightBubble = true;
                        messageThread.leftBubble = false;
                    }
                    else
                    {
                        //show on left side of window
                        messageThread.rightBubble = false;
                        messageThread.leftBubble = true;
                    }
                    inbox.Add(messageThread);
                }
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return inbox;
        }

        public async Task<List<ConversationList>> FetchConversation(string userName)
        {
            var recipientList = new List<string>();
            var conversationList = new List<ConversationList>();
            IEnumerable<ParseObject> conversation = new List<ParseObject>();
            try
            {
                var inboxQuery = ParseObject.GetQuery("Conversation")
                        .WhereEqualTo("senderUserID", userName)
                        //.OrderBy("updatedAt");
                        .OrderByDescending("updatedAt");

                var outboxQuery = ParseObject.GetQuery("Conversation")
                       .WhereEqualTo("receipientUserID", userName)
                       //.OrderBy("updatedAt");
                       .OrderByDescending("updatedAt");

                /*
                var lotsOfWins = new Parse.Query("Player");
lotsOfWins.greaterThan(150);

var fewWins = new Parse.Query("Player");
fewWins.lessThan(5);

var mainQuery = Parse.Query.or(lotsOfWins, fewWins);*/

                var inboxResults = await inboxQuery.FindAsync();
                var outboxResults = await outboxQuery.FindAsync();


                //conversation = await outboxQuery.FindAsync();

                //get the inbox results
                foreach (var result in inboxResults)
                {
                    var recipientId = result.Get<string>("receipientUserID");
                    var senderId = result.Get<string>("senderUserID");
                    var updatedAt = result.UpdatedAt; //Get<DateTime>("createdAt");
                    //add the message to the list

                    var conversationItem = new ConversationList
                    {
                        ConversationID = result.ObjectId,
                        SenderId = senderId,
                        RecipientId = recipientId,
                        UpdatedAt = updatedAt.Value
                    };
                    var recipientInList = recipientList.Contains(result.ObjectId);
                    if (!recipientInList) //if its not in the list add it
                    {
                        conversationList.Add(conversationItem);
                        recipientList.Add(result.ObjectId);
                    }
                }

                //Get the outbox results
                foreach (var result in outboxResults)
                {
                    var recipientId = result.Get<string>("receipientUserID");
                    var senderId = result.Get<string>("senderUserID");
                    var updatedAt = result.UpdatedAt; //Get<DateTime>("createdAt");
                    //add the message to the list

                    var conversationItem = new ConversationList
                    {
                        ConversationID = result.ObjectId,
                        SenderId = senderId,
                        RecipientId = recipientId,
                        UpdatedAt = updatedAt.Value
                    };
                    var recipientInList = recipientList.Contains(result.ObjectId);
                    if (!recipientInList) //if its not in the list add it
                    {
                        conversationList.Add(conversationItem);
                        recipientList.Add(result.ObjectId);
                    }
                }


    
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return conversationList;
        }

        /// <summary>
        /// Retreive all cars associated with a particular user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="limit"></param>
        /// <param name="pageIndex"></param>
        public async Task<List<Tuple<string, string, string, string, Uri, string>>> GetUserVehicles(string userName,
            int limit = 20, int pageIndex = 0)
        {
            var vehicleProfile = new List<Tuple<string, string, string, string, Uri, string>>();
            try
            {
                var vehicleProfileData = await vehicleData.GetUserVehicle(userName: userName, limit: limit, pageIndex: pageIndex);

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var profile in vehicleProfileData)
                {

                    string rawPrice = profile.ContainsKey("listPrice") ? profile.Get<string>("listPrice") : "0";
                    var listPrice = HelperClass.CleanUpPrice(rawPrice);
                    var make = profile.ContainsKey("make") ? profile.Get<string>("make") : "N/A";
                    var carModel = profile.ContainsKey("model") ? profile.Get<string>("model") : "N/A";
                    var username = profile.ContainsKey("username") ? profile.Get<string>("username") : "N/A";

                    var imgUrl = await vehicleData.FetchSingleCarImage(profile.ObjectId);
                    if (imgUrl != null)
                    {
                        var vehicleTuple = new Tuple<string, string, string, string, Uri, string>(profile.ObjectId, make,
                            listPrice, carModel, imgUrl, username);
                        vehicleProfile.Add(vehicleTuple);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }


            return vehicleProfile;
        }

        /// <summary>
        /// Fetch user requests from parse
        /// </summary>
        /// <param name="userName">username variable</param>
        /// <returns></returns>
        public async Task<List<Tuple<string, string, string, string>>> GetUserRequests(string userName)
        {
            var requestsList = new List<Tuple<string, string, string, string>>();
            try
            {
                var query = ParseObject.GetQuery("Requests").WhereEqualTo("username", userName);
                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        var objectId = parseObject.ObjectId;
                        var make = parseObject.Get<string>("make");
                        var model = parseObject.Get<string>("model");
                        var year = parseObject.Get<string>("year");

                        //build the tuple first
                        var requestTuple = new Tuple<string, string, string, string>(objectId, make, model, year);
                        //add it to the list
                        requestsList.Add(requestTuple);

                    }
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Unable to fetch user requests {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
            return requestsList;
        }


        /// <summary>
        /// Fetch request details
        /// </summary>
        /// <param name="requestObjectId"></param>
        /// <returns></returns>
        public async Task<List<Tuple<string, string, string, string>>> GetRequestDetails(string requestObjectId)
        {

            var requestDetailList = new List<Tuple<string, string, string, string>>();
            try
            {
                var query = ParseObject.GetQuery("Requests").WhereEqualTo("objectId", requestObjectId);

                IEnumerable<ParseObject> results = await query.FindAsync();

                if (results != null)
                {
                    foreach (ParseObject parseObject in results)
                    {
                        var objectId = parseObject.ObjectId;
                        var make = parseObject.Get<string>("make");
                        var model = parseObject.Get<string>("model");
                        var year = parseObject.Get<string>("year");

                        //build the tuple first
                        var requestTuple = new Tuple<string, string, string, string>(objectId, make, model, year);
                        //add it to the list
                        requestDetailList.Add(requestTuple);
                    }
                }

            }
            catch (Exception ex)
            {
                var message = string.Format("Unable to fetch request details {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

            return requestDetailList;
        }

        /// <summary>
        /// Saves the parse car images.
        /// </summary>
        /// <returns>boolean state of the save actions</returns>
        /// <param name="carImages">Car images List.</param>
        /// <param name="username">Username.</param>
        /// <param name="make">Make.</param>
        /// <param name="model">Model.</param>
        /// <param name="year">Year.</param>
        /// <param name="carObjectId">Car object identifier.</param>
        public void SaveParseImage(List<Bitmap> carImages, string username, string make, string model, string year,
            string carObjectId)
        {
            try
            {
                //loop through the car images
                Console.WriteLine("Images uploading starting , we expect " + carImages.Count);
                Parallel.ForEach(carImages, async img =>
                {
                    //Your stuff
                    Console.WriteLine("Parallel thread {0} started now running...", carImages.IndexOf(img));

                    Guid guid = Guid.NewGuid(); //generate uniqe guid to prevent file conflicts

                    Bitmap resizedImage = img; //DataManager.MaxResizeImage(image, 200, 200);

                    byte[] imageData;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        resizedImage.Compress(Bitmap.CompressFormat.Png, 100, stream);
                        imageData = stream.ToArray();
                        resizedImage.Recycle(); //clear the data from memory
                    }

                    string fileName = username + guid + ".png";
                    ParseFile imgfile = new ParseFile(fileName, imageData);


                    await imgfile.SaveAsync(new Progress<ParseUploadProgressEventArgs>(args =>
                    {
                        var uploadProgress = (int) (args.Progress*100);
                        Console.WriteLine(string.Format("Vehicle Image upload progress {0}%", uploadProgress));

                    }), cancellationToken: CancellationToken.None);

                    ParseObject imageObject = new ParseObject("Images");
                    imageObject["vehicleObjectId"] = carObjectId;
                    imageObject["username"] = username;
                    imageObject["image"] = imgfile;

                    //save the vehicle pointer id to allow for better linking
                    imageObject.Add("vehicle", ParseObject.CreateWithoutData("VehicleProfile", carObjectId));

                    await imageObject.SaveAsync();



                    Console.WriteLine("Image uploading finished {0} Filename: {1}", username, fileName);
                });

                Console.WriteLine("All images finished uploading successfully {0} in total", carImages.Count);

            }
            catch (Exception ex)
            {
                var message = "Unable to save images " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
        }

        public async Task<ParseFile> SaveDealerLicense(Bitmap image, string username)
        {
            ;
            try
            {
                //create parse user object to ger current logged in user
                Guid guid = Guid.NewGuid(); //generate uniqe guid to prevent file conflicts


                string fileName = username + guid + ".png";
                byte[] imageData;
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Compress(Bitmap.CompressFormat.Png, 100, stream);
                    imageData = stream.ToArray();
                }

                var imgfile = new ParseFile(fileName, imageData);


                await imgfile.SaveAsync(new Progress<ParseUploadProgressEventArgs>(args =>
                {
                    Console.WriteLine("Dealer license upload progress " + args.Progress*100 + "%");

                }), cancellationToken: CancellationToken.None);
                return imgfile;
            }
            catch (Exception ex)
            {
                var message = "Unable to save dealer license " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

            return null;
        }


        public async Task<string> CheckConversationAlreadyExists(string senderUserId, string receipientUserId)
        {
            string convResult = null;

            try
            {

                var conversationQuery =
                    await
                        (ParseObject.GetQuery("Conversation")
                            .WhereEqualTo("senderUserID", senderUserId)
                            .WhereEqualTo("receipientUserID", receipientUserId)).FirstAsync();

                if (conversationQuery != null)
                {
                    convResult = conversationQuery.ObjectId;
                }
            }
            catch (Exception ex)
            {
                var message = "Error checking duplicate conversation " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

            return convResult;
        }

        public async Task<string> CreateConversationThread(Conversation conversation)
        {
            string conversationId = null;
            try
            {
                if (!string.IsNullOrEmpty(conversation.senderUserID) &&
                    !string.IsNullOrEmpty(conversation.receipientUserID))
                {
                    ParseObject parseObject = new ParseObject("Conversation");
                    parseObject["senderUserID"] = conversation.senderUserID;
                    parseObject["receipientUserID"] = conversation.receipientUserID;
                    parseObject["firstMessageDate"] = conversation.firstMessageDate;
                    parseObject["lastMessageDate"] = conversation.lastMessageDate;
                    await parseObject.SaveAsync();

                    conversationId = parseObject.ObjectId;
                }
            }
            catch (Exception ex)
            {
                var message = "Unable to create conversation " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

            return conversationId;
        }

        public async void UpdateConversationDates(string conversationId)
        {
            var updateObject = ParseObject.CreateWithoutData("Conversation", conversationId);
            updateObject["lastMessageDate"] = DateTime.UtcNow;

            await updateObject.SaveAsync();
        }
        public async Task DeleteConversation(string conversationId)
        {
            try
            {
                ParseObject parseObject = new ParseObject("Conversation");
                parseObject.ObjectId = conversationId;
                await parseObject.DeleteAsync();
            }
            catch (Exception ex)
            {
                var message = "Unable to delete message " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
        }

        #region functions to return the car makes and models, we will filter the models by passing the parameter of the make

        /// <summary>
        /// Return all the vehicle makes in teh parse table
        /// note the limit for return data is 100, check function for how to override
        /// </summary>
        /// <returns>List object</returns>
        public List<string> GetVehicleMake(bool isFilterScenario = false)
        {
            /*
            var makeList = await vehicleData.FetchVehicleMakes();

            return makeList;*/
                var makesList = offlineData.FetchAllMakes(isFilterScenario);
            return makesList;
        }

        public List<Tuple<string, string>> GetVehicleModels(string vehicleMake = null)
        {
            /*
            var makeList = await vehicleData.FetchVehicleMakes();

            return makeList;*/
            var modelsList = offlineData.FetchAllModels(vehicleMake);
            return modelsList;
        }

        /// <summary>
        /// Get the vehicle model based on a particular make
        /// </summary>
        /// <param name="vehicleMake">List object</param>
        /// <returns>List object</returns>
        public List<string> GetVehicleModel(List<string> vehicleMake)
        {

            //var data = await vehicleData.FetchVehicleModel(vehicleMake);
            //return data;

            var data = offlineData.FilterModels(vehicleMake);
            return data;
        }

        public async Task<List<Tuple<string, string, string, string, Uri, string>>> GetVehcileProfile(int limit = 20,
            int pageIndex = 0)
        {
            //@TODO cache to sqlite table before checing parse
            var vehicleProfile = new List<Tuple<string, string, string, string, Uri, string>>();
            IEnumerable<ParseObject> vehicleProfileData;
            List<VehicleProfileTable> sqlVehicleProfileList = new List<VehicleProfileTable>();
            string listPrice;
            string make = null;
            string carModel = null;

            try
            {

                //lets delete a test image
                vehicleProfileData = await vehicleData.VehicleProfile(limit, pageIndex);

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var profile in vehicleProfileData)
                {

                    string rawPrice = profile.ContainsKey("listPrice") ? profile.Get<string>("listPrice") : "0";
                    listPrice = HelperClass.CleanUpPrice(rawPrice);
                    make = profile.ContainsKey("make") ? profile.Get<string>("make") : "N/A";
                    carModel = profile.ContainsKey("model") ? profile.Get<string>("model") : "N/A";
                    var username = profile.ContainsKey("username") ? profile.Get<string>("username") : "N/A";

                    var imgUrl = await vehicleData.FetchSingleCarImage(profile.ObjectId);
                    if (imgUrl != null)
                    {
                        var vehicleTuple = new Tuple<string, string, string, string, Uri, string>(profile.ObjectId, make,
                            listPrice, carModel, imgUrl, username);
                        vehicleProfile.Add(vehicleTuple);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Unable search vehicle " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }


            return vehicleProfile;
        }

        public async Task<List<Tuple<string, string, string, string, Uri, string>>> SearchVehcileProfile(string makeString, string modelString = null, string yearString = null)
        {
            var vehicleProfile = new List<Tuple<string, string, string, string, Uri, string>>();

            var vehicleProfileData = await vehicleData.FilterVehicleProfile(makeString, modelString, yearString);
            try
            {
                foreach (var profile in vehicleProfileData)
                {

                    var rawPrice = profile.ContainsKey("listPrice") ? profile.Get<string>("listPrice") : "0";
                    var listPrice = HelperClass.CleanUpPrice(rawPrice);
                    var make = profile.ContainsKey("make") ? profile.Get<string>("make") : "N/A";
                    string model = profile.ContainsKey("model") ? profile.Get<string>("model") : "N/A";
                    string username = profile.ContainsKey("username") ? profile.Get<string>("username") : "N/A";

                    var imgUrl = await vehicleData.FetchSingleCarImage(profile.ObjectId);
                    if (imgUrl != null)
                    {
                        var vehicleTuple = new Tuple<string, string, string, string, Uri, string>(profile.ObjectId, make,
                            listPrice, model, imgUrl, username);
                        vehicleProfile.Add(vehicleTuple);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Unable to search vehilde " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
            return vehicleProfile;
        }

        public async Task<List<Tuple<string, string, string, string, Uri, string>>> SearchVehicleByModelOnly(
            string makeString, string modelString, string yearString = null)
        {
            var vehicleProfile = new List<Tuple<string, string, string, string, Uri, string>>();

            var vehicleProfileData = await vehicleData.FilterVehicleProfile(makeString, modelString, yearString);
            try
            {
                foreach (var profile in vehicleProfileData)
                {
                    var rawPrice = profile.ContainsKey("listPrice") ? profile.Get<string>("listPrice") : "0";
                    var listPrice = HelperClass.CleanUpPrice(rawPrice);
                    var make = profile.ContainsKey("make") ? profile.Get<string>("make") : "N/A";
                    string model = profile.ContainsKey("model") ? profile.Get<string>("model") : "N/A";
                    string username = profile.ContainsKey("username") ? profile.Get<string>("username") : "N/A";

                    var imgUrl = await vehicleData.FetchSingleCarImage(profile.ObjectId);

                    if (imgUrl != null)
                    {
                        var vehicleTuple = new Tuple<string, string, string, string, Uri, string>(profile.ObjectId, make,
                            listPrice, model, imgUrl, username);
                        vehicleProfile.Add(vehicleTuple);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Unable to search vehicle " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
            return vehicleProfile;
        }


        public async Task<bool> LoadSingleVehicleProfile(string vehicleObjectId)
        {
            ParseObject vehicle = null;
            CarProfile.ResetFields();
            CarProfile.ClearInventoryImages();
            try
            {

                var vehicleProfileData = await vehicleData.GetIndividualVehicleProfile(vehicleObjectId);

                //end execution if data is null;
                if (!vehicleProfileData.Any())
                {
                    //call if teh first batch had no data
                    vehicleProfileData = await vehicleData.GetIndividualVehicleProfile(vehicleObjectId, true);
                    foreach (var noImageData in vehicleProfileData)
                    {
                        vehicle = noImageData; //.Get<ParseObject>("VehicleProfile");
                    }
                }
                else
                {
                    //
                    //lets get each image entry here
                    //first clear the previously set values in teh CarProfile class
                    foreach (var vehicleObject in vehicleProfileData)
                    {
                        //get the vehicle object details
                        vehicle = vehicleObject.Get<ParseObject>("vehicle");
                        var imageFile = vehicleObject.Get<ParseFile>("image");
                        //add the image to the image list
                        Uri imageUrl = imageFile.Url; //get the absolute path


                        CarProfile.VehicleImages(imageUrl);
                    }
                }
                //var h = CarProfile.carsForSale.Count;

                if (vehicle != null)
                {

                    if (vehicle.ContainsKey("engine"))
                    {
                        CarProfile.engine = vehicle.Get<string>("engine");
                    }


                    if (vehicle.ContainsKey("transmission"))
                    {
                        CarProfile.transmission = vehicle.Get<string>("transmission");
                    }

                    if (vehicle.ContainsKey("exteriorColor"))
                    {
                        CarProfile.exteriorColor = vehicle.Get<string>("exteriorColor");
                    }


                    if (vehicle.ContainsKey("interiorColor"))
                    {
                        CarProfile.interiorColor = vehicle.Get<string>("interiorColor");
                    }


                    if (vehicle.ContainsKey("priceOnRequest"))
                    {
                        CarProfile.priceOnRequest = vehicle.Get<bool>("priceOnRequest");
                    }


                    if (vehicle.ContainsKey("vat"))
                    {
                        CarProfile.vat = vehicle.Get<bool>("vat");
                    }


                    if (vehicle.ContainsKey("model"))
                    {
                        CarProfile.model = vehicle.Get<string>("model");
                    }


                    if (vehicle.ContainsKey("mileage"))
                    {
                        CarProfile.mileage = vehicle.Get<int>("mileage");
                    }

                    if (vehicle.ContainsKey("distanceUnit"))
                    {
                        CarProfile.milesData = vehicle.Get<string>("distanceUnit");
                    }


                    if (vehicle.ContainsKey("listPrice"))
                    {
                        CarProfile.listPrice = vehicle.Get<string>("listPrice");
                    }


                    if (vehicle.ContainsKey("carType"))
                    {
                        CarProfile.carType = vehicle.Get<string>("carType");
                    }


                    if (vehicle.ContainsKey("make"))
                    {
                        CarProfile.make = vehicle.Get<string>("make");
                    }



                    if (vehicle.ContainsKey("options"))
                    {
                        CarProfile.options = vehicle.Get<string>("options");
                    }


                    if (vehicle.ContainsKey("fuelType"))
                    {
                        CarProfile.fuelType = vehicle.Get<string>("fuelType");
                    }


                    if (vehicle.ContainsKey("status"))
                    {
                        CarProfile.status = vehicle.Get<string>("status");
                    }


                    if (vehicle.ContainsKey("username"))
                    {
                        CarProfile.ownerUsername = vehicle.Get<string>("username");
                    }


                    if (vehicle.ContainsKey("vin"))
                    {
                        CarProfile.vin = vehicle.Get<string>("vin");
                    }


                    if (vehicle.ContainsKey("condition"))
                    {
                        CarProfile.condition = vehicle.Get<string>("condition");
                    }


                    if (vehicle.ContainsKey("modelVariant"))
                    {
                        CarProfile.modelVariant = vehicle.Get<string>("modelVariant");
                    }


                    if (vehicle.ContainsKey("year"))
                    {
                        CarProfile.year = vehicle.Get<string>("year");
                    }

                    if (vehicle.ContainsKey("driveTrain"))
                    {
                        CarProfile.driveTrain = vehicle.Get<string>("driveTrain");
                    }

                    if (vehicle.ContainsKey("description"))
                    {
                        CarProfile.description = vehicle.Get<string>("description");
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                var message = "Unable to load vehicle profile " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

            return false;
        }

        #endregion

        public void UpdateAppAccessDates(DateTime? appAccessStartDate, DateTime? appAccessEndDate)
        {
            try
            {
                ParseUser loggedInUser = ParseUser.CurrentUser;
                loggedInUser["appAccessStartDate"] = appAccessStartDate;
                loggedInUser["appAccessEndDate"] = appAccessEndDate;

                loggedInUser.SaveAsync();
            }
            catch (Exception ex)
            {
                var message = "Unable to get app dates " + ex.Message + ex.StackTrace;
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }
        }

    }
}

