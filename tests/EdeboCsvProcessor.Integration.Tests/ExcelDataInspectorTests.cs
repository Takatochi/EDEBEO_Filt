using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Xunit;
using Xunit.Abstractions;

namespace EdeboCsvProcessor.Integration.Tests;

public class ExcelDataInspectorTests
{
    private readonly ITestOutputHelper _output;

    public ExcelDataInspectorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void InspectHeaders()
    {
        var filePath = @"c:\Users\nazer\OneDrive\Documents\EDBEO\EdeboCsvProcessor\testTable\111.xlsx";
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        
        var firstRow = worksheet.Row(1);
        var cells = firstRow.CellsUsed();
        var headers = cells.Select(c => c.Value.ToString()).ToList();
        
        _output.WriteLine("Headers:");
        foreach (var header in headers)
        {
            _output.WriteLine($"- {header}");
        }

        var rowCount = worksheet.RowsUsed().Count();
        _output.WriteLine($"Total Rows Used: {rowCount}");

        foreach (var row in worksheet.RowsUsed().Skip(1).Take(5))
        {
            var rowValues = row.CellsUsed().Select(c => c.Value.ToString()).ToList();
            if (rowValues.Any())
            {
                _output.WriteLine($"Row {row.RowNumber()} Values:");
                foreach (var val in rowValues)
                {
                    _output.WriteLine($"- {val}");
                }
                break;
            }
        }
    }
}
