namespace Metheo.Tools;

public class Position
{
    public static (double lat, double lon) RetrievePosition(string position)
    {
        var coordinates = position.Split(',');
        if (coordinates.Length != 2 ||
            !double.TryParse(coordinates[0], out double lat) ||
            !double.TryParse(coordinates[1], out double lon))
        {
            throw new ArgumentException("Invalid position format. Expected 'latitude,longitude'.");
        }
        
        return (lat, lon);
    }
}