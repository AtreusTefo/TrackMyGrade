using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    public interface IAuditLogService
    {
        /// <summary>Log a create operation to the audit trail.</summary>
        void LogCreate(string entityType, int entityId, object newValues, string performedBy, string ipAddress = null, string userAgent = null);

        /// <summary>Log an update operation to the audit trail.</summary>
        void LogUpdate(string entityType, int entityId, object oldValues, object newValues, string performedBy, string ipAddress = null, string userAgent = null);

        /// <summary>Log a delete operation to the audit trail.</summary>
        void LogDelete(string entityType, int entityId, object oldValues, string performedBy, string ipAddress = null, string userAgent = null);

        /// <summary>Retrieve paginated audit logs with optional filtering.</summary>
        AuditLogPagedResponseDto GetAuditLogs(AuditLogFilterDto filter);

        /// <summary>Retrieve all audit logs for a specific entity.</summary>
        List<AuditLogDto> GetAuditLogsByEntity(string entityType, int entityId);

        /// <summary>Retrieve all audit logs performed by a specific user.</summary>
        List<AuditLogDto> GetAuditLogsByUser(string email);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _db;

        public AuditLogService(ApplicationDbContext db)
        {
            _db = db;
        }

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
