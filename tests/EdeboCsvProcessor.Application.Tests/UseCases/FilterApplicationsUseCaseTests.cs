using EdeboCsvProcessor.Application.DTOs;
using EdeboCsvProcessor.Application.UseCases;
using EdeboCsvProcessor.Domain.Entities;
using EdeboCsvProcessor.Domain.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Application.Tests.UseCases;

public class FilterApplicationsUseCaseTests
{
    [Fact]
    public void Execute_AppliesFiltersAndSortsCorrectly()
    {
        // Arrange
        var apps = new List<DomainApplication>
        {
            new DomainApplication("1", "T1", ApplicationDate.Parse("05.01.2025 10:00:00"), new CompetitiveProposal("Медицина"), "S1"),
            new DomainApplication("2", "T2", ApplicationDate.Parse("01.01.2025 10:00:00"), new CompetitiveProposal("Комп'ютерна інженерія"), "S2"),
            new DomainApplication("3", "T3", ApplicationDate.Parse("02.01.2025 10:00:00"), new CompetitiveProposal("Комп'ютерна інженерія"), "S3"),
            new DomainApplication("4", "T4", ApplicationDate.Parse("10.01.2025 10:00:00"), new CompetitiveProposal("Комп'ютерна інженерія"), "S4")
        };

        var criteria = new FilterCriteria
        {
            DateFrom = new DateTime(2025, 1, 1),
            DateTo = new DateTime(2025, 1, 8),
            ProposalName = "інженерія"
        };

        var useCase = new FilterApplicationsUseCase();

        // Act
        var result = useCase.Execute(apps, criteria).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].ApplicationNumber.Should().Be("2"); // 01.01 - earliest
        result[1].ApplicationNumber.Should().Be("3"); // 02.01 - later
    }
}
