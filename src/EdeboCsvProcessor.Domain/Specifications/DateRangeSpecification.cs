using System;
using EdeboCsvProcessor.Domain.Entities;

namespace EdeboCsvProcessor.Domain.Specifications;

public class DateRangeSpecification : ISpecification<Application>
{
    private readonly DateTime? _from;
    private readonly DateTime? _to;

    public DateRangeSpecification(DateTime? from, DateTime? to)
    {
        _from = from;
        _to = to;
    }

    public bool IsSatisfiedBy(Application entity)
    {
        var appDate = entity.SubmissionDate.Value;

        if (_from.HasValue && appDate < _from.Value)
            return false;

        if (_to.HasValue && appDate > _to.Value)
            return false;

        return true;
    }
}
