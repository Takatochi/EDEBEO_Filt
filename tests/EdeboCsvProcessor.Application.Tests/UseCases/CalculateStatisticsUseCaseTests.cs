using System;
using System.Collections.Generic;
using EdeboCsvProcessor.Application.UseCases;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;
using EdeboCsvProcessor.Domain.ValueObjects;
using Xunit;

namespace EdeboCsvProcessor.Application.Tests.UseCases;

public class CalculateStatisticsUseCaseTests
{
    [Fact]
    public void Execute_WithVariousApplications_CalculatesCorrectStatistics()
    {
        // Arrange
        var applications = new List<DomainApplication>
        {
            new DomainApplication("001", "Ivanov Ivan", ApplicationDate.Parse("01.07.2023 10:00:00"), new CompetitiveProposal("Computer Science"), "Допущено", new List<double> { 180, 190, 200 }),
            new DomainApplication("002", "Ivanov Ivan", ApplicationDate.Parse("02.07.2023 11:00:00"), new CompetitiveProposal("Software Engineering"), "Допущено", new List<double> { 180, 190, 200 }),
            new DomainApplication("003", "Petrov Petro", ApplicationDate.Parse("03.07.2023 12:00:00"), new CompetitiveProposal("Computer Science"), "Відмовлено", new List<double> { 140, 145 }),
            new DomainApplication("004", "Sidorov Sidir", ApplicationDate.Parse("04.07.2023 13:00:00"), new CompetitiveProposal("Mathematics"), "Рекомендовано", new List<double> { 160, 170 })
        };
        
        var useCase = new CalculateStatisticsUseCase();
        
        // Act
        var result = useCase.Execute(applications);
        
        // Assert
        Assert.Equal(4, result.TotalApplications);
        Assert.Equal(3, result.UniqueApplicants); // Ivanov Ivan is duplicated
        
        // Check Grants (Ivanov and Sidorov have >=150 in at least 2 subjects)
        Assert.Equal(3, result.GrantEligibleCount);
        Assert.Equal(1, result.GrantNotEligibleCount);
        
        // Check status distribution
        Assert.Equal(3, result.ApplicationsByStatus.Count);
        Assert.Equal(2, result.ApplicationsByStatus["Допущено"]);
        Assert.Equal(1, result.ApplicationsByStatus["Відмовлено"]);
        Assert.Equal(1, result.ApplicationsByStatus["Рекомендовано"]);
        
        // Check proposal distribution
        Assert.Equal(3, result.ApplicationsByProposal.Count);
        Assert.Equal(2, result.ApplicationsByProposal["Computer Science"]);
        Assert.Equal(1, result.ApplicationsByProposal["Software Engineering"]);
        Assert.Equal(1, result.ApplicationsByProposal["Mathematics"]);
    }
}
