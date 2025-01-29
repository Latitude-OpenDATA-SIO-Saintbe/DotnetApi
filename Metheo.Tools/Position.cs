/// <summary>
/// Provides functionality to retrieve and validate geographical positions.
/// </summary>

namespace Metheo.Tools;

public class Position
{
    /// <summary>
    ///     Retrieves and validates the geographical position based on the provided latitude and longitude.
    /// </summary>
    /// <param name="Latitude">The latitude value, which must be between -90 and 90.</param>
    /// <param name="Longitude">The longitude value, which must be between -180 and 180.</param>
    /// <returns>A tuple containing the validated latitude and longitude.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when the latitude or longitude is out of the valid range or is
    ///     NaN.
    /// </exception>
    public static (double lat, double lon) RetrievePosition(double Latitude, double Longitude)
    {
        if (double.IsNaN(Latitude) || double.IsNaN(Longitude) ||
            Latitude < -90 || Latitude > 90 || Longitude < -180 || Longitude > 180)
            throw new ArgumentOutOfRangeException("Invalid latitude or longitude value.");

        return (Latitude, Longitude);
    }
}