using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parse;

namespace liquidtorque.DataAccessLayer
{
	public class ParseStorage
	{
	    private string parseServer = "https://api.parse.com/1";
        /// <summary>
        /// .NET production keys
        /// </summary>
        //private const string AppId = "h6tjVQfVQoTsCiT41iOTt9xlXGUvwkacPx9xMBiG";
        //private const string NetKey = "1C1rwJzM3E993HVKKV2ZOtoJ2NSBruoUzwImtXOx";

        /// <summary>
        /// .NET testing keys
        /// </summary>
        private const string AppId = "l1fG8wihq5uZ0ySpfs8vrdlTUfyK5XkqkIbdxrRD";
        private const string NetKey = "eT4WUadkbMIVNxpbPe4FQZXmyIPmXHJYHSmq1oRd";
        static ParseStorage todoServiceInstance = new ParseStorage ();
     
		Dictionary<string, string> userData = new Dictionary<string, string> ();

		public bool isLoginSuccessful { get; set; }

		public ParseStorage ()
		{
            Initialize();

        }

		public void Initialize () 
		{
			try {
				//Initialize the parse client with your Application ID and .NET Key 
				//ParseClient.Initialize ("h6tjVQfVQoTsCiT41iOTt9xlXGUvwkacPx9xMBiG","1C1rwJzM3E993HVKKV2ZOtoJ2NSBruoUzwImtXOx");
				ParseClient.Initialize (AppId,NetKey);
                /*ParseClient.Initialize(new ParseClient.Configuration
                {
                    ApplicationId = AppId,
                    Server = parseServer
                });*/

            } catch (Exception e) {
				Console.WriteLine ("Initializing error "+e.Message+e.StackTrace);
			}
		}

		public async void SaveParseImage (string imgPath, string fileName)
		{
			try {
				ParseUser user = ParseUser.CurrentUser;

				byte[] b = System.IO.File.ReadAllBytes (imgPath);
				ParseFile imgfile = new ParseFile (fileName, b);
            
				await imgfile.SaveAsync ();
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}

		public async void SignUpUser (ParseUser userData)
		{
			try {
				await userData.SignUpAsync (); //sign up the user
				//we need a way ot telling the user that the sign up is successful
			} catch (Exception e) {
				Console.WriteLine (e.Message);
                
			}
		}

		public async void UploadImage (string path)
		{
			try {
				byte[] b = System.IO.File.ReadAllBytes (path);
				ParseFile imgfile = new ParseFile (path, b);

				await imgfile.SaveAsync ();
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}
			

		public async Task<bool> LoginUser (string username, string password)
		{
			//after login cache user to disk to prevent continuous login everytime 

			try {

				await ParseUser.LogInAsync (username.Trim ().ToLower (), password.Trim ());

				isLoginSuccessful = true;

			} catch (ParseException exception) {

				isLoginSuccessful = false;


				Console.WriteLine ("Login Failed!", "Incorrect Username/Password", exception.Message);
			} 

			return isLoginSuccessful;
		}


		void DisplayError (string title, string errorMessage, params object[] formatting)
		{
            //@FIX show alert here for an eror messsage
            throw new NotImplementedException("This method id yet to be implemented for android");
		}

		public void LogoutUser ()
		{
			try {
				 ParseUser.LogOut();
			} catch (ParseException e) {
				Console.WriteLine (e.Message);
			}
		}

		public async void ResetPassword (string userEmail)
		{
			try {
				//reset link will be sent to the user's email address
				await ParseUser.RequestPasswordResetAsync (userEmail);
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}

	    public async Task<IEnumerable<ParseObject>> FetchUserData(string userName)
	    {
	        IEnumerable<ParseObject> results = null;
	        try
	        {
	            var query = ParseObject.GetQuery("User").WhereEqualTo("username", userName);
	            results = await query.FindAsync();
	        }
	        catch (Exception ex)
	        {
	            Console.WriteLine(ex.Message + ex.StackTrace);
	        }

	        return results;
	    }

	    public async Task<IEnumerable<ParseObject>> FetchData (string className, string columnName, string columnValue)
		{
			var query = ParseObject.GetQuery (className).WhereEqualTo (columnName, columnValue);
			IEnumerable<ParseObject> results = await query.FindAsync ();
			return results;
		}

		public async Task<ParseObject> FetchData (string className, string objectID)
		{
			var query = ParseObject.GetQuery (className);
			ParseObject results = await query.GetAsync (objectID);
			return results;
		}

		public async void SaveParseObject (string className, ParseObject parseObject)
		{
			try {
				await parseObject.SaveAsync ();
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}

		public async void SaveParseUser (string className, ParseUser parseUser)
		{
			try {
				await parseUser.SignUpAsync ();
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
			//await parseUser.SaveAsync ();
		}

		public string GetCurrentUserName ()
		{
			try {
				ParseUser currentUser = ParseUser.CurrentUser;

				if (currentUser != null) {
					return currentUser.Username;
				} else {
					return "";
				}
			} catch (Exception e) {
				Console.WriteLine (e.Message);
				return "";
			}
		}

		public string GetCurrentUserId ()
		{
			ParseUser currentUser = ParseUser.CurrentUser;
			return currentUser.ObjectId;
		}
	}
}