namespace Metheo.Tools;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// Provides utility methods for handling date ranges.
/// </summary>
public static class DateUtils
{
    // Precompiled regex patterns for better performance
    private static readonly Dictionary<string, Func<string, (DateTime, DateTime?)>> DateParsers = new()
    {
        { @"^\d{4}$", ParseYear },
        { @"^(0?[1-9]|1[0-2])-\d{4}$", ParseMonthYear },
        { @"^\d{2}-\d{2}-\d{4}$", ParseFullDate },
        { @"^\d{4}:\d{4}$", ParseYearRange },
        { @"^\d{2}-\d{4}:\d{2}-\d{4}$", ParseMonthYearRange },
        { @"^\d{2}-\d{2}-\d{4}:\d{2}-\d{2}-\d{4}$", ParseFullDateRange },
        { @"^\d{2}-\d{2}-\d{4}:\d{4}$", ParseDateToYear },
        { @"^\d{4}:\d{2}-\d{2}-\d{4}$", ParseYearToDate },
        { @"^\d{2}-\d{4}:\d{2}-\d{2}-\d{4}$", ParseMonthYearToDate },
        { @"^\d{2}-\d{2}-\d{4}:\d{2}-\d{4}$", ParseDateToMonthYear },
        { @"^\d{4}:\d{2}-\d{4}$", ParseYearToMonthYear },
        { @"^\d{2}-\d{4}:\d{4}$", ParseMonthYearToYear }
    };
    
    /// <summary>
    /// Parses a date string and returns a tuple containing the start date and an optional end date.
    /// </summary>
    /// <param name="date">The date string to parse.</param>
    /// <returns>A tuple containing the start date and an optional end date.</returns>
    /// <exception cref="FormatException">Thrown when the date string is in an invalid format.</exception>
    public static bool TryParseDateRange(string dateRange, out DateTime startDate, out DateTime? endDate)
    {
        startDate = default;
        endDate = default;

        if (string.IsNullOrWhiteSpace(dateRange))
            return false;

        foreach (var parser in DateParsers)
        {
            if (Regex.IsMatch(dateRange, parser.Key))
            {
                try
                {
                    (startDate, endDate) = parser.Value(dateRange);
                    return true;
                }
                catch
                {
                    // Ignore exceptions and continue to the next parser
                }
            }
        }

        return false;
    }
    
    // Parsing methods for specific formats
    private static (DateTime, DateTime?) ParseYear(string date)
    {
        int year = int.Parse(date);
        return (new DateTime(year, 1, 1), new DateTime(year, 12, 31));
    }

    private static (DateTime, DateTime?) ParseMonthYear(string date)
    {
        var parts = date.Split('-');
        int month = int.Parse(parts[0]);
        int year = int.Parse(parts[1]);
        return (new DateTime(year, month, 1), new DateTime(year, month, DateTime.DaysInMonth(year, month)));
    }

    private static (DateTime, DateTime?) ParseFullDate(string date)
    {
        var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        return (parsedDate, parsedDate);
    }

    private static (DateTime, DateTime?) ParseYearRange(string date)
    {
        var years = date.Split(':');
        return (new DateTime(int.Parse(years[0]), 1, 1), new DateTime(int.Parse(years[1]), 12, 31));
    }

    private static (DateTime, DateTime?) ParseMonthYearRange(string date)
    {
        var parts = date.Split(':');
        var startParts = parts[0].Split('-');
        var endParts = parts[1].Split('-');
        var startDate = new DateTime(int.Parse(startParts[1]), int.Parse(startParts[0]), 1);
        var endDate = new DateTime(int.Parse(endParts[1]), int.Parse(endParts[0]), DateTime.DaysInMonth(int.Parse(endParts[1]), int.Parse(endParts[0])));
        return (startDate, endDate);
    }

    private static (DateTime, DateTime?) ParseFullDateRange(string date)
    {
        var parts = date.Split(':');
        var startDate = DateTime.ParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture);
        var endDate = DateTime.ParseExact(parts[1], "dd-MM-yyyy", CultureInfo.InvariantCulture);
        return (startDate, endDate);
    }

    private static (DateTime, DateTime?) ParseDateToYear(string date)
    {
        var parts = date.Split(':');
        var startDate = DateTime.ParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture);
        var endDate = new DateTime(int.Parse(parts[1]), 12, 31);
        return (startDate, endDate);
    }

    private static (DateTime, DateTime?) ParseYearToDate(string date)
    {
        var parts = date.Split(':');
        var startDate = new DateTime(int.Parse(parts[0]), 1, 1);
        var endDate = DateTime.ParseExact(parts[1], "dd-MM-yyyy", CultureInfo.InvariantCulture);
        return (startDate, endDate);
    }

    private static (DateTime, DateTime?) ParseMonthYearToDate(string date)
    {
        var parts = date.Split(':');
        var startParts = parts[0].Split('-');
        var startDate = new DateTime(int.Parse(startParts[1]), int.Parse(startParts[0]), 1);
        var endDate = DateTime.ParseExact(parts[1], "dd-MM-yyyy", CultureInfo.InvariantCulture);
        return (startDate, endDate);
    }

    private static (DateTime, DateTime?) ParseDateToMonthYear(string date)
    {
        var parts = date.Split(':');
        var startDate = DateTime.ParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture);
        var endParts = parts[1].Split('-');
        var endDate = new DateTime(int.Parse(endParts[1]), int.Parse(endParts[0]), DateTime.DaysInMonth(int.Parse(endParts[1]), int.Parse(endParts[0])));
        return (startDate, endDate);
    }

    private static (DateTime, DateTime?) ParseYearToMonthYear(string date)
    {
        var parts = date.Split(':');
        var startDate = new DateTime(int.Parse(parts[0]), 1, 1);
        var endParts = parts[1].Split('-');
        var endDate = new DateTime(int.Parse(endParts[1]), int.Parse(endParts[0]), DateTime.DaysInMonth(int.Parse(endParts[1]), int.Parse(endParts[0])));
        return (startDate, endDate);
    }

    private static (DateTime, DateTime?) ParseMonthYearToYear(string date)
    {
        var parts = date.Split(':');
        var startParts = parts[0].Split('-');
        var startDate = new DateTime(int.Parse(startParts[1]), int.Parse(startParts[0]), 1);
        var endDate = new DateTime(int.Parse(parts[1]), 12, 31);
        return (startDate, endDate);
    }
}
