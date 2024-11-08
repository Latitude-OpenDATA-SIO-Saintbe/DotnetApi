namespace Metheo.Tools;

using Metheo.Tools;

public class Calcul
{
    public static double CalculateHaversineDistance(double lat1, double lon1, string position)
    {
        var positionData = Position.RetrievePosition(position);
        
        const double R = 6371; // Radius of the Earth in kilometers
        var dLat = (positionData.lat - lat1) * (Math.PI / 180);
        var dLon = (positionData.lon - lon1) * (Math.PI / 180);
        var a = 
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(positionData.lat * (Math.PI / 180)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c; // Distance in kilometers
    }
}