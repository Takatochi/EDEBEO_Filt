using System;
using System.Globalization;

namespace EdeboCsvProcessor.Domain.ValueObjects;

public class ApplicationDate : IComparable<ApplicationDate>
{
    public DateTime Value { get; }

    private ApplicationDate(DateTime value)
    {
        Value = value;
    }

    public static ApplicationDate Parse(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            throw new ArgumentException("Date string cannot be empty", nameof(dateString));

        // Format is dd.MM.yyyy HH:mm:ss or yyyy-MM-dd HH:mm:ss
        string[] formats = { "dd.MM.yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss" };
        if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return new ApplicationDate(date);
        }

        // Add fallback format parsing just in case EDEBO format slightly varies
        if (DateTime.TryParse(dateString, new CultureInfo("uk-UA"), DateTimeStyles.None, out var fallbackDate))
        {
            return new ApplicationDate(fallbackDate);
        }

        throw new ArgumentException($"Unable to parse date: {dateString}", nameof(dateString));
    }

    public int CompareTo(ApplicationDate? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Value.Equals(((ApplicationDate)obj).Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString("dd.MM.yyyy HH:mm:ss");
    }
}
