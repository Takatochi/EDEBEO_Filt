using System.IO;
using System.Linq;
using System.Text;
using EdeboCsvProcessor.Infrastructure.Csv;
using FluentAssertions;
using Xunit;

namespace EdeboCsvProcessor.Integration.Tests;

public class CsvImportTests
{
    [Fact]
    public void GetAll_ValidCsv_ReturnsApplications()
    {
        // Arrange
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var testFilePath = "test_edebo.csv";
        var content = "Ід заявки;Вступник;Час додання заяви до ЄДЕБО;Назва КП;Статус заявки\n" +
                      "12345;Шевченко Тарас Григорович;09.03.2025 12:00:00;Комп'ютерна інженерія;Допущено\n" +
                      "12346;Франко Іван Якович;27.08.2025 10:15:30;Інженерія програмного забезпечення;Допущено\n";
        
        File.WriteAllText(testFilePath, content, Encoding.GetEncoding(1251));

        var repo = new CsvApplicationRepository();

        // Act
        var result = repo.GetAll(testFilePath).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].ApplicationNumber.Should().Be("12345");
        result[0].ApplicantName.Should().Be("Шевченко Тарас Григорович");
        result[0].Proposal.Name.Should().Be("Комп'ютерна інженерія");
        
        result[1].ApplicationNumber.Should().Be("12346");

        // Cleanup
        File.Delete(testFilePath);
    }
}
