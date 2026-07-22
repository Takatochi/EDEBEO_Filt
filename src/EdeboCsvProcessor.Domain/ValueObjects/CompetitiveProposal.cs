using System;

namespace EdeboCsvProcessor.Domain.ValueObjects;

public class CompetitiveProposal
{
    public string Name { get; }

    public CompetitiveProposal(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Proposal name cannot be empty", nameof(name));
        Name = name;
    }

    public bool Matches(string searchName, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(searchName)) return false;

        if (exactMatch)
        {
            return string.Equals(Name, searchName, StringComparison.OrdinalIgnoreCase);
        }

        return Name.Contains(searchName, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return string.Equals(Name, ((CompetitiveProposal)obj).Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }

    public override string ToString()
    {
        return Name;
    }
}
