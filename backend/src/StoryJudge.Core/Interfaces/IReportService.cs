using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;

namespace StoryJudge.Core.Interfaces;

public interface IReportService
{
    Task<ReportDto?> GetByIdAsync(string id);
    Task<List<ReportDto>> GetPendingAsync(int page, int pageSize);
    Task<List<ReportDto>> GetAllAsync(int page, int pageSize);
    Task<ReportDto> CreateAsync(CreateReportRequest request, string reporterId);
    Task<ReportDto> ResolveAsync(string id, ResolveReportRequest request, string moderatorId);
    Task<int> GetPendingCountAsync();
}
