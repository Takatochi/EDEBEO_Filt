using System.IO;
using System.Linq;
using ClosedXML.Excel;
using EdeboCsvProcessor.Infrastructure.Excel;
using FluentAssertions;
using Xunit;

namespace EdeboCsvProcessor.Integration.Tests;

public class UserSampleIntegrationTests
{
    [Fact]
    public void AddDataToUserSampleAndTestImport()
    {
        // 1. Setup paths
        var originalPath = @"c:\Users\nazer\OneDrive\Documents\EDBEO\EdeboCsvProcessor\testTable\111.xlsx";
        var testPath = @"c:\Users\nazer\OneDrive\Documents\EDBEO\EdeboCsvProcessor\testTable\111_test.xlsx";
        
        File.Copy(originalPath, testPath, true);

        // 2. Додаємо туди інформацію, як просив користувач
        using (var workbook = new XLWorkbook(testPath))
        {
            var worksheet = workbook.Worksheet(1);
            
            // Знайдемо колонки, щоб записати дані у правильні місця
            var headersRow = worksheet.Row(1);
            int appNumCol = 0, applicantCol = 0, dateCol = 0, proposalCol = 0, statusCol = 0;
            
            foreach (var cell in headersRow.CellsUsed())
            {
                var header = cell.Value.ToString().Trim();
                if (header == "Ід заявки") appNumCol = cell.Address.ColumnNumber;
                if (header == "Вступник") applicantCol = cell.Address.ColumnNumber;
                if (header == "Час додання заяви до ЄДЕБО") dateCol = cell.Address.ColumnNumber;
                if (header == "Назва КП") proposalCol = cell.Address.ColumnNumber;
                if (header == "Статус заявки") statusCol = cell.Address.ColumnNumber;
            }

            // Додаємо 2 рядки тестових даних
            worksheet.Cell(2, appNumCol).Value = "99991";
            worksheet.Cell(2, applicantCol).Value = "Леся Українка";
            worksheet.Cell(2, dateCol).Value = "01.07.2025 15:30:00";
            worksheet.Cell(2, proposalCol).Value = "Кібербезпека";
            worksheet.Cell(2, statusCol).Value = "Допущено";

            worksheet.Cell(3, appNumCol).Value = "99992";
            worksheet.Cell(3, applicantCol).Value = "Григорій Сковорода";
            worksheet.Cell(3, dateCol).Value = "15.08.2025 09:00:00";
            worksheet.Cell(3, proposalCol).Value = "Філософія";
            worksheet.Cell(3, statusCol).Value = "Допущено";

            workbook.Save();
        }

        // 3. Здійснюємо інтеграційне тестування імпорту
        var repo = new ExcelApplicationRepository();
        var applications = repo.GetAll(testPath).ToList();

        // 4. Перевірки (Asserts)
        applications.Should().HaveCount(2);
        
        var app1 = applications[0];
        app1.ApplicationNumber.Should().Be("99991");
        app1.ApplicantName.Should().Be("Леся Українка");
        app1.Proposal.Name.Should().Be("Кібербезпека");
        
        var app2 = applications[1];
        app2.ApplicantName.Should().Be("Григорій Сковорода");

        // Прибирання
        File.Delete(testPath);
    }
}
