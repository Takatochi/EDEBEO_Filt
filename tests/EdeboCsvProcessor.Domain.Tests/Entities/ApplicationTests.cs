using EdeboCsvProcessor.Domain.Entities;
using EdeboCsvProcessor.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EdeboCsvProcessor.Domain.Tests.Entities;

public class ApplicationTests
{
    [Fact]
    public void CreateApplication_ValidData_ShouldInitializeCorrectly()
    {
        // Arrange
        var number = "123456";
        var name = "Іванов Іван Іванович";
        var date = ApplicationDate.Parse("01.01.2025 15:00:00");
        var proposal = new CompetitiveProposal("Комп'ютерна інженерія");
        var status = "Допущено до конкурсу";

        // Act
        var application = new Application(number, name, date, proposal, status);

        // Assert
        application.ApplicationNumber.Should().Be(number);
        application.ApplicantName.Should().Be(name);
        application.SubmissionDate.Should().Be(date);
        application.Proposal.Should().Be(proposal);
        application.Status.Should().Be(status);
    }
}
