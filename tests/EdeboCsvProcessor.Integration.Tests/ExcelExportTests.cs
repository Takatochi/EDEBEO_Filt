using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using EdeboCsvProcessor.Domain.ValueObjects;
using EdeboCsvProcessor.Infrastructure.Excel;
using FluentAssertions;
using Xunit;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Integration.Tests;

public class ExcelExportTests
{
    [Fact]
    public void Export_CreatesValidExcelFile()
    {
        // Arrange
        var service = new ExcelExportService();
        var apps = new List<DomainApplication>
        {
            new DomainApplication("123", "Тест Тестович", ApplicationDate.Parse("01.01.2025 12:00:00"), new CompetitiveProposal("КП"), "Допущено")
        };
        var filePath = "test_output.xlsx";

        if (File.Exists(filePath)) File.Delete(filePath);

        // Act
        service.Export(apps, filePath);

        // Assert
        File.Exists(filePath).Should().BeTrue();

        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        
        worksheet.Cell(1, 1).Value.ToString().Should().Be("Номер заяви");
        worksheet.Cell(2, 1).Value.ToString().Should().Be("123");
        worksheet.Cell(2, 2).Value.ToString().Should().Be("Тест Тестович");

        // Cleanup
        File.Delete(filePath);
    }
}
