using StoryJudge.Core.Enums;

namespace StoryJudge.Core.DTOs;

public record CreateReportRequest(
    ReportTargetType TargetType,
    string TargetId,
    ReportReason Reason,
    string? Details
);

public record ReportDto(
    string Id,
    string ReporterId,
    string ReporterName,
    string TargetType,
    string TargetId,
    string Reason,
    string? Details,
    string Status,
    string? ResolvedById,
    string? ResolverName,
    string? Resolution,
    DateTime CreatedAt,
    DateTime? ResolvedAt
);

public record ResolveReportRequest(
    string Resolution,
    string Action // "dismiss", "hide_content", "ban_user"
);
