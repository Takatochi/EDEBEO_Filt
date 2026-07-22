using System.Collections.Generic;
using System.Linq;
using EdeboCsvProcessor.Application.DTOs;
using EdeboCsvProcessor.Domain.Entities;
using EdeboCsvProcessor.Domain.Specifications;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;

namespace EdeboCsvProcessor.Application.UseCases;

public class FilterApplicationsUseCase
{
    public IEnumerable<DomainApplication> Execute(IEnumerable<DomainApplication> applications, FilterCriteria criteria)
    {
        var dateSpec = new DateRangeSpecification(criteria.DateFrom, criteria.DateTo);
        var proposalSpec = new ProposalNameSpecification(criteria.ProposalName);

        return applications
            .Where(app => dateSpec.IsSatisfiedBy(app) && proposalSpec.IsSatisfiedBy(app))
            .OrderBy(app => app.SubmissionDate);
    }
}
