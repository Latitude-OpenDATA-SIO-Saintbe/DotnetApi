namespace Metheo.Tools
{
    public class Calcul
    {
        public static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of the Earth in kilometers

            // Convert degrees to radians
            double dLat = (lat2 - lat1) * (Math.PI / 180);
            double dLon = (lon2 - lon1) * (Math.PI / 180);

            // Haversine formula
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in kilometers
        }
    }
}
