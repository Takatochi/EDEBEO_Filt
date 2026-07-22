using EdeboCsvProcessor.Application.UseCases;
using EdeboCsvProcessor.Domain.Entities;
using EdeboCsvProcessor.Domain.Interfaces;
using EdeboCsvProcessor.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Application.Tests.UseCases;

public class ImportApplicationsUseCaseTests
{
    [Fact]
    public void Execute_CallsRepositoryAndReturnsApplications()
    {
        // Arrange
        var mockRepo = new Mock<IApplicationRepository>();
        var fakeApps = new List<DomainApplication>
        {
            new DomainApplication("1", "Test1", ApplicationDate.Parse("01.01.2025 10:00:00"), new CompetitiveProposal("P1"), "Status1"),
            new DomainApplication("2", "Test2", ApplicationDate.Parse("02.01.2025 10:00:00"), new CompetitiveProposal("P2"), "Status2")
        };

        var path = "test.csv";
        mockRepo.Setup(r => r.GetAll(path)).Returns(fakeApps);

        var useCase = new ImportApplicationsUseCase(mockRepo.Object);

        // Act
        var result = useCase.Execute(path).ToList();

        // Assert
        result.Should().HaveCount(2);
        mockRepo.Verify(r => r.GetAll(path), Times.Once);
    }
}
