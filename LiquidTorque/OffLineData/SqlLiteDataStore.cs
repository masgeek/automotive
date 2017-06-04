using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using liquidtorque.DataAccessLayer;
using SQLite;
using Environment = System.Environment;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable MemberCanBeMadeStatic.Local


namespace liquidtorque.OffLineData
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlLiteDataStore
    {
        private static SqlLiteDataStore _offlineStorage;
        private static readonly object MyObject = new object();
        static readonly string DatabasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string DatabaseFilePath
        {
            get
            {
                const string dbName = "liquidtorque_cache.db";
                var dbPath = Path.Combine(DatabasePath, dbName);
                return dbPath;
            }
        }

        private SqlLiteDataStore()
        {
            
        }

        public static SqlLiteDataStore GetInstance()
        {
            lock (MyObject)
            {
                return _offlineStorage ?? (_offlineStorage = new SqlLiteDataStore());
            }
        }

        public void CreateDatabaseAndTables()
        {
            try
            {
                using (var conn = new SQLiteConnection(DatabaseFilePath))
                {
                    conn.CreateTable<MakeTable>(); //makes table
                    conn.CreateTable<ModelTable>(); //models table
                    conn.CreateTable<MissingImages>(); //missing images table
                    conn.CreateTable<VehicleProfileTable>(); //vehicle profile table
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database creation exception "+ex.Message );
            }
        }

        #region data insertion into the tables

        public void InsertMakes(List<MakeTable> makesList)
        {
            try
            {
                using (var db = new SQLiteConnection(DatabaseFilePath))
                {
                    db.BeginTransaction();
                    foreach (var make in makesList)
                    {
                        if (!CheckIfMakeExists(make.ParseObjectId, make.Make))
                        {
                            db.Insert(make);
                        }
                        else
                        {
                            Console.WriteLine(make.Make+" already exists");
                        }
                    }
                    db.Commit();
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Exception for inserting value into database " + ex.Message);
            }
            catch (Exception exGe)
            {
                Console.WriteLine("General exception " + exGe.Message);
            }

        }

        public void InsertModels(List<ModelTable> modelsList)
        {
            try
            {
                using (var db = new SQLiteConnection(DatabaseFilePath))
                {
                    db.BeginTransaction();

                    foreach (var model in modelsList)
                    {
                        if (!CheckIfModelExists(model.ParseObjectId))
                        {
                            db.Insert(model);
                        }
                        else
                        {
                            Console.WriteLine(model.Model + " already exists");
                        }
                    }
                    db.Commit();
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Exception for database " + ex.Message);
            }
            catch (Exception exGe)
            {
                Console.WriteLine("General exception "+exGe.Message);
                
            }

        }

        #endregion end of data insertion into tables

        #region Data fetching from tables

        public List<string> FetchAllMakes(bool isfilterScenario = false)
        {
            List<string> makeList = new List<string> { isfilterScenario ? "All" : "Select Make" };

            try
            {
                using (var db = new SQLiteConnection(DatabaseFilePath))
                {
                    TableQuery<MakeTable> data = db.Table<MakeTable>();

                    TableQuery<MakeTable> foundMakes = from vehicleMake in data
                                                       orderby vehicleMake.Make
                                                        select vehicleMake
                                                        ;
                    //we need to sort t alphabetically first
                    // makeList.AddRange(data.Select(makeTable => makeTable.Make));
                    foreach (var makeTable in foundMakes)
                    {
                        makeList.Add(makeTable.Make);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in local db "+ex.Message);
            }

            return makeList;
        }


		public List<Tuple<string, string>> FetchAllModels(string filterMake)
        {
		    var modelList = new List<Tuple<string, string>>();
		    try
            {
                using (var db = new SQLiteConnection(DatabaseFilePath))
                {
                    TableQuery<ModelTable> data = db.Table<ModelTable>();

                    if (string.IsNullOrEmpty(filterMake))
                    {
                        foreach (var modelTable in data)
                        {
                            modelList.Add(new Tuple<string, string>(modelTable.Model, modelTable.Make));
                        }
                    }
                    else
                    {
                        TableQuery<ModelTable> foundMakes = from vehicleMake in data
                            where vehicleMake.Make.StartsWith(filterMake)
                            select vehicleMake;

                        foreach (var modelTable in foundMakes)
                        {
                            modelList.Add(new Tuple<string, string>(modelTable.Model, modelTable.Make));
                        }
                    }

                    //modelList.AddRange(data.Select(modelTable => modelTable.Model.ToString()));
  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in local db " + ex.Message);
            }
            return modelList;
        }
        public List<string> FilterModels(List<string> make)
        {
            List<string> modelsList = new List<string>();
            try
            {
                do
                {

                    for (var i = make.Count - 1; i >= 0; i--)
                    {
                        var makeToFilter = make[i];
                        using (var db = new SQLiteConnection(DatabaseFilePath))
                        {
                            var data = db.Table<ModelTable>();

                            var foundMakes = from vehicleMake in data
                                             where vehicleMake.Make.StartsWith(makeToFilter)
                                             select vehicleMake;

                            // modelList.AddRange(elements.Select(modelTable => modelTable.Make));
                            foreach (var modelTable in foundMakes)
                            {
                                modelsList.Add(modelTable.Model);
                            }
                        }

                        //after building the array remove the index of the queried item
                        make.RemoveAt(i);
                    }
                } while (make.Any());
                //return the filtered models list
            }
            catch (Exception genEx)
            {
                Console.WriteLine("Exception "+genEx.Message);
            }

            return modelsList;
        }

        public List<string> FilterMakes(string objectId,string make)
        {

            var modelList = new List<string>();
            try
            {
                using (var db = new SQLiteConnection(DatabaseFilePath))
                {
                    var data = db.Table<MakeTable>();

                    var foundMakes = from makeTable in data
                                   where makeTable.ParseObjectId.Equals(objectId) && makeTable.Make.Equals(make)
                                   select makeTable;

                    // modelList.AddRange(elements.Select(modelTable => modelTable.Make));
                    foreach (var modelTable in foundMakes)
                    {
                        modelList.Add(modelTable.Make);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to fetch list " + ex.Message);
            }

            return modelList;
        }
        #endregion End data fetching from tables

        /// <summary>
        /// Checks if the database already exists on the device
        /// if it does we wont fetch data from parse.
        /// if it doesnt we will  fetch data from parse
        /// </summary>
        public async void CheckIfDatabaseExists()
        {
            try
            {

                //documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var dbPath = Path.Combine(DatabasePath, DatabaseFilePath);

                if (File.Exists(dbPath))
                {
                    //dont fetch data from parse, instead get it from the local storage

                    //File.Delete(dbPath);
                    Console.WriteLine("Database exists, skip creation");
                }
                else
                {
                    Console.WriteLine("Database does not exist, lets create it");
                    var vehicles = new VehicleData();
                    //lets call parse tables and fetch the data then insert it into our tables
                    //instantiate and create then populate table
                    CreateDatabaseAndTables();

                    List<Tuple<string, string>> makes = await vehicles.PopulateMakes(); //dataManager.GetVehicleMakes();

                    //add the above to a class
                    List<MakeTable> sqlMakeList = makes.Select(make => new MakeTable
                    {
                        Make = make.Item1.Trim(),
                        ParseObjectId = make.Item2
                    }).ToList();

                    var models = await vehicles.PopulateModels(); // dataManager.GetVehicleModels();

                    List<ModelTable> sqlModelList = models.Select(model => new ModelTable
                    {
                        Make = model.Item1.Trim(),
                        Model = model.Item2.Trim(),
                        ParseObjectId = model.Item3
                    }).ToList();

                    //now lets add this data to the tables
                    InsertMakes(sqlMakeList);
                    InsertModels(sqlModelList);
                }
            }
            catch (Exception ex)
            {
              //send this data to insights too
                Console.WriteLine("Unable to build database " + ex.Message);
            }
        }

        /// <summary>
        /// This helps prevent duplicate model entries since I added unique
        /// constraints the columns
        /// </summary>
        /// <param name="vehicleMake"></param>
        /// <returns></returns>
        private bool CheckIfModelExists(string vehicleMake)
        {
            try
            {
                var filterList = new List<string> {vehicleMake};

                var data = FilterModels(filterList);
                if (data.Count > 0) return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Checking error " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Helps prevent duplicate makes based on the object id and make name
        /// Models here is a 1:M relationship
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="make"></param>
        /// <returns></returns>
        private bool CheckIfMakeExists(string objectId, string make)
        {
            try
            {
                var data = FilterMakes(objectId,make);
                if (data.Count > 0) return true;
            }
            catch (Exception ex)
            {
               Console.WriteLine("Checking error "+ex.Message);
            }
            return false;
        }
    }
}