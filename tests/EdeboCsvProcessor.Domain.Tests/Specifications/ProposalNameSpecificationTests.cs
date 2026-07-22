using EdeboCsvProcessor.Domain.Entities;
using EdeboCsvProcessor.Domain.Specifications;
using EdeboCsvProcessor.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EdeboCsvProcessor.Domain.Tests.Specifications;

public class ProposalNameSpecificationTests
{
    private Application CreateApplicationWithProposal(string proposalName)
    {
        return new Application(
            "123",
            "Test",
            ApplicationDate.Parse("01.01.2025 10:00:00"),
            new CompetitiveProposal(proposalName),
            "Status");
    }

    [Theory]
    [InlineData("Комп'ютерна інженерія", "Комп'ютерна інженерія", true, true)]
    [InlineData("Комп'ютерна інженерія", "комп'ютерна інженерія", true, true)]
    [InlineData("Комп'ютерна інженерія", "Комп'ютерна", true, false)]
    [InlineData("Комп'ютерна інженерія", "Комп'ютерна", false, true)]
    [InlineData("Комп'ютерна інженерія", "Інженерія", false, true)]
    [InlineData("Комп'ютерна інженерія", "Медицина", false, false)]
    public void IsSatisfiedBy_ReturnsExpectedResult(
        string appProposal, string searchProposal, bool exactMatch, bool expected)
    {
        var app = CreateApplicationWithProposal(appProposal);
        var spec = new ProposalNameSpecification(searchProposal, exactMatch);

        spec.IsSatisfiedBy(app).Should().Be(expected);
    }
    
    [Fact]
    public void IsSatisfiedBy_EmptySearch_ReturnsTrue()
    {
        var app = CreateApplicationWithProposal("Комп'ютерна інженерія");
        var spec = new ProposalNameSpecification(null, false);
        
        spec.IsSatisfiedBy(app).Should().BeTrue();
    }
}
