namespace liquidtorque.ComponentClasses
{
    /// <summary>
    /// This class allow us to share the filter string globally
    /// </summary>
    public static class VehicleFilterStrings
    {
        public static string Model { get; set; }
        public static string Make { get; set; }
        public static string Year { get; set; }

        public static bool FiltersSet { get; set; }
    }
}