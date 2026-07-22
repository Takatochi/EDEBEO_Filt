using System;

namespace EdeboCsvProcessor.Application.DTOs;

public class FilterCriteria
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ProposalName { get; set; }
}
