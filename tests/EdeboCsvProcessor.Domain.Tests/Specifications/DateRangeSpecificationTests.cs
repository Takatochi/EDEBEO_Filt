using EdeboCsvProcessor.Domain.Entities;
using EdeboCsvProcessor.Domain.Specifications;
using EdeboCsvProcessor.Domain.ValueObjects;
using FluentAssertions;
using System;
using Xunit;

namespace EdeboCsvProcessor.Domain.Tests.Specifications;

public class DateRangeSpecificationTests
{
    private Application CreateApplicationWithDate(string dateStr)
    {
        return new Application(
            "123",
            "Test",
            ApplicationDate.Parse(dateStr),
            new CompetitiveProposal("Test Proposal"),
            "Status");
    }

    [Theory]
    [InlineData("05.01.2025 10:00:00", "01.01.2025 00:00:00", "10.01.2025 00:00:00", true)]
    [InlineData("01.01.2025 00:00:00", "01.01.2025 00:00:00", "10.01.2025 00:00:00", true)]
    [InlineData("10.01.2025 00:00:00", "01.01.2025 00:00:00", "10.01.2025 00:00:00", true)]
    [InlineData("31.12.2024 23:59:59", "01.01.2025 00:00:00", "10.01.2025 00:00:00", false)]
    [InlineData("10.01.2025 00:00:01", "01.01.2025 00:00:00", "10.01.2025 00:00:00", false)]
    public void IsSatisfiedBy_WithBothDates_ReturnsExpectedResult(
        string appDateStr, string fromStr, string toStr, bool expected)
    {
        var app = CreateApplicationWithDate(appDateStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);
        var spec = new DateRangeSpecification(from, to);

        spec.IsSatisfiedBy(app).Should().Be(expected);
    }

    [Fact]
    public void IsSatisfiedBy_OnlyFromDate_ReturnsExpectedResult()
    {
        var spec = new DateRangeSpecification(new DateTime(2025, 1, 1), null);
        
        spec.IsSatisfiedBy(CreateApplicationWithDate("05.01.2025 10:00:00")).Should().BeTrue();
        spec.IsSatisfiedBy(CreateApplicationWithDate("31.12.2024 23:59:59")).Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_OnlyToDate_ReturnsExpectedResult()
    {
        var spec = new DateRangeSpecification(null, new DateTime(2025, 1, 10));
        
        spec.IsSatisfiedBy(CreateApplicationWithDate("05.01.2025 10:00:00")).Should().BeTrue();
        spec.IsSatisfiedBy(CreateApplicationWithDate("11.01.2025 10:00:00")).Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_NoDates_ReturnsTrue()
    {
        var spec = new DateRangeSpecification(null, null);
        
        spec.IsSatisfiedBy(CreateApplicationWithDate("05.01.2025 10:00:00")).Should().BeTrue();
    }
}
