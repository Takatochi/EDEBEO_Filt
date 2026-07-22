using System;
using System.Globalization;
using EdeboCsvProcessor.Application.DTOs;
using EdeboCsvProcessor.Application.Services;
using EdeboCsvProcessor.Application.UseCases;
using EdeboCsvProcessor.Domain.Interfaces;
using EdeboCsvProcessor.Infrastructure.Csv;
using EdeboCsvProcessor.Infrastructure.Excel;

namespace EdeboCsvProcessor.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;
        DateTime? dateFrom = null;
        DateTime? dateTo = null;
        string? proposal = null;

        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--input":
                        inputPath = args[++i];
                        break;
                    case "--output":
                        outputPath = args[++i];
                        break;
                    case "--date-from":
                        dateFrom = DateTime.ParseExact(args[++i], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        break;
                    case "--date-to":
                        var dt = DateTime.ParseExact(args[++i], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        dateTo = dt.AddDays(1).AddTicks(-1); // include the entire day
                        break;
                    case "--proposal":
                        proposal = args[++i];
                        break;
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Missing value for a parameter.");
            PrintUsage();
            return;
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid date format. Expected dd.MM.yyyy");
            PrintUsage();
            return;
        }

        if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
        {
            PrintUsage();
            return;
        }

        var filterCriteria = new FilterCriteria
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            ProposalName = proposal
        };

        IApplicationRepository repo;
        if (inputPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || inputPath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
        {
            repo = new ExcelApplicationRepository();
        }
        else
        {
            repo = new CsvApplicationRepository();
        }

        var exportService = new ExcelExportService();

        var processingService = new ApplicationProcessingService(
            new ImportApplicationsUseCase(repo),
            new FilterApplicationsUseCase(),
            new ExportApplicationsUseCase(exportService)
        );

        try
        {
            Console.WriteLine($"Processing {inputPath}...");
            processingService.Process(inputPath, outputPath, filterCriteria);
            Console.WriteLine($"Successfully exported to {outputPath}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage: EdeboCsvProcessor.exe --input <data.csv> --output <result.xlsx> [--date-from dd.MM.yyyy] [--date-to dd.MM.yyyy] [--proposal \"name\"]");
    }
}
