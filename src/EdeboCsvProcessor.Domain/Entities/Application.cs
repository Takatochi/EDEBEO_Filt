using EdeboCsvProcessor.Domain.ValueObjects;

namespace EdeboCsvProcessor.Domain.Entities;

public class Application
{
    public string ApplicationNumber { get; }
    public string ApplicantName { get; }
    public ApplicationDate? SubmissionDate { get; }
    public CompetitiveProposal? Proposal { get; }
    public string? Status { get; }
    public IReadOnlyList<double> Scores { get; }
    public System.Collections.Generic.Dictionary<string, string> RawData { get; }
    
    public string GrantStatus => Scores.Count(s => s >= 150) >= 2 ? "Так" : "Ні";

    public Application(System.Collections.Generic.Dictionary<string, string> rawData)
    {
        RawData = rawData ?? new System.Collections.Generic.Dictionary<string, string>();
        
        // Clean phone numbers
        var keys = new System.Collections.Generic.List<string>(RawData.Keys);
        foreach (var key in keys)
        {
            if (key.Contains("телефон", System.StringComparison.OrdinalIgnoreCase) || key.Contains("контактний номер", System.StringComparison.OrdinalIgnoreCase))
            {
                if (RawData[key] != null)
                {
                    RawData[key] = RawData[key].Replace("+", "");
                }
            }
        }
        
        // Map common properties for easy filtering
        ApplicationNumber = GetRawValue("Ід заявки", "Номер заяви") ?? "";
        ApplicantName = GetRawValue("Вступник", "ПІБ") ?? "";
        Status = GetRawValue("Статус заявки", "Статус") ?? "";
        
        var dateStr = GetRawValue("Час додання заяви до ЄДЕБО", "Дата додавання");
        if (!string.IsNullOrWhiteSpace(dateStr))
            try { SubmissionDate = ApplicationDate.Parse(dateStr); } catch { }

        var proposalStr = GetRawValue("Назва КП", "Конкурсна пропозиція", "КП");
        if (!string.IsNullOrWhiteSpace(proposalStr))
            Proposal = new CompetitiveProposal(proposalStr);

        var scores = new List<double>();
        foreach (var kvp in RawData)
        {
            if (kvp.Key.Contains("ЗНО.") || kvp.Key.Contains("НМТ."))
            {
                if (double.TryParse(kvp.Value?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var score))
                {
                    scores.Add(score);
                }
            }
        }
        Scores = scores;
    }

    private string? GetRawValue(params string[] possibleKeys)
    {
        foreach (var key in possibleKeys)
        {
            if (RawData.TryGetValue(key, out var val))
                return val;
        }
        return null;
    }

    public Application(string applicationNumber, string applicantName, ApplicationDate submissionDate, CompetitiveProposal proposal, string status, System.Collections.Generic.IEnumerable<double>? scores = null)
    {
        RawData = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Номер заяви", applicationNumber },
            { "ПІБ", applicantName },
            { "Дата додавання", submissionDate.ToString() }, // not strictly accurate formatting, but fine for tests
            { "Конкурсна пропозиція", proposal?.Name ?? "" },
            { "Статус", status }
        };
        
        if (scores != null)
        {
            int i = 1;
            foreach (var s in scores)
            {
                RawData[$"ЗНО.Предмет{i}"] = s.ToString(System.Globalization.CultureInfo.InvariantCulture);
                i++;
            }
        }
        
        ApplicationNumber = applicationNumber;
        ApplicantName = applicantName;
        SubmissionDate = submissionDate;
        Proposal = proposal;
        Status = status;
        Scores = scores?.ToList() ?? new List<double>();
    }
}
