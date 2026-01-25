using MongoDB.Driver;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;
using StoryJudge.Infrastructure.Data;

namespace StoryJudge.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly MongoDbContext _context;

    public ReportRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(string id)
    {
        return await _context.Reports.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Report>> GetByStatusAsync(ReportStatus status, int page, int pageSize)
    {
        return await _context.Reports
            .Find(r => r.Status == status)
            .SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<Report>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Reports
            .Find(_ => true)
            .SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Report> CreateAsync(Report report)
    {
        await _context.Reports.InsertOneAsync(report);
        return report;
    }

    public async Task<Report> UpdateAsync(Report report)
    {
        await _context.Reports.ReplaceOneAsync(r => r.Id == report.Id, report);
        return report;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.Reports.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<int> GetPendingCountAsync()
    {
        var count = await _context.Reports
            .CountDocumentsAsync(r => r.Status == ReportStatus.Pending);
        return (int)count;
    }
}
