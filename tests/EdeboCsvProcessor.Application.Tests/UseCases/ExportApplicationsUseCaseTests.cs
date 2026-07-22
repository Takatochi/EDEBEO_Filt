using EdeboCsvProcessor.Application.UseCases;
using EdeboCsvProcessor.Domain.Interfaces;
using EdeboCsvProcessor.Domain.ValueObjects;
using Moq;
using System.Collections.Generic;
using Xunit;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Application.Tests.UseCases;

public class ExportApplicationsUseCaseTests
{
    [Fact]
    public void Execute_CallsExportService()
    {
        // Arrange
        var mockExportService = new Mock<IExportService>();
        var apps = new List<DomainApplication>
        {
            new DomainApplication("1", "T1", ApplicationDate.Parse("01.01.2025 10:00:00"), new CompetitiveProposal("P1"), "S1")
        };
        var filePath = "output.xlsx";

        var useCase = new ExportApplicationsUseCase(mockExportService.Object);

        // Act
        useCase.Execute(apps, filePath);

        // Assert
        mockExportService.Verify(s => s.Export(apps, filePath, null), Times.Once);
    }
}
