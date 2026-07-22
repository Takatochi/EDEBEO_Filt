using System;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Linq;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
using var reader = new StreamReader(@"C:\Users\nazer\OneDrive\Documents\EDBEO\EdeboCsvProcessor\SCV\ЕкспортЗаяв 22.07.2026 08.21.csv", Encoding.GetEncoding(1251));
using var csv = new CsvReader(reader, config);

csv.Read();
csv.ReadHeader();

var headers = csv.HeaderRecord;
Console.WriteLine($"Total headers: {headers.Length}");
foreach (var header in headers)
{
    if (header.Contains("ЗНО") || header.Contains("НМТ"))
    {
        Console.WriteLine($"Found header: {header}");
    }
}

if (csv.Read())
{
    for (int i = 0; i < headers.Length; i++)
    {
        var header = headers[i];
        if (header.Contains("ЗНО") || header.Contains("НМТ"))
        {
            var val = csv.GetField(i);
            Console.WriteLine($"{header}: {val}");
        }
    }
}
