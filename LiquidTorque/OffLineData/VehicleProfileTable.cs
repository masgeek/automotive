using SQLite;

namespace liquidtorque.OffLineData
{
    public class VehicleProfileTable
    {
        /*
        objectId
engine
transmission
exteriorColor
interiorColor
priceOnRequest
vat
model
mileage
listPrice
carType
make
fuelType
status
username
subscriptionId
vin
condition
modelVariant
description
year
options
driveTrain
stockNumber
*/
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        [MaxLength(30), NotNull, Unique]
        public string ParseObjectId { get; set; }

        [NotNull]
        public string Make { get; set; }

        [NotNull]
        public string Model { get; set; }

        [NotNull]
        public string Username { get; set; }

        public string Engine { get; set; }
        public string VehicleJsonData { get; set; }
    }
}