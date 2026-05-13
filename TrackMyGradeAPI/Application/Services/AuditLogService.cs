using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    /// <summary>
    /// Service interface for audit logging operations.
    /// Tracks all create, update, and delete operations for compliance and debugging.
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>Log a create operation to the audit trail.</summary>
        /// <param name="entityType">The type of entity being created.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="newValues">The new values of the entity.</param>
        /// <param name="performedBy">The user or system that performed the action.</param>
        /// <param name="ipAddress">Optional IP address of the user.</param>
        /// <param name="userAgent">Optional user agent string.</param>
        void LogCreate(string entityType, int entityId, object newValues, string performedBy, string ipAddress = null, string userAgent = null);

        /// <summary>Log an update operation to the audit trail.</summary>
        /// <param name="entityType">The type of entity being updated.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="oldValues">The old values of the entity.</param>
        /// <param name="newValues">The new values of the entity.</param>
        /// <param name="performedBy">The user or system that performed the action.</param>
        /// <param name="ipAddress">Optional IP address of the user.</param>
        /// <param name="userAgent">Optional user agent string.</param>
        void LogUpdate(string entityType, int entityId, object oldValues, object newValues, string performedBy, string ipAddress = null, string userAgent = null);

        /// <summary>Log a delete operation to the audit trail.</summary>
        /// <param name="entityType">The type of entity being deleted.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="oldValues">The values of the deleted entity.</param>
        /// <param name="performedBy">The user or system that performed the action.</param>
        /// <param name="ipAddress">Optional IP address of the user.</param>
        /// <param name="userAgent">Optional user agent string.</param>
        void LogDelete(string entityType, int entityId, object oldValues, string performedBy, string ipAddress = null, string userAgent = null);

        /// <summary>Retrieve paginated audit logs with optional filtering.</summary>
        /// <param name="filter">The filter criteria.</param>
        /// <returns>A paginated response containing filtered audit logs.</returns>
        AuditLogPagedResponseDto GetAuditLogs(AuditLogFilterDto filter);

        /// <summary>Retrieve all audit logs for a specific entity.</summary>
        /// <param name="entityType">The type of entity.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <returns>List of audit log DTOs for the specified entity.</returns>
        List<AuditLogDto> GetAuditLogsByEntity(string entityType, int entityId);

        /// <summary>Retrieve all audit logs performed by a specific user.</summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>List of audit log DTOs for the specified user.</returns>
        List<AuditLogDto> GetAuditLogsByUser(string email);
    }

    /// <summary>
    /// Implementation of IAuditLogService for audit trail logging.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the AuditLogService class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public AuditLogService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Logs a create operation to the audit trail.
        /// </summary>
        /// <param name="entityType">The type of entity being created.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="newValues">The new values of the entity.</param>
        /// <param name="performedBy">The user or system that performed the action.</param>
        /// <param name="ipAddress">Optional IP address of the user.</param>
        /// <param name="userAgent">Optional user agent string.</param>
        public void LogCreate(string entityType, int entityId, object newValues, string performedBy, string ipAddress = null, string userAgent = null)
        {
            if (string.IsNullOrWhiteSpace(performedBy))
                throw new ArgumentException("performedBy cannot be null or empty.", nameof(performedBy));

            var log = new AuditLog
            {
                Action      = "Created",
                EntityType  = entityType ?? string.Empty,
                EntityId    = entityId,
                Changes     = SerializeObject(newValues),
                PerformedBy = performedBy.Trim(),
                PerformedAt = DateTime.UtcNow,
                IpAddress   = ipAddress?.Trim(),
                UserAgent   = userAgent?.Trim()
            };

            _db.AuditLogs.Add(log);
            _db.SaveChanges();
        }

        /// <summary>
        /// Logs an update operation to the audit trail.
        /// </summary>
        /// <param name="entityType">The type of entity being updated.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="oldValues">The old values of the entity.</param>
        /// <param name="newValues">The new values of the entity.</param>
        /// <param name="performedBy">The user or system that performed the action.</param>
        /// <param name="ipAddress">Optional IP address of the user.</param>
        /// <param name="userAgent">Optional user agent string.</param>
        public void LogUpdate(string entityType, int entityId, object oldValues, object newValues, string performedBy, string ipAddress = null, string userAgent = null)
        {
            if (string.IsNullOrWhiteSpace(performedBy))
                throw new ArgumentException("performedBy cannot be null or empty.", nameof(performedBy));

            var delta = new { Old = oldValues, New = newValues };

            var log = new AuditLog
            {
                Action      = "Updated",
                EntityType  = entityType ?? string.Empty,
                EntityId    = entityId,
                Changes     = SerializeObject(delta),
                PerformedBy = performedBy.Trim(),
                PerformedAt = DateTime.UtcNow,
                IpAddress   = ipAddress?.Trim(),
                UserAgent   = userAgent?.Trim()
            };

            _db.AuditLogs.Add(log);
            _db.SaveChanges();
        }

        /// <summary>
        /// Logs a delete operation to the audit trail.
        /// </summary>
        /// <param name="entityType">The type of entity being deleted.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="oldValues">The values of the deleted entity.</param>
        /// <param name="performedBy">The user or system that performed the action.</param>
        /// <param name="ipAddress">Optional IP address of the user.</param>
        /// <param name="userAgent">Optional user agent string.</param>
        public void LogDelete(string entityType, int entityId, object oldValues, string performedBy, string ipAddress = null, string userAgent = null)
        {
            if (string.IsNullOrWhiteSpace(performedBy))
                throw new ArgumentException("performedBy cannot be null or empty.", nameof(performedBy));

            var log = new AuditLog
            {
                Action      = "Deleted",
                EntityType  = entityType ?? string.Empty,
                EntityId    = entityId,
                Changes     = SerializeObject(oldValues),
                PerformedBy = performedBy.Trim(),
                PerformedAt = DateTime.UtcNow,
                IpAddress   = ipAddress?.Trim(),
                UserAgent   = userAgent?.Trim()
            };

            _db.AuditLogs.Add(log);
            _db.SaveChanges();
        }

        /// <summary>
        /// Retrieves paginated audit logs with optional filtering.
        /// </summary>
        /// <param name="filter">The filter criteria for querying audit logs.</param>
        /// <returns>A paginated response containing audit logs matching the filter.</returns>
        public AuditLogPagedResponseDto GetAuditLogs(AuditLogFilterDto filter)
        {
            if (filter == null)
                filter = new AuditLogFilterDto();

            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageSize < 1 || filter.PageSize > 500) filter.PageSize = 50;

            var query = _db.AuditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.EntityType))
                query = query.Where(a => a.EntityType == filter.EntityType);

            if (!string.IsNullOrWhiteSpace(filter.Action))
                query = query.Where(a => a.Action == filter.Action);

            if (!string.IsNullOrWhiteSpace(filter.PerformedBy))
                query = query.Where(a => a.PerformedBy.Contains(filter.PerformedBy));

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.PerformedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.PerformedAt <= filter.EndDate.Value);

            // Count total before pagination
            int totalCount = query.Count();

            // Order by most recent first
            query = query.OrderByDescending(a => a.PerformedAt);

            // Apply pagination
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            var records = query.Skip(skip).Take(filter.PageSize)
                .Select(a => new AuditLogDto
                {
                    Id          = a.Id,
                    EntityType  = a.EntityType,
                    EntityId    = a.EntityId,
                    Action      = a.Action,
                    Changes     = a.Changes,
                    PerformedBy = a.PerformedBy,
                    PerformedAt = a.PerformedAt,
                    IpAddress   = a.IpAddress,
                    UserAgent   = a.UserAgent
                }).ToList();

            return new AuditLogPagedResponseDto
            {
                TotalCount  = totalCount,
                PageNumber  = filter.PageNumber,
                PageSize    = filter.PageSize,
                Records     = records
            };
        }

        /// <summary>
        /// Retrieves all audit logs for a specific entity.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <returns>List of audit log DTOs for the specified entity, ordered by most recent first.</returns>
        public List<AuditLogDto> GetAuditLogsByEntity(string entityType, int entityId)
        {
            return _db.AuditLogs
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.PerformedAt)
                .Select(a => new AuditLogDto
                {
                    Id          = a.Id,
                    EntityType  = a.EntityType,
                    EntityId    = a.EntityId,
                    Action      = a.Action,
                    Changes     = a.Changes,
                    PerformedBy = a.PerformedBy,
                    PerformedAt = a.PerformedAt,
                    IpAddress   = a.IpAddress,
                    UserAgent   = a.UserAgent
                }).ToList();
        }

        /// <summary>
        /// Retrieves all audit logs performed by a specific user.
        /// </summary>
        /// <param name="email">The email of the user who performed the actions.</param>
        /// <returns>List of audit log DTOs for the specified user, ordered by most recent first.</returns>
        public List<AuditLogDto> GetAuditLogsByUser(string email)
        {
            return _db.AuditLogs
                .Where(a => a.PerformedBy == email)
                .OrderByDescending(a => a.PerformedAt)
                .Select(a => new AuditLogDto
                {
                    Id          = a.Id,
                    EntityType  = a.EntityType,
                    EntityId    = a.EntityId,
                    Action      = a.Action,
                    Changes     = a.Changes,
                    PerformedBy = a.PerformedBy,
                    PerformedAt = a.PerformedAt,
                    IpAddress   = a.IpAddress,
                    UserAgent   = a.UserAgent
                }).ToList();
        }

        private string SerializeObject(object obj)
        {
            if (obj == null) return null;
            try
            {
                return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch
            {
                return obj.ToString();
            }
        }
    }
}
