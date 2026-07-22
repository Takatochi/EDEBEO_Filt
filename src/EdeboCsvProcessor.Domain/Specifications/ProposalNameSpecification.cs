using EdeboCsvProcessor.Domain.Entities;

namespace EdeboCsvProcessor.Domain.Specifications;

public class ProposalNameSpecification : ISpecification<Application>
{
    private readonly string? _proposalName;
    private readonly bool _exactMatch;

    public ProposalNameSpecification(string? proposalName, bool exactMatch = false)
    {
        _proposalName = proposalName;
        _exactMatch = exactMatch;
    }

    public bool IsSatisfiedBy(Application entity)
    {
        if (string.IsNullOrWhiteSpace(_proposalName))
            return true;

        return entity.Proposal.Matches(_proposalName, _exactMatch);
    }
}
