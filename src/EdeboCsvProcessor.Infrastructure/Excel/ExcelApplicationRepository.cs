using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using EdeboCsvProcessor.Domain.Interfaces;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Infrastructure.Excel;

public class ExcelApplicationRepository : IApplicationRepository
{
    public IEnumerable<DomainApplication> GetAll(string filePath)
    {
        var applications = new List<DomainApplication>();
        
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        
        var headersRow = worksheet.Row(1);
        var headers = new Dictionary<int, string>();
        
        foreach (var cell in headersRow.CellsUsed())
        {
            headers[cell.Address.ColumnNumber] = cell.Value.ToString().Trim();
        }

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var dict = new Dictionary<string, string>();
            foreach (var kvp in headers)
            {
                dict[kvp.Value] = row.Cell(kvp.Key).Value.ToString();
            }

            if (dict.Values.All(string.IsNullOrWhiteSpace))
                continue;

            applications.Add(new DomainApplication(dict));
        }

        return applications;
    }
}
