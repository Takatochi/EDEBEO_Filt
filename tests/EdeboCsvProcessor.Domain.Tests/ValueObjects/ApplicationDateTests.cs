using EdeboCsvProcessor.Domain.ValueObjects;
using FluentAssertions;
using System;
using Xunit;

namespace EdeboCsvProcessor.Domain.Tests.ValueObjects;

public class ApplicationDateTests
{
    [Theory]
    [InlineData("01.01.2025 10:30:00", 2025, 1, 1, 10, 30, 0)]
    [InlineData("31.12.2025 23:59:59", 2025, 12, 31, 23, 59, 59)]
    public void Parse_ValidString_ReturnsCorrectDate(string dateString, int year, int month, int day, int hour, int minute, int second)
    {
        var appDate = ApplicationDate.Parse(dateString);
        appDate.Value.Should().Be(new DateTime(year, month, day, hour, minute, second));
    }

    [Fact]
    public void Parse_InvalidString_ThrowsArgumentException()
    {
        Action act = () => ApplicationDate.Parse("invalid-date");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CompareTo_WorksCorrectly()
    {
        var earlier = ApplicationDate.Parse("01.01.2025 10:00:00");
        var later = ApplicationDate.Parse("01.01.2025 12:00:00");

        earlier.CompareTo(later).Should().BeLessThan(0);
        later.CompareTo(earlier).Should().BeGreaterThan(0);
        earlier.CompareTo(earlier).Should().Be(0);
    }
}
