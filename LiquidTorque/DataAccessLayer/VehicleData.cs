using System;
using System.Collections.Generic;
using System.Linq;
using Parse;
using System.Threading.Tasks;
using liquidtorque.ComponentClasses;
using MetricsManager = HockeyApp.Android.Metrics.MetricsManager;

namespace liquidtorque.DataAccessLayer
{
    /// <summary>
    /// I have extensively used LINQ here so as to speed up the query process
    /// Class to retrieve the vehicle make and models object from parse
    /// @TODO consider adding lazy loading option to cate for vehicle numbers greater than 1000
    /// </summary>
    public class VehicleData
    {
        /// <summary>
        /// Return a list data type form the async  wait List<string></string>
        /// You can put it in a controller view or filter search
        /// </summary>
        /// <returns>Vehicle make List object</returns>
        public async Task<List<string>> FetchVehicleMakes()
        {
            try
            {
                ParseQuery<ParseObject> makeQuery = ParseObject.GetQuery("Make");
                var makes = await makeQuery.FindAsync();
                //put the makes in a list object
                return makes.Select(make => make.Get<string>("make")).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }


        public async Task<List<Tuple<string, string>>> PopulateMakes()
        {
            var makesList = new List<Tuple<string, string>>();
            try
            {
                //@TODO add callback function to check if the records will exceed 1000, parse limits query result to 1000 for each call
                ParseQuery<ParseObject> makeQuery = ParseObject.GetQuery("Make").Limit(1000);
                var makes = await makeQuery.FindAsync();

                makesList.AddRange(
                    makes.Select(
                        make => new Tuple<string, string>(
                            make.Get<string>("make"),
                            make.ObjectId
                            ))
                    );
                return makesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting makes to table " + ex.Message + ex.Message);
                return null;
            }
        }

        public async Task<List<Tuple<string, string, string>>> PopulateModels()
        {
            var modelsList = new List<Tuple<string, string, string>>();
            try
            {
                //@TODO add callback function to check if teh records will exceed 1000, parse limits query result to 1000 for each call
                ParseQuery<ParseObject> makeQuery = ParseObject.GetQuery("Model").Limit(1000);
                var models = await makeQuery.FindAsync();

                modelsList.AddRange(
                    models.Select(
                        model => new Tuple<string, string, string>(
                            model.Get<string>("make"),
                            model.Get<string>("model"),
                            model.ObjectId
                            ))
                    );
                return modelsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting models to table " + ex.Message + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Fetchs the vehicle models.
        /// </summary>
        /// <returns>The vehicle model.</returns>
        /// <param name="vehicleMake">Vehicle make.</param>
        public async Task<List<string>> FetchVehicleModel(List<string> vehicleMake)
        {
            List<string> modelsList = new List<string>();
            try
            {
                do
                {

                    for (var i = vehicleMake.Count - 1; i >= 0; i--)
                    {
                        //pass to parse query
                        var modelQuery = ParseObject.GetQuery("Model")
                            //.WhereMatches("make", vehicleMake[i]);
                            .WhereContains("make", vehicleMake[i]);
                        //after returning add the result to list and proceed
                        var vehicleModels = await modelQuery.FindAsync();

                        modelsList.AddRange(vehicleModels.Select(vehicleModel => vehicleModel.Get<string>("model")));

                        //after building the array remove the index of the queried item
                        vehicleMake.RemoveAt(i);
                    }
                } while (vehicleMake.Any());
                //return the filtered models list
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            catch (Exception genEx)
            {
                Console.WriteLine(genEx.Message + genEx.StackTrace);
            }

            return modelsList;
        }


        /// <summary>
        /// Fetchs the single car image.
        /// </summary>
        /// <returns>The single car image.</returns>
        /// <param name="carObjectId">Car object identifier.</param>
        public async Task<Uri> FetchSingleCarImage(string carObjectId)
        {
            //carObjectId = "BWRVh6Fzx0";
            //return null; //vehicleObjectId
            //@TODO Remove this image placeholder
            Uri imgUrl = null; //new Uri("http://fusionluxurymotors.com/images/inventory/lrg/zffcw56a130135440_10.jpg");
            try
            {

                var vehicleProfileQuery = ParseObject.GetQuery("Images")
                    .WhereEqualTo("vehicleObjectId", carObjectId);
                //.Include("vehicle")
                //.WhereMatchesQuery("vehicle", vehicle)
                //.Limit(1); //limit to only one record

                var vehicleObject = await vehicleProfileQuery.FirstAsync();

                var imageFile = vehicleObject.Get<ParseFile>("image");
                //add the image to the image list
                imgUrl = imageFile.Url; //get the absolute path

            }
            catch (ParseException e)
            {
                //@TODO delete vehicle profiles with no images
                if (e.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    //dont log this to insights
                    Console.WriteLine("Image not found");
                    //add the vehilce ids to teh table to be deleted
                    DeleteNoImageVehicle(carObjectId);
                }
                else
                {
                    // Some other error occurred
                    //log to insight
                    Console.WriteLine(
                        String.Format("Image file does not exist in the images table {0}{1} car object id {2}", e.Code,
                            e.StackTrace, carObjectId));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Image file does not exist in the images table " + ex.Message);
            }

            return imgUrl;
        }

        /// <summary>
        /// returns all images belonging to a particular vehicle
        /// </summary>
        /// <param name="carObjectId"></param>
        /// <returns></returns>
        public static async Task<List<Uri>> FetchAllCarImage(string carObjectId)
        {

            List<Uri> imageList = new List<Uri>();
            //lets clear the global inventory list first
            CarProfile.ClearInventoryImages();
            CarProfile.ResetFields();
            try
            {

                var vehicleProfileQuery = ParseObject.GetQuery("Images")
                    .WhereEqualTo("vehicleObjectId", carObjectId);

                var vehicleObject = await vehicleProfileQuery.FindAsync();
                foreach (var imgObject in vehicleObject)
                {
                    var imgObj = imgObject.Get<ParseFile>("image");
                    imageList.Add(imgObj.Url);
                    //also add url to the global car for sale object to be used ion the other view
                    CarProfile.VehicleInventoryImages(imgObj.Url); //pass the uri values for this
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                //Insights.Report(ex);
            }

            return imageList;
        }

        /// <summary>
        /// Load the list of vehicles we want to delete
        /// </summary>
        /// <param name="carObjectId"></param>
        /// <returns></returns>
        public static async Task<List<Tuple<string, Uri>>> FetchAllCarImagesForDeletion(string carObjectId)
        {

            var imageDeletionList = new List<Tuple<string, Uri>>();
            try
            {

                var vehicleProfileQuery = ParseObject.GetQuery("Images")
                    .WhereEqualTo("vehicleObjectId", carObjectId);

                var vehicleObject = await vehicleProfileQuery.FindAsync();
                foreach (var imgObject in vehicleObject)
                {
                    var imageObjectId = imgObject.ObjectId; //.Get<string>("objectId");
                    var imgObj = imgObject.ContainsKey("image") ? imgObject.Get<ParseFile>("image") : null;
                    if (imgObj != null)
                    {
                        imageDeletionList.Add(new Tuple<string, Uri>(imageObjectId, imgObj.Url));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Deletion image setting error {0}{1}", ex.StackTrace, ex.Source);
            }

            return imageDeletionList;
        }

        public async Task<IEnumerable<ParseObject>> VehicleProfile(int dataCount = 50, int pageIndex = 0)
        {
            //@TODO find a wauy to cache the data locally
            try
            {
                var vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .Limit(dataCount)
                    .Skip(pageIndex*dataCount)

                    .OrderByDescending("createdAt");



                var profileData = await vehicleProfileQuery.FindAsync();

                return profileData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting vehicle data " + ex.Message + ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Get all vehicles for a particular user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="limit"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ParseObject>> GetUserVehicle(string userName, int limit = 50, int pageIndex = 0)
        {
            try
            {

                var vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereEqualTo("username", userName)
                    .Limit(limit)
                    .Skip(pageIndex*limit)
                    .OrderByDescending("createdAt");

                var profileData = await vehicleProfileQuery.FindAsync();

                return profileData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Vehicle fetching error " + ex.Message + ex.StackTrace);
                return null;
            }
        }

        public async Task<IEnumerable<ParseObject>> FilterVehicleProfile(string makeString, string modelString,
            string yearString, int limit = 70, int pageIndex = 0)
        {
            try
            {
                //if model and year are null search for make
                var vehicleProfileQuery = HomePageFilterRules.DetermineSearchFilter(makeString, modelString, yearString);


                var profileData = await vehicleProfileQuery.FindAsync();

                return profileData;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Model search error " + ex.Message + ex.StackTrace);
            }
            return null;
        }

        public async Task<IEnumerable<ParseObject>> GetIndividualVehicleProfile(string objectId,
            bool imageMissing = false)
        {
            try
            {
                var vehicle = new ParseQuery<ParseObject>("VehicleProfile").WhereEqualTo("objectId", objectId);

                var vehicleProfileQuery = ParseObject.GetQuery("Images")
                    .Include("vehicle")
                    .WhereMatchesQuery("vehicle", vehicle);
                var profileData = await vehicleProfileQuery.FindAsync();

                if (imageMissing)
                {
                    profileData = await vehicle.FindAsync();
                }
                //if the car has no image let us query the old way
                return profileData;

            }
            catch (Exception ex)
            {

                Console.Write("Error getting individual vehicle profile " + ex.Message + ex.StackTrace);
            }

            return null;
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            var corrected = char.ToUpper(s[0]) + s.Substring(1);

            return corrected;
        }


        /// <summary>
        /// This handles the delettion of a particular car Image
        /// </summary>
        /// <param name="imageObjectId"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteVehicleImage(string imageObjectId)
        {
            //lets search for the image based on the passed id
            try
            {
                var query = ParseObject.GetQuery("Images").WhereEqualTo("objectId", imageObjectId);

                IEnumerable<ParseObject> results = await query.FindAsync();
                if (results == null) return false; //return false if the result data is empty, otherwise proceed

                foreach (ParseObject deleteObject in results)
                {
                    await deleteObject.DeleteAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting image " + ex.Message + ex.StackTrace);
                MetricsManager.TrackEvent(ex.StackTrace + ex.Message);
            }
            return false;
        }

        public async void DeleteNoImageVehicle(string carObjectId)
        {
            var vehicle = new ParseQuery<ParseObject>("VehicleProfile")
                .WhereEqualTo("objectId", carObjectId);

            IEnumerable<ParseObject> car = await vehicle.FindAsync();
            foreach (var v in car)
            {
                await v.DeleteAsync(); //remove the missing vehicle
                Console.WriteLine("Vehicle profile deleted");
            }
        }
    }
}
