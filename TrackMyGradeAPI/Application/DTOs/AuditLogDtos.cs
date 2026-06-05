using System;
using System.Collections.Generic;

namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Represents a single audit log entry.</summary>
    public class AuditLogDto
    {
        /// <summary>Gets or sets the audit log ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the type of the affected entity.</summary>
        public string EntityType { get; set; }
        /// <summary>Gets or sets the primary key of the affected entity.</summary>
        public int EntityId { get; set; }
        /// <summary>Gets or sets the action performed (Created, Updated, Deleted).</summary>
        public string Action { get; set; }
        /// <summary>Gets or sets the JSON-serialized changes.</summary>
        public string Changes { get; set; }
        /// <summary>Gets or sets the email of the user who performed the action.</summary>
        public string PerformedBy { get; set; }
        /// <summary>Gets or sets the date and time of the action.</summary>
        public DateTime PerformedAt { get; set; }
        /// <summary>Gets or sets the IP address of the requester.</summary>
        public string IpAddress { get; set; }
        /// <summary>Gets or sets the browser user agent of the requester.</summary>
        public string UserAgent { get; set; }
    }

    /// <summary>Filtering criteria for retrieving audit logs.</summary>
    public class AuditLogFilterDto
    {
        /// <summary>Filter by entity type.</summary>
        public string EntityType { get; set; }
        /// <summary>Filter by specific action.</summary>
        public string Action { get; set; }
        /// <summary>Filter by the person who performed the action.</summary>
        public string PerformedBy { get; set; }
        /// <summary>Filter logs starting from this date.</summary>
        public DateTime? StartDate { get; set; }
        /// <summary>Filter logs ending at this date.</summary>
        public DateTime? EndDate { get; set; }
        /// <summary>The page number to retrieve (1-based).</summary>
        public int PageNumber { get; set; } = 1;
        /// <summary>Number of records per page.</summary>
        public int PageSize { get; set; } = 50;
    }

    /// <summary>Paginated response for audit log queries.</summary>
    public class AuditLogPagedResponseDto
    {
        /// <summary>Gets or sets the total count of records matching the filter.</summary>
        public int TotalCount { get; set; }
        /// <summary>Gets or sets the current page number.</summary>
        public int PageNumber { get; set; }
        /// <summary>Gets or sets the size of the page.</summary>
        public int PageSize { get; set; }
        /// <summary>Gets or sets the collection of audit log records.</summary>
        public List<AuditLogDto> Records { get; set; }
    }
}