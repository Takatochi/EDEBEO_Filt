using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using EdeboCsvProcessor.Domain.Interfaces;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Infrastructure.Excel;

public class ExcelExportService : IExportService
{
    public void Export(IEnumerable<DomainApplication> applications, string filePath, IList<string>? orderedColumns = null)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Заяви");

        var appsList = applications.ToList();
        if (appsList.Count == 0)
        {
            workbook.SaveAs(filePath);
            return;
        }

        // Get all unique keys from all applications if not provided by UI
        var allKeys = orderedColumns ?? appsList.SelectMany(a => a.RawData.Keys).Distinct().ToList();
        
        var exportColumns = new System.Collections.Generic.List<string>(allKeys);

        // If the UI didn't already include the grant column (e.g., if we fall back to raw data), insert it
        if (!exportColumns.Contains("Можливий грант"))
        {
            int phoneIndex = exportColumns.FindIndex(k => k.IndexOf("телефон", System.StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("контактний номер", System.StringComparison.OrdinalIgnoreCase) >= 0);
            int grantIndex = phoneIndex >= 0 ? phoneIndex + 1 : exportColumns.Count;
            exportColumns.Insert(grantIndex, "Можливий грант");
        }

        // Write headers
        for (int i = 0; i < exportColumns.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = exportColumns[i];
        }

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;

        // Write Data
        for (int i = 0; i < appsList.Count; i++)
        {
            var app = appsList[i];
            var row = i + 2;

            for (int j = 0; j < exportColumns.Count; j++)
            {
                var colName = exportColumns[j];
                if (colName == "Можливий грант")
                {
                    worksheet.Cell(row, j + 1).Value = app.GrantStatus;
                    if (app.GrantStatus == "Так")
                    {
                        worksheet.Cell(row, j + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    }
                }
                else
                {
                    if (app.RawData.TryGetValue(colName, out var val))
                    {
                        worksheet.Cell(row, j + 1).Value = val;
                    }
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
    }
}
