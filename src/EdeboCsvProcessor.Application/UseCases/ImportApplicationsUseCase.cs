using System.Collections.Generic;
using EdeboCsvProcessor.Domain.Entities;
using EdeboCsvProcessor.Domain.Interfaces;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Application.UseCases;

public class ImportApplicationsUseCase
{
    private readonly IApplicationRepository _repository;

    public ImportApplicationsUseCase(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<DomainApplication> Execute(string filePath)
    {
        return _repository.GetAll(filePath);
    }
}
