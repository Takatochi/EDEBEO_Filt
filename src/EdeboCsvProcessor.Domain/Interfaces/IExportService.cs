using System.Collections.Generic;
using EdeboCsvProcessor.Domain.Entities;

namespace EdeboCsvProcessor.Domain.Interfaces;

public interface IExportService
{
    void Export(IEnumerable<Application> applications, string filePath, IList<string>? orderedColumns = null);
}
