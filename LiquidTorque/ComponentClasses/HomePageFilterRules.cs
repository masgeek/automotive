using System;
using System.Collections.Generic;
using System.Linq;
using Parse;

namespace liquidtorque.ComponentClasses
{
    public class HomePageFilterRules
    {
        static ParseQuery<ParseObject> _vehicleProfileQuery;

        public HomePageFilterRules()
        {
            _vehicleProfileQuery = new ParseQuery<ParseObject>();
        }

        public static ParseQuery<ParseObject> DetermineSearchFilter(string make, string model, string year)
        {
			if (year != null) {
				if (year.Equals ("All")) {
					year = null;
				}
			}

            ParseQuery<ParseObject> query1 = null;
            ParseQuery<ParseObject> query2 = null;
            ParseQuery<ParseObject> query3 = null;
            ParseQuery<ParseObject> query4 = null;
            ParseQuery<ParseObject> query5 = null;
            ParseQuery<ParseObject> query6 = null;
            ParseQuery<ParseObject> query7 = null;
            ParseQuery<ParseObject> query8 = null;
            //first check the all models conditions

            try
            {
                query1 = AllMakesNoModelNoYear(make, model, year);
                if (query1 == null)
                {
                    query2 = AllMakesHasModelNoYear(make, model, year);
                }
                if (query2 == null)
                {
                    query3 = AllMakesNoModelHasYear(make, model, year);
                }

                if (query3 == null)
                {
                    query4 = AllMakesHasModelHasYear(make, model, year);
                }

                if (query4 == null)
                {
                    query5 = OneMakesNoModelNoYear(make, model, year);
                }

                if (query5 == null)
                {
                    query6 = OneMakesHasModelNoYear(make, model, year);
                }

                if (query6 == null)
                {
                    query7 = OneMakesNoModelHasYear(make, model, year);
                }

                if (query7 == null)
                {
                    query8 = OneMakesHasModelHasYear(make, model, year);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Query filter error " + ex.Message + ex.StackTrace);
            }

            return _vehicleProfileQuery;
        }

        #region When all makes is selected

        public static ParseQuery<ParseObject> AllMakesNoModelNoYear(string make, string model, string year)
        {
            if (make.Equals("All") && string.IsNullOrEmpty(model) && string.IsNullOrEmpty(year))
            {
                //return all vehicles
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .OrderByDescending("createdAt");

                Console.WriteLine("Search all makes");

                return _vehicleProfileQuery;
            }

            return null;
        }

        public static ParseQuery<ParseObject> AllMakesHasModelNoYear(string make, string model, string year)
        {
            if (make.Equals("All") && !string.IsNullOrEmpty(model) && string.IsNullOrEmpty(year))
            {
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereStartsWith("model", model)
                    .OrderByDescending("createdAt");

                return _vehicleProfileQuery;
            }
            return null;
        }

        public static ParseQuery<ParseObject> AllMakesNoModelHasYear(string make, string model, string year)
        {
            if (make.Equals("All") && string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(year))
            {
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereStartsWith("year", year)
                    .OrderByDescending("createdAt");

                return _vehicleProfileQuery;
            }
            return null;
        }

        public static ParseQuery<ParseObject> AllMakesHasModelHasYear(string make, string model, string year)
        {
            if (make.Equals("All") && !string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(year))
            {
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereStartsWith("model", model)
                    .WhereStartsWith("year", year)
                    .OrderByDescending("createdAt");

                return _vehicleProfileQuery;
            }
            return null;
        }

        #endregion all makes region

        #region when a make is specified

        public static ParseQuery<ParseObject> OneMakesNoModelNoYear(string make, string model, string year)
        {
            if (!make.Equals("All") && string.IsNullOrEmpty(model) && string.IsNullOrEmpty(year))
            {
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereStartsWith("make", make)
                    .OrderByDescending("createdAt");

                return _vehicleProfileQuery;
            }
            return null;
        }

        public static ParseQuery<ParseObject> OneMakesHasModelNoYear(string make, string model, string year)
        {
            if (!make.Equals("All") && !string.IsNullOrEmpty(model) && string.IsNullOrEmpty(year))
            {
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereStartsWith("make", make)
                    .WhereStartsWith("model", model)
                    .OrderByDescending("createdAt");

                return _vehicleProfileQuery;
            }
            return null;
        }

        public static ParseQuery<ParseObject> OneMakesNoModelHasYear(string make, string model, string year)
        {
            if (!make.Equals("All") && string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(year))
            {
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereStartsWith("make", make)
                    .WhereStartsWith("year", year)
                    .OrderByDescending("createdAt");

                return _vehicleProfileQuery;
            }
            return null;
        }

        public static ParseQuery<ParseObject> OneMakesHasModelHasYear(string make, string model, string year)
        {
            if (!make.Equals("All") && !string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(year))
            {
                _vehicleProfileQuery = ParseObject.GetQuery("VehicleProfile")
                    .WhereStartsWith("make", make)
                    .WhereStartsWith("model", model)
                    .WhereStartsWith("year", year)
                    .OrderByDescending("createdAt");

                return _vehicleProfileQuery;
            }
            return null;
        }

        #endregion end make specified region



        /// <summary>
        ///Get list of years to filter by
        /// </summary>
        /// <returns></returns>
        public static List<string> GetYears()
        {
            //@TODO check on android equivalent
            var yearFilterList = new List<string>();
            try
            {

                var years = new List<int>(Enumerable.Range(1930, (2017 - 1930) + 1)).OrderByDescending(i => i).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Year filter exception {0}{1}", ex.Message, ex.StackTrace);
            }
            return yearFilterList;
        }
    }
}

