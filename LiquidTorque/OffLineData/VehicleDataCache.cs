using System;
using System.Collections.Generic;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using Newtonsoft.Json;
using SQLite;

namespace liquidtorque.OffLineData
{
    public class VehicleDataCache
    {
        private readonly string _databaseFilePath = SqlLiteDataStore.DatabaseFilePath;
        private static readonly object MyObject = new object();
        private static VehicleDataCache vCache;
        private readonly VehicleData vehicleData;
        public static VehicleDataCache GetInstance()
        {
            lock (MyObject)
            {
                return vCache ?? (vCache = new VehicleDataCache());
            }
        }

        public VehicleDataCache()
        {
            vehicleData = new VehicleData();
        }
        public void InsertVehcileProfile(List<VehicleProfileTable> vehicleTable)
        {
   
            try
            {
                using (var db = new SQLiteConnection(_databaseFilePath))
                {
                    db.BeginTransaction();
                    foreach (var car in vehicleTable)
                    {
                        if (!CheckIfMakeExists(car.ParseObjectId))
                        {
                            db.Insert(car);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Data for vehcile id {0} already exists", car.ParseObjectId));
                            //@TODO we should turn this off
                            MetricsManager.TrackEvent(string.Format("Data for vehcile id {0} already exists", car.ParseObjectId));
                        }
                    }
                    db.Commit();
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Exception for inserting value into database " + ex.Message);
                MetricsManager.TrackEvent("Exception for inserting value into database " + ex.Message+ex.StackTrace);
            }
            catch (Exception exGe)
            {
                Console.WriteLine("General exception " + exGe.Message);
                MetricsManager.TrackEvent("Exception for inserting value into database " + exGe.Message+exGe.StackTrace);
            }

        }

        public bool UpdateVehcileCacheData(string parseObjectId,string make, string model,string vehicleJson)
        {
            try
            {
                using (var db = new SQLiteConnection(_databaseFilePath))
                {
                    var query = db.Table<VehicleProfileTable>().Where(v => v.ParseObjectId.Equals(parseObjectId));

                    
                    db.BeginTransaction();
                    foreach (var car in query)
                    {
                        car.Make = make;
                        car.Model = model;
                        car.VehicleJsonData = vehicleJson; 
                        db.Update(car); //update the new data
                        Console.WriteLine(string.Format("Data for vehcile id {0} already exists", car.ParseObjectId));
                    }
                    db.Commit();
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Exception for updatng value into database " + ex.Message);
                MetricsManager.TrackEvent("Exception for inserting value into database " + ex.Message + ex.StackTrace);
            }
            catch (Exception exGe)
            {
                Console.WriteLine("General exception " + exGe.Message);
                MetricsManager.TrackEvent("Exception for inserting value into database " + exGe.Message + exGe.StackTrace);
            }
            return false;
        }

        public List<Tuple<string, string, string, string, Uri,string>> FetchCachedVehiclesList(string userName  = null)
        {
            var vehicleList = new List<Tuple<string, string, string, string, Uri,string>>();
            List<VehicleProfile> lists = new List<VehicleProfile>();
            try
            {
                using (var db = new SQLiteConnection(_databaseFilePath))
                {
                    //if username is null do not use it as a filter in the query
                    TableQuery<VehicleProfileTable> query;
                    if(string.IsNullOrEmpty(userName))
                    {
                        query = db.Table<VehicleProfileTable>();

                    }else{
                        query = db.Table<VehicleProfileTable>().Where(v => v.Username.Equals(userName));
                    }

                    foreach (var car in query)
                    {
                        if (string.IsNullOrEmpty(userName))
                        {
                            var jsonString = car.VehicleJsonData;
                            lists = JsonConvert.DeserializeObject<List<VehicleProfile>>(jsonString);
                        }
                        else if (string.Equals(car.Username, userName))
                        {
                            var jsonString = car.VehicleJsonData;
                            //convert the string to json object
                            //var vehicleTuple = new Tuple<string, string, string, string, Uri>(profile.ObjectId, make,listPrice, model, imgUrl);
                            //var list = JsonConvert.DeserializeObject<new Tuple<VehicleProfile>()>(jsonString);
                            lists = JsonConvert.DeserializeObject<List<VehicleProfile>>(jsonString);
                        }
                    }

                    //actual final loop

                    foreach (var item in lists)
                    {
                        //convert string to proper url and check if it is formed correctly
                        var urlString = item.Item5;
                        Uri imageUrl = null;

                        if (Uri.IsWellFormedUriString(urlString, UriKind.Absolute))
                        {
                            imageUrl = new Uri(urlString);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Invalid url string {0}", urlString));
                        }
          
                        vehicleList.Add(new
                                Tuple<string, string, string, string, Uri,string>
                                (
                                    item.Item1,
                                    item.Item2,
                                    item.Item3,
                                    item.Item4,
                                    imageUrl,
                                    item.Item6
                                ));
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Exception for updatng value into database " + ex.Message);
                MetricsManager.TrackEvent("Exception for inserting value into database " + ex.Message + ex.StackTrace);
            }
            catch (Exception exGe)
            {
                Console.WriteLine("General exception " + exGe.Message);
                MetricsManager.TrackEvent("Exception for inserting value into database " + exGe.Message + exGe.StackTrace);
            }
            return vehicleList;
        }

        public List<string> FilterVehicle(string make,string model)
        {
            return null;
        }

        public List<string> FilterProfile(string make)
        {
            List<string> profileList = new List<string>();
            try
            {

                using (var db = new SQLiteConnection(_databaseFilePath))
                {
                    var data = db.Table<VehicleProfileTable>();

                    var foundProfile = from vehicleProfileTable in data
                        where vehicleProfileTable.ParseObjectId.Contains(make)
                        select vehicleProfileTable;

                    // modelList.AddRange(elements.Select(modelTable => modelTable.Make));
                    foreach (var modelTable in foundProfile)
                    {
                        profileList.Add(modelTable.Model);
                    }
                }
                //return the filtered models list
            }
            catch (Exception genEx)
            {
                Console.WriteLine("Exception " + genEx.Message+genEx.StackTrace);
                MetricsManager.TrackEvent("Exception for inserting value into database " + genEx.Message + genEx.StackTrace);
            }

            return profileList;
        }

        private bool CheckIfMakeExists(string objectId)
        {
            try
            {
                var data = FilterProfile(objectId);
                if (data.Count > 0) return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Checking error " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// prefill the vehicles table for local cache
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="pageIndex"></param>
        public async void CacheVehicleProfiles(int limit = 1000,int pageIndex = 0)
        {
            var vehicleProfile = new List<Tuple<string, string, string, string, Uri,string>>();
            List<VehicleProfileTable> sqlVehicleProfileList = new List<VehicleProfileTable>();
            try
            {
                var vehicleProfileData = await vehicleData.VehicleProfile(limit, pageIndex);

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var profile in vehicleProfileData)
                {

                    string rawPrice = profile.ContainsKey("listPrice") ? profile.Get<string>("listPrice") : "0";
                    var objectId = profile.ObjectId;
                    var listPrice = HelperClass.CleanUpPrice(rawPrice);
                    var make = profile.ContainsKey("make") ? profile.Get<string>("make") : "N/A";
                    var carModel = profile.ContainsKey("model") ? profile.Get<string>("model") : "N/A";
                    var username = profile.ContainsKey("username") ? profile.Get<string>("username") : "N/A";

                    var imgUrl = await vehicleData.FetchSingleCarImage(profile.ObjectId);
                    if (imgUrl != null)
                    {
                        var vehicleTuple = new Tuple<string, string, string, string, Uri,string>(profile.ObjectId, make,
                            listPrice, carModel, imgUrl,username);
                        vehicleProfile.Add(vehicleTuple);
                    }

                    var json = JsonConvert.SerializeObject(vehicleProfile); //@TODO some chaos happening here corrrect it
                    var profileTable = new VehicleProfileTable()
                    {
                        ParseObjectId = objectId,
                        Make = make,
                        Model = carModel,
                        Username = username,
                        VehicleJsonData = json
                    };

                    sqlVehicleProfileList.Add(profileTable);

                }


                //now insert to table
                vCache.InsertVehcileProfile(sqlVehicleProfileList);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}