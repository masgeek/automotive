using System.Collections.Generic;
using liquidtorque.DataAccessLayer;

namespace DataAccessLayer
{
	public class UserInventory
	{
		public string userID { get; set; }

		public List<Car> carInventory { get; set; }

		public UserInventory ()
		{
			carInventory = new List<Car> ();
		}

		public void addCar(Car car)
		{
			carInventory.Add (car);
		}
		public void ClearInventory()
		{
			carInventory.Clear ();
		}
			
	}
}

