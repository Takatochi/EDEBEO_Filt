using System.Collections.Generic;
using EdeboCsvProcessor.Domain.Interfaces;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Application.UseCases;

public class ExportApplicationsUseCase
{
    private readonly IExportService _exportService;

    public ExportApplicationsUseCase(IExportService exportService)
    {
        _exportService = exportService;
    }

    public void Execute(IEnumerable<DomainApplication> applications, string filePath, IList<string>? orderedColumns = null)
    {
        _exportService.Export(applications, filePath, orderedColumns);
    }
}
