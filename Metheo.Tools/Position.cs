namespace Metheo.Tools
{
    public class Position
    {
        public static (double lat, double lon) RetrievePosition(double Latitude, double Longitude)
        {
            if (double.IsNaN(Latitude) || double.IsNaN(Longitude) ||
                Latitude < -90 || Latitude > 90 || Longitude < -180 || Longitude > 180)
            {
                throw new ArgumentOutOfRangeException("Invalid latitude or longitude value.");
            }

            return (Latitude, Longitude);
        }
    }
}
