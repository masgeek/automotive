namespace liquidtorque.OffLineData
{
    public class VehicleProfile
    {
        //var vehicleTuple = new Tuple<string, string, string, string, Uri>(profile.ObjectId, make,listPrice, model, imgUrl);
        /// <summary>
        /// Vehicle object id
        /// </summary>
        public string Item1 { get; set; } //object id
        /// <summary>
        /// Vehicle make
        /// </summary>
        public string Item2 { get; set; } //make
        /// <summary>
        /// vehicle model
        /// </summary>
        public string Item3 { get; set; } //model
        /// <summary>
        /// Vehicle price
        /// </summary>
        public string Item4 { get; set; } //price
        /// <summary>
        /// Profile image url
        /// </summary>
        public string Item5 { get; set; } //url

        /// <summary>
        /// Vehicle owner username
        /// </summary>
        public string Item6 { get; set; } //username
    }
}