namespace Metheo.Tools;

using System;
using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// Provides utility methods for handling date ranges.
/// </summary>
public static class DateUtils
{
    /// <summary>
    /// Parses a date string and returns a tuple containing the start date and an optional end date.
    /// </summary>
    /// <param name="date">The date string to parse. Supported formats include:
    /// <list type="bullet">
    /// <item><description>YYYY (e.g., "2024")</description></item>
    /// <item><description>MM-YYYY (e.g., "03-2024")</description></item>
    /// <item><description>DD-MM-YYYY (e.g., "25-03-2024")</description></item>
    /// <item><description>YYYY:YYYY (e.g., "2025:2026")</description></item>
    /// <item><description>MM-YYYY:MM-YYYY (e.g., "03-2025:02-2026")</description></item>
    /// <item><description>DD-MM-YYYY:DD-MM-YYYY (e.g., "01-02-2024:08-03-2024")</description></item>
    /// <item><description>DD-MM-YYYY:YYYY (e.g., "01-02-2024:2024")</description></item>
    /// <item><description>YYYY:DD-MM-YYYY (e.g., "2024:01-03-2024")</description></item>
    /// <item><description>MM-YYYY:DD-MM-YYYY (e.g., "02-2024:01-03-2024")</description></item>
    /// <item><description>DD-MM-YYYY:MM-YYYY (e.g., "01-02-2024:03-2024")</description></item>
    /// <item><description>YYYY:MM-YYYY (e.g., "2024:03-2024")</description></item>
    /// <item><description>MM-YYYY:YYYY (e.g., "03-2024:2024")</description></item>
    /// </list>
    /// </param>
    /// <returns>A tuple containing the start date and an optional end date.</returns>
    /// <exception cref="FormatException">Thrown when the date string is in an invalid format.</exception>
    public static (DateTime startDate, DateTime? endDate) GetDateRange(string date)
    {
        DateTime startDate;
        DateTime? endDate = null;

        try
        {
            // Regex patterns for different formats
            if (Regex.IsMatch(date, @"^\d{4}$")) // Matches "2024"
            {
                startDate = new DateTime(int.Parse(date), 1, 1);
            }
            else if (Regex.IsMatch(date, @"^(0?[1-9]|1[0-2])-\d{4}$")) // Matches "03-2024"
            {
                var parts = date.Split('-');
                startDate = new DateTime(int.Parse(parts[1]), int.Parse(parts[0]), 1);
            }
            else if (Regex.IsMatch(date, @"^\d{2}-\d{2}-\d{4}$")) // Matches "25-03-2024"
            {
                startDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
            else if (Regex.IsMatch(date, @"^\d{4}:\d{4}$")) // Matches "2025:2026"
            {
                var years = date.Split(':');
                startDate = new DateTime(int.Parse(years[0]), 1, 1);
                endDate = new DateTime(int.Parse(years[1]), 12, 31);
            }
            else if (Regex.IsMatch(date, @"^\d{2}-\d{4}:\d{2}-\d{4}$")) // Matches "03-2025:02-2026"
            {
                var months = date.Split(':');
                var startParts = months[0].Split('-');
                var endParts = months[1].Split('-');
                startDate = new DateTime(int.Parse(startParts[1]), int.Parse(startParts[0]), 1);
                endDate = new DateTime(int.Parse(endParts[1]), int.Parse(endParts[0]), DateTime.DaysInMonth(int.Parse(endParts[1]), int.Parse(endParts[0])));
            }
            else if (Regex.IsMatch(date, @"^\d{2}-\d{2}-\d{4}:\d{2}-\d{2}-\d{4}$")) // Matches "01-02-2024:08-03-2024"
            {
                var dates = date.Split(':');
                startDate = DateTime.ParseExact(dates[0], "dd-MM-yyyy", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(dates[1], "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
            else if (Regex.IsMatch(date, @"^\d{2}-\d{2}-\d{4}:\d{4}$")) // Matches "01-02-2024:2024"
            {
                var parts = date.Split(':');
                startDate = DateTime.ParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture);
                endDate = new DateTime(int.Parse(parts[1]), 12, 31); // End of the year
            }
            else if (Regex.IsMatch(date, @"^\d{4}:\d{2}-\d{2}-\d{4}$")) // Matches "2024:01-03-2024"
            {
                var parts = date.Split(':');
                startDate = new DateTime(int.Parse(parts[0]), 1, 1);
                endDate = DateTime.ParseExact(parts[1], "dd-MM-yyyy", CultureInfo.InvariantCulture); // Expecting the end date to be a full date
            }
            else if (Regex.IsMatch(date, @"^\d{2}-\d{4}:\d{2}-\d{2}-\d{4}$")) // Matches "02-2024:01-03-2024"
            {
                var months = date.Split(':');
                var startParts = months[0].Split('-');
                var endParts = months[1].Split('-');
                startDate = new DateTime(int.Parse(startParts[1]), int.Parse(startParts[0]), 1);
                endDate = DateTime.ParseExact(endParts[0] + "-" + endParts[1] + "-" + endParts[2], "dd-MM-yyyy", CultureInfo.InvariantCulture); // Full date
            }
            else if (Regex.IsMatch(date, @"^\d{2}-\d{2}-\d{4}:\d{2}-\d{4}$")) // Matches "01-02-2024:03-2024"
            {
                var parts = date.Split(':');
                startDate = DateTime.ParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var endParts = parts[1].Split('-');
                endDate = new DateTime(int.Parse(endParts[1]), int.Parse(endParts[0]), DateTime.DaysInMonth(int.Parse(endParts[1]), int.Parse(endParts[0]))); // Last day of the month
            }
            else if (Regex.IsMatch(date, @"^\d{4}:\d{2}-\d{4}$")) // Matches "2024:03-2024"
            {
                var parts = date.Split(':');
                var startParts = parts[0].Split('-');
                startDate = new DateTime(int.Parse(startParts[0]), 1, 1);
                var monthYearParts = parts[1].Split('-');
                if (monthYearParts.Length == 2)
                {
                    int month = int.Parse(monthYearParts[0]);
                    int year = int.Parse(monthYearParts[1]);
                    endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)); // Last day of the month
                }
            }
            else if (Regex.IsMatch(date, @"^\d{2}-\d{4}:\d{4}$")) // Matches "03-2024:2024"
            {
                var parts = date.Split(':');
                var startParts = parts[0].Split('-');
                startDate = new DateTime(int.Parse(startParts[1]), int.Parse(startParts[0]), 1);
                endDate = new DateTime(int.Parse(parts[1]), 12, 31); // End of the year
            }
            else
            {
                throw new FormatException("Invalid date format");
            }
        }
        catch (FormatException ex)
        {
            throw new FormatException("Invalid date format", ex);
        }

        return (startDate, endDate);
    }
}
