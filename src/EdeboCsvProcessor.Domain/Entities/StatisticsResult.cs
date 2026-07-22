using System.Collections.Generic;

namespace EdeboCsvProcessor.Domain.Entities;

public class StatisticsResult
{
    public int TotalApplications { get; set; }
    public int UniqueApplicants { get; set; }
    public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();
    public Dictionary<string, int> ApplicationsByProposal { get; set; } = new();
    public int GrantEligibleCount { get; set; }
    public int GrantNotEligibleCount { get; set; }
    
    public int TodayApplications { get; set; }
    
    public int TotalOriginals { get; set; }
    public int TodayOriginals { get; set; }
    
    public int TotalContracts { get; set; }
    public int TodayContracts { get; set; }
    
    public int TotalContractApplications { get; set; }
    public int TodayContractApplications { get; set; }
    
    public int Priority1Count { get; set; }
    public int Priority2Count { get; set; }
    public int Priority3Count { get; set; }
}
