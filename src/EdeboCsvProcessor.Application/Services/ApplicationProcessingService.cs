using EdeboCsvProcessor.Application.DTOs;
using EdeboCsvProcessor.Application.UseCases;

namespace EdeboCsvProcessor.Application.Services;

public class ApplicationProcessingService
{
    private readonly ImportApplicationsUseCase _importUseCase;
    private readonly FilterApplicationsUseCase _filterUseCase;
    private readonly ExportApplicationsUseCase _exportUseCase;

    public ApplicationProcessingService(
        ImportApplicationsUseCase importUseCase,
        FilterApplicationsUseCase filterUseCase,
        ExportApplicationsUseCase exportUseCase)
    {
        _importUseCase = importUseCase;
        _filterUseCase = filterUseCase;
        _exportUseCase = exportUseCase;
    }

    public void Process(string inputFilePath, string outputFilePath, FilterCriteria criteria)
    {
        var allApps = _importUseCase.Execute(inputFilePath);
        var filteredApps = _filterUseCase.Execute(allApps, criteria);
        _exportUseCase.Execute(filteredApps, outputFilePath);
    }
}
