using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using EdeboCsvProcessor.Domain.Interfaces;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Infrastructure.Csv;

public class CsvApplicationRepository : IApplicationRepository
{
    public IEnumerable<DomainApplication> GetAll(string filePath)
    {
        // For Windows-1251
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";", // Common for EDEBO CSVs
            HasHeaderRecord = true,
            MissingFieldFound = null, // Ignore missing fields
            BadDataFound = null
        };

        var encoding = Encoding.GetEncoding(1251);

        using var reader = new StreamReader(filePath, encoding);
        using var csv = new CsvReader(reader, config);
        
        var applications = new List<DomainApplication>();
        
        if (csv.Read())
        {
            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            while (csv.Read())
            {
                var dict = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    dict[headers[i]] = csv.GetField(i);
                }
                
                applications.Add(new DomainApplication(dict));
            }
        }

        return applications;
    }
}
