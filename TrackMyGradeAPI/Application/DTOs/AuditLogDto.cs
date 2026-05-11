using System;
using System.Collections.Generic;

namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Audit log record returned to admin users.</summary>
    public class AuditLogDto
    {
        /// <summary>Primary key of the audit entry.</summary>
        public int Id { get; set; }

        /// <summary>Entity type that was changed, e.g. "Teacher", "Student", "Assignment".</summary>
        public string EntityType { get; set; }

        /// <summary>Primary key of the affected entity.</summary>
        public int EntityId { get; set; }

        /// <summary>Action performed: "Created", "Updated", or "Deleted".</summary>
        public string Action { get; set; }

        /// <summary>JSON serialized changes: before/after values or delta.</summary>
        public string Changes { get; set; }

        /// <summary>Admin email or system identifier who performed the action.</summary>
        public string PerformedBy { get; set; }

        /// <summary>UTC timestamp of the change.</summary>
        public DateTime PerformedAt { get; set; }

        /// <summary>Optional: IP address of the requester.</summary>
        public string IpAddress { get; set; }

        /// <summary>Optional: User-agent string of the requester.</summary>
        public string UserAgent { get; set; }
    }

    /// <summary>Query parameters for fetching audit logs with pagination and filtering.</summary>
    public class AuditLogFilterDto
    {
        /// <summary>Filter by entity type (optional).</summary>
        public string EntityType { get; set; }

        /// <summary>Filter by action: "Created", "Updated", "Deleted" (optional).</summary>
        public string Action { get; set; }

        /// <summary>Filter by performer email (optional).</summary>
        public string PerformedBy { get; set; }

        /// <summary>Filter logs starting from this date (UTC).</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Filter logs ending at this date (UTC).</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Page number (1-based).</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Records per page.</summary>
        public int PageSize { get; set; } = 50;
    }

    /// <summary>Paginated response of audit logs.</summary>
    public class AuditLogPagedResponseDto
    {
        /// <summary>Total count of records matching the filter.</summary>
        public int TotalCount { get; set; }

        /// <summary>Page number of this response.</summary>
        public int PageNumber { get; set; }

        /// <summary>Records per page.</summary>
        public int PageSize { get; set; }

        /// <summary>Audit log records for this page.</summary>
        public List<AuditLogDto> Records { get; set; } = new List<AuditLogDto>();
    }
}