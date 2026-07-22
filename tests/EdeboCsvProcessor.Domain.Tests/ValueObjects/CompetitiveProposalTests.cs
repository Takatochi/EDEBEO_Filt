using EdeboCsvProcessor.Domain.ValueObjects;
using FluentAssertions;
using System;
using Xunit;

namespace EdeboCsvProcessor.Domain.Tests.ValueObjects;

public class CompetitiveProposalTests
{
    [Fact]
    public void Constructor_ValidName_SetsName()
    {
        var cp = new CompetitiveProposal("Комп'ютерна інженерія");
        cp.Name.Should().Be("Комп'ютерна інженерія");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_ThrowsArgumentException(string invalidName)
    {
        Action act = () => new CompetitiveProposal(invalidName);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Комп'ютерна інженерія", "Комп'ютерна інженерія", true, true)]
    [InlineData("Комп'ютерна інженерія", "Комп'ютерна", false, true)]
    [InlineData("Комп'ютерна інженерія", "Комп'ютерна", true, false)]
    [InlineData("Комп'ютерна інженерія", "Інженерія", false, true)]
    [InlineData("Комп'ютерна інженерія", "Медицина", false, false)]
    [InlineData("Комп'ютерна інженерія", "комп'ютерна інженерія", true, true)] // case insensitive exact match
    [InlineData("Комп'ютерна інженерія", "комп'ютерна", false, true)] // case insensitive partial match
    public void Matches_ReturnsExpectedResult(string proposalName, string searchName, bool exactMatch, bool expectedResult)
    {
        var cp = new CompetitiveProposal(proposalName);
        cp.Matches(searchName, exactMatch).Should().Be(expectedResult);
    }
}
