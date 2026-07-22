using System.Collections.Generic;
using EdeboCsvProcessor.Domain.Entities;

namespace EdeboCsvProcessor.Domain.Interfaces;

public interface IApplicationRepository
{
    IEnumerable<Application> GetAll(string filePath);
}
