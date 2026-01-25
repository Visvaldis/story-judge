using StoryJudge.Core.Enums;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(string id);
    Task<List<Report>> GetByStatusAsync(ReportStatus status, int page, int pageSize);
    Task<List<Report>> GetAllAsync(int page, int pageSize);
    Task<Report> CreateAsync(Report report);
    Task<Report> UpdateAsync(Report report);
    Task<bool> DeleteAsync(string id);
    Task<int> GetPendingCountAsync();
}
