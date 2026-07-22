using System.Collections.Generic;
using System.Linq;
using EdeboCsvProcessor.Domain.Entities;

namespace EdeboCsvProcessor.Application.UseCases;

public class CalculateStatisticsUseCase
{
    public StatisticsResult Execute(IEnumerable<Domain.Entities.Application> applications)
    {
        var appList = applications.ToList();
        var result = new StatisticsResult
        {
            TotalApplications = appList.Count,
            UniqueApplicants = appList.Select(a => a.ApplicantName).Distinct().Count(),
            GrantEligibleCount = appList.Count(a => a.GrantStatus == "Так"),
            GrantNotEligibleCount = appList.Count(a => a.GrantStatus == "Ні")
        };

        var today = System.DateTime.Today;
        var todayApps = appList.Where(a => a.SubmissionDate != null && a.SubmissionDate.Value.Date == today).ToList();

        // 1. Кількість заяв поданих абітурієнтами для вступу на кафедру (Today / Total)
        result.TodayApplications = todayApps.Count;
        
        // 2. Кількість оригіналів документів
        result.TotalOriginals = appList.Count(a => a.IsOriginalSubmitted);
        result.TodayOriginals = todayApps.Count(a => a.IsOriginalSubmitted);
        
        // 3. Кількість заключених договорів
        result.TotalContracts = appList.Count(a => a.IsContractSigned);
        result.TodayContracts = todayApps.Count(a => a.IsContractSigned);
        
        // 4. Кількість заяв поданих на контракт
        result.TotalContractApplications = appList.Count(a => a.IsContractApplication);
        result.TodayContractApplications = todayApps.Count(a => a.IsContractApplication);
        
        // 5. За пріоритетами 1, 2, 3 (Total only based on requirement structure)
        result.Priority1Count = appList.Count(a => a.Priority == 1);
        result.Priority2Count = appList.Count(a => a.Priority == 2);
        result.Priority3Count = appList.Count(a => a.Priority == 3);

        result.ApplicationsByStatus = appList
            .Where(a => !string.IsNullOrEmpty(a.Status))
            .GroupBy(a => a.Status!)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        result.ApplicationsByProposal = appList
            .Where(a => a.Proposal != null && !string.IsNullOrEmpty(a.Proposal.Name))
            .GroupBy(a => a.Proposal!.Name)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        return result;
    }
}
