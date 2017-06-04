using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using RestSharp;

namespace liquidtorque.ComponentClasses
{

    /// <summary>
    /// Class to find user city based on their xip code or postal code, 
    /// implmented for UK and US but future countries are also supported
    /// </summary>
	public class GeoLocation
	{
		/// <summary>
		/// The invalid json elements.
		/// </summary>
		public static List<string> InvalidJsonElements;

		/// <summary>
		/// Gets the city from code.
		/// for US countries use countyName
		/// for UK countries use boroughName
#pragma warning disable 1570
        /// http://api.geonames.org/postalCodeSearch?postalcode=9011&country=us&maxRows=10&username=pristone
#pragma warning restore 1570
		/// </summary>
		/// <returns>The city from code.</returns>
		/// <param name="country">Country i.e UK or US.</param>
		/// <param name="zipPostalCode">Zip or Postal code.</param>
		public async Task<Dictionary<string, string>> GetCityFromCode(string country,string zipPostalCode)
		{
		

			var locationDictionary = new Dictionary<string, string>();
			try{
			    if (!string.IsNullOrEmpty(zipPostalCode))
			    {
			        var client = new RestClient("http://api.geonames.org");

			        var request =
			            new RestRequest(string.Format("postalCodeSearch?postalcode={0}&country={1}&username=pristone",
			                zipPostalCode, country));

			        var cancellationTokenSource = new CancellationTokenSource();


			        var restResponse = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);


			        using (var stringReader = new StringReader(restResponse.Content))
			        using (var reader = new XmlTextReader(stringReader))
			        {
			            while (reader.Read())
			            {
			                if (reader.IsStartElement())
			                {
			                    switch (reader.Name)
			                    {
			                        case "postalcode":
			                            locationDictionary.Add("postalCode", reader.ReadString());
			                            break;
			                        case "name":
			                            locationDictionary.Add("cityName", reader.ReadString());
			                            break;
			                        case "countryCode":
			                            locationDictionary.Add("countryName", reader.ReadString());
			                            break;
			                        case "lat":
			                            locationDictionary.Add("lat", reader.ReadString());
			                            break;
			                        case "lng":
			                            locationDictionary.Add("lng", reader.ReadString());
			                            break;
			                        case "adminCode1":
			                            locationDictionary.Add("stateShortName", reader.ReadString());
			                            break;
			                        case "adminName1":
			                            locationDictionary.Add("stateLongName", reader.ReadString());
			                            break;
			                        case "adminCode2":
			                            locationDictionary.Add("stateCodeNo", reader.ReadString());
			                            break;
			                        case "adminName2":
			                            locationDictionary.Add("countyName", reader.ReadString());
			                            break;
			                        case "adminCode3":
			                            locationDictionary.Add("boroughCode", reader.ReadString());
			                            break;
			                        case "adminName3":
			                            locationDictionary.Add("boroughName", reader.ReadString());
			                            break;
			                    }
			                }
			            }
			        }
			    }
			}catch(Exception ex) {
				Console.WriteLine (ex.Message);
			}
			return locationDictionary;
		}
	}
}
