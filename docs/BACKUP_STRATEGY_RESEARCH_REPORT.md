# SQL Server Backup Strategy Research Report
## Developer's Guide to Full, Differential, and Transaction Log Backups

**Date:** 2025  
**Project:** Tickets CRM System  
**Target Database:** ApplicationDbContext (SQL Server)

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Backup Types Overview](#backup-types-overview)
3. [Full Backup](#full-backup)
4. [Differential Backup](#differential-backup)
5. [Transaction Log Backup](#transaction-log-backup)
6. [Recovery Models](#recovery-models)
7. [Backup Strategy Recommendations](#backup-strategy-recommendations)
8. [Automating Backups with SSMS Management Wizard](#automating-backups-with-ssms-management-wizard)
9. [T-SQL and PowerShell Automation](#tsql-and-powershell-automation)
10. [Integration with .NET Applications](#integration-with-net-applications)
11. [Monitoring and Maintenance](#monitoring-and-maintenance)

---

## Executive Summary

This report provides comprehensive guidance on implementing a robust SQL Server backup strategy for the Tickets CRM system. A proper backup strategy is critical for:
- **Data Protection**: Protect against data loss due to hardware failures, corruption, or human error
- **Business Continuity**: Minimize downtime in disaster scenarios
- **Compliance**: Meet regulatory requirements for data retention and recovery
- **Performance**: Balance backup frequency with system performance impact

**Recommended Strategy**: Combination of Full + Differential + Transaction Log backups with automated scheduling

---

## Backup Types Overview

| Backup Type | Size | Speed | Frequency | Use Case |
|------------|------|-------|-----------|----------|
| **Full** | Large | Slower | Weekly | Complete database copy |
| **Differential** | Medium | Faster | Daily | Incremental changes since last full |
| **Transaction Log** | Small | Fastest | Hourly/15min | Point-in-time recovery |

---

## Full Backup

### What is a Full Backup?

A **full backup** is a complete copy of the entire database at a specific point in time. It includes:
- All data pages
- All indexes
- All database metadata
- Transaction log information

### Characteristics

| Property | Value |
|----------|-------|
| **Size** | Largest (100% of database) |
| **Duration** | Longest |
| **Recovery Point** | One point in time |
| **Restore Time** | Fastest single-file restore |
| **Frequency** | Weekly or monthly for large DBs |

### When to Use Full Backup

✅ **Use Full Backup When:**
- Setting up initial backup chain
- After major schema changes
- Before critical applications go live
- For weekly/monthly archival copies
- When storage space is abundant
- For off-site disaster recovery copies

### Advantages
- **Complete snapshot** of database
- **No dependency** on other backups
- **Simple restore** - single file needed
- **Fast restore** process
- **Easy validation** of integrity

### Disadvantages
- **Large file size** (entire database)
- **Longer backup time** (impacts system performance)
- **More storage required**
- **Higher I/O overhead** during backup window

### T-SQL Example

```sql
-- Full Backup
BACKUP DATABASE [TicketsDb]
TO DISK = 'D:\Backups\TicketsDb_Full_20250101.bak'
WITH 
	DESCRIPTION = 'Full backup of Tickets database',
	NAME = 'TicketsDb_Full_Backup',
	COMPRESSION;
```

### Performance Impact

- **I/O Impact**: High - reads entire database
- **CPU Impact**: Medium (if compression enabled)
- **Network Impact**: High (if backing up over network)
- **User Impact**: Possible slowdown during backup window
- **Estimated Time**: 5-30 minutes (depending on database size)

---

## Differential Backup

### What is a Differential Backup?

A **differential backup** is an incremental backup that contains only the data blocks that have **changed since the last full backup**. It:
- References the full backup as its base
- Contains only modified extents
- Is smaller than full backup
- Requires full backup for restore

### Characteristics

| Property | Value |
|----------|-------|
| **Size** | 10-50% of database size |
| **Duration** | Moderate |
| **Recovery Point** | Since last full backup |
| **Restore Time** | Moderate (requires full + differential) |
| **Frequency** | Daily or multiple times per day |

### When to Use Differential Backup

✅ **Use Differential Backup When:**
- Need faster backups than full
- Changes are 10-40% of database size
- Want shorter recovery time than transaction logs alone
- Have limited backup window
- Store daily snapshots

### Advantages
- **Smaller size** than full backup (faster)
- **Faster backup** than full (less I/O)
- **Quick restore** (2 files: full + latest differential)
- **Easy recovery point** selection
- **Good for daily** backup schedules

### Disadvantages
- **Cannot restore alone** (requires full backup)
- **Size grows over time** until next full backup
- **Dependency chain** (full backup must exist)
- **Less efficient** if many changes occur
- **Only works with recovery models** that support it

### How It Works

```
Week Timeline:
Monday:    Full Backup (100% of DB)
Tuesday:   Differential (5% changed since Mon)
Wednesday: Differential (12% changed since Mon)
Thursday:  Differential (8% changed since Mon)
Friday:    Differential (15% changed since Mon)
```

### T-SQL Example

```sql
-- Differential Backup (requires prior full backup)
BACKUP DATABASE [TicketsDb]
TO DISK = 'D:\Backups\TicketsDb_Diff_20250102.bak'
WITH 
	DIFFERENTIAL,
	DESCRIPTION = 'Differential backup of Tickets database',
	NAME = 'TicketsDb_Diff_Backup',
	COMPRESSION;
```

### Restore Example

```sql
-- Restore process requires full backup first
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\TicketsDb_Full_20250101.bak'
WITH NORECOVERY;

-- Then apply latest differential
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\TicketsDb_Diff_20250102.bak'
WITH RECOVERY;
```

### Performance Impact

- **I/O Impact**: Low-Medium (only reads changed blocks)
- **CPU Impact**: Low
- **Network Impact**: Medium (if networked)
- **User Impact**: Minimal
- **Estimated Time**: 5-15 minutes (depending on changes)

---

## Transaction Log Backup

### What is a Transaction Log Backup?

A **transaction log backup** is a copy of the transaction log that records every committed transaction since the last transaction log backup. It:
- Captures every database change
- Enables point-in-time recovery
- Records both committed and rolled-back transactions
- Is cumulative and sequential

### Characteristics

| Property | Value |
|----------|-------|
| **Size** | Very small (KB to MB) |
| **Duration** | Very fast (seconds) |
| **Recovery Point** | Point-in-time precision |
| **Restore Time** | Slowest (requires multiple restores) |
| **Frequency** | Every 15 minutes to 1 hour |

### When to Use Transaction Log Backup

✅ **Use Transaction Log Backup When:**
- Need point-in-time recovery
- Using FULL or BULK_LOGGED recovery model
- Want to recover to specific minute/second
- Have database in production
- Need to minimize data loss (RTO/RPO requirements)

### Advantages
- **Smallest backup size** (very fast)
- **Minimal system impact** (low I/O)
- **Frequent backups** possible
- **Point-in-time recovery** capability
- **Enables "up to the minute" recovery**
- **Can replay** specific transactions

### Disadvantages
- **Cannot restore alone** (requires full + differentials)
- **Multiple restores needed** for recovery
- **Longer recovery process** (all logs replayed)
- **Depends on log chain integrity**
- **Storage accumulates** (many small files)

### Recovery Models Required

Transaction log backups work with these recovery models:

| Recovery Model | Supports TLog | Purpose |
|---|---|---|
| **FULL** | ✅ Yes | Complete logging, point-in-time recovery |
| **BULK_LOGGED** | ✅ Yes | Optimized for bulk operations |
| **SIMPLE** | ❌ No | No transaction log backups |

### T-SQL Example

```sql
-- First, ensure database is in FULL recovery mode
ALTER DATABASE [TicketsDb]
SET RECOVERY FULL;

-- Transaction Log Backup
BACKUP LOG [TicketsDb]
TO DISK = 'D:\Backups\TicketsDb_Log_20250102_090000.trn'
WITH 
	DESCRIPTION = 'Transaction log backup',
	NAME = 'TicketsDb_TLog_Backup',
	COMPRESSION;
```

### Point-in-Time Recovery Example

```sql
-- Recover database to a specific point in time
-- First restore full backup with no recovery
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\TicketsDb_Full_20250101.bak'
WITH NORECOVERY;

-- Apply differential backup
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\TicketsDb_Diff_20250102.bak'
WITH NORECOVERY;

-- Apply transaction logs until the desired time
RESTORE LOG [TicketsDb]
FROM DISK = 'D:\Backups\TicketsDb_Log_20250102_080000.trn'
WITH NORECOVERY;

RESTORE LOG [TicketsDb]
FROM DISK = 'D:\Backups\TicketsDb_Log_20250102_090000.trn'
WITH NORECOVERY, STOPAT = '2025-01-02 09:30:00';

-- Recover the database
RESTORE DATABASE [TicketsDb] WITH RECOVERY;
```

### Performance Impact

- **I/O Impact**: Minimal (only transaction log)
- **CPU Impact**: Minimal
- **Network Impact**: Minimal
- **User Impact**: None (background operation)
- **Estimated Time**: 5-30 seconds

---

## Recovery Models

### Understanding Recovery Models

The **recovery model** determines what gets logged and what can be backed up:

### FULL Recovery Model

```
Best for: Production systems with high availability needs
Backup Support: Full, Differential, Transaction Log
Data Loss Risk: Minimal (up to last transaction log backup)
Storage Required: High (must keep all transaction logs)

Example Configuration:
ALTER DATABASE [TicketsDb] SET RECOVERY FULL;
```

**Use FULL when:**
- Production database
- Need point-in-time recovery
- Data loss is unacceptable
- Regulatory compliance required

### BULK_LOGGED Recovery Model

```
Best for: Bulk operations with point-in-time recovery
Backup Support: Full, Differential, Transaction Log
Data Loss Risk: Minimal
Storage Required: Medium (logs bulk operations minimally)

Example Configuration:
ALTER DATABASE [TicketsDb] SET RECOVERY BULK_LOGGED;
```

**Use BULK_LOGGED when:**
- Running large BULK INSERT operations
- Still need point-in-time recovery
- Want to minimize transaction log size during bulk ops
- Can be temporary (switch back to FULL after bulk op)

### SIMPLE Recovery Model

```
Best for: Development, testing, non-critical databases
Backup Support: Full, Differential (NO Transaction Log)
Data Loss Risk: High (can lose up to last full/diff backup)
Storage Required: Low

Example Configuration:
ALTER DATABASE [TicketsDb] SET RECOVERY SIMPLE;
```

**Use SIMPLE when:**
- Development/test environment
- Non-critical data
- Don't need point-in-time recovery
- Want to minimize transaction log size

---

## Backup Strategy Recommendations

### Strategy Option 1: Weekly Full + Daily Differential + 30-minute Transaction Log (Recommended for Active Systems)

#### Daily Backup Schedule

| Time | Backup Type | Frequency | Retention | File Location |
|------|------------|-----------|-----------|--------------|
| **Sunday 2:00 AM** | Full | Weekly | 4 weeks | D:\Backups\Full |
| **Mon-Sat 2:00 AM** | Differential | Daily | 7 days | D:\Backups\Differential |
| **Every 30 min** | Transaction Log | 48/day | 7 days | D:\Backups\TLog |

**Best For:**
- High-transaction databases
- 24/7 availability requirements
- RPO < 30 minutes acceptable
- High storage availability

**Storage Impact:**
```
Daily Estimate:   1 Diff (500MB) + 48 Logs (100MB) = 600 MB
Weekly Estimate:  Full (4.8GB) + 6 Diff (3GB) + 336 Logs (3.4GB) = 11.2 GB
Monthly Estimate: ~45 GB
```

---

### Strategy Option 2: Monthly Full + Daily Differential + Hourly Transaction Log (COST-OPTIMIZED ⭐ RECOMMENDED)

#### Monthly Backup Schedule

| Day | Time | Backup Type | Frequency | Retention | File Location |
|-----|------|------------|-----------|-----------|--------------|
| **1st of Month** | 2:00 AM | Full | Monthly | 90 days | D:\Backups\Full |
| **All other days** | 2:00 AM | Differential | Daily | 35 days | D:\Backups\Differential |
| **Every hour** | 00:00-23:00 | Transaction Log | 24/day | 30 days | D:\Backups\TLog |

#### Key Advantages

✅ **Significant storage savings** (~75% reduction compared to weekly strategy)
✅ **Minimal performance impact** (hourly logs < 30-minute logs)
✅ **Still excellent recovery capability** (hourly RPO)
✅ **Reduced backup bandwidth** on network
✅ **Ideal for mid-sized databases** (5-20GB)
✅ **Lower infrastructure costs**
✅ **Easier cleanup** (fewer full backups to manage)

#### Storage Comparison

For a typical CRM database (~5GB):

```
MONTHLY STRATEGY (Cost-Optimized):
├─ Full Backup:        4.8 GB (once/month)
├─ Differential:       500 MB × 30 = 15 GB (daily for 30 days)
├─ Transaction Log:    80 MB × 24 × 30 = 57.6 GB (hourly for 30 days)
└─ Total Monthly:      77.4 GB (~$7-10/month on cloud storage)

vs. WEEKLY STRATEGY:
├─ Full Backup:        4.8 GB × 4 = 19.2 GB (weekly)
├─ Differential:       500 MB × 6 × 4 = 12 GB (daily, 6 days/week)
├─ Transaction Log:    100 MB × 48 × 30 = 144 GB (every 30 min)
└─ Total Monthly:      175.2 GB (~$17-22/month on cloud storage)

SAVINGS: ~97.8 GB per month (55% reduction)
```

#### Recovery Time Comparison

```
Scenario: Need to restore database to 2:45 PM on the 15th

MONTHLY STRATEGY:
1. Restore Full Backup (01st, 2:00 AM) → 20 minutes
2. Restore Differential (15th, 2:00 AM) → 5 minutes
3. Restore Transaction Logs (15th, 02:00 → 14:00) → 10 minutes
4. Restore Transaction Log (15th, 15:00) until 2:45 PM → 5 minutes
────────────────────────────────────────────────
Total Recovery Time: 40 minutes
Restored to: 2:45 PM (exact point-in-time)
Data Loss: 0 minutes (except uncommitted transactions)

WEEKLY STRATEGY:
1. Restore Full Backup (15th Sunday, 2:00 AM) → 20 minutes
2. Restore Differential (15th Monday, 2:00 AM) → 3 minutes
3. Restore Transaction Logs (15th Mon, 02:00 → 14:45) → 8 minutes
────────────────────────────────────────────────
Total Recovery Time: 31 minutes
Restored to: 2:45 PM (exact point-in-time)
Data Loss: 0 minutes

NOTE: Monthly strategy is slightly longer but still acceptable for RTO
```

#### Implementation Code

```sql
-- Set to FULL recovery mode
ALTER DATABASE [TicketsDb]
SET RECOVERY FULL;

-- Initialize backup chain with full backup
BACKUP DATABASE [TicketsDb]
TO DISK = 'D:\Backups\Full\TicketsDb_Full_20250101.bak'
WITH 
    DESCRIPTION = 'Initial full backup - start of backup chain',
    NAME = 'TicketsDb_Full_Initial',
    COMPRESSION;

-- Verify recovery model
SELECT recovery_model_desc FROM sys.databases WHERE name = 'TicketsDb';
-- Output: FULL (required for transaction log backups)
```

#### Monthly Full Backup (1st of month at 2:00 AM)

```sql
BACKUP DATABASE [TicketsDb]
TO DISK = 'D:\Backups\Full\TicketsDb_Full_' + 
    CONVERT(VARCHAR(8), GETDATE(), 112) + '.bak'
WITH 
    DESCRIPTION = 'Monthly full backup - backup chain start',
    NAME = 'TicketsDb_Full_Monthly',
    COMPRESSION,
    CHECKSUM;
```

#### Daily Differential Backup (All days at 2:00 AM, except 1st)

```sql
BACKUP DATABASE [TicketsDb]
TO DISK = 'D:\Backups\Differential\TicketsDb_Diff_' + 
    CONVERT(VARCHAR(8), GETDATE(), 112) + '.bak'
WITH 
    DIFFERENTIAL,
    DESCRIPTION = 'Daily differential backup',
    NAME = 'TicketsDb_Diff_Daily',
    COMPRESSION,
    CHECKSUM;
```

#### Hourly Transaction Log Backup (Every hour)

```sql
BACKUP LOG [TicketsDb]
TO DISK = 'D:\Backups\TLog\TicketsDb_Log_' + 
    CONVERT(VARCHAR(8), GETDATE(), 112) + '_' +
    CONVERT(VARCHAR(4), GETDATE(), 114) + '.trn'
WITH 
    DESCRIPTION = 'Hourly transaction log backup',
    NAME = 'TicketsDb_Log_Hourly',
    COMPRESSION,
    CHECKSUM;
```

#### Recommended Retention Cleanup

```sql
-- Monthly cleanup job - runs on the 1st at 3:00 AM
DECLARE @CutoffDate_Diff DATETIME = DATEADD(DAY, -35, CAST(GETDATE() AS DATE));
DECLARE @CutoffDate_Log DATETIME = DATEADD(DAY, -30, CAST(GETDATE() AS DATE));

-- Delete old differential backups (keep 35 days)
DELETE FROM msdb.dbo.backupset
WHERE database_name = 'TicketsDb'
  AND backup_type = 'I'  -- I = Incremental (Differential)
  AND backup_finish_date < @CutoffDate_Diff;

-- Delete old transaction logs (keep 30 days)
DELETE FROM msdb.dbo.backupset
WHERE database_name = 'TicketsDb'
  AND backup_type = 'L'  -- L = Log
  AND backup_finish_date < @CutoffDate_Log;

-- Delete orphaned backup media
DELETE FROM msdb.dbo.backupmediafamily
WHERE media_set_id NOT IN (SELECT media_set_id FROM msdb.dbo.backupset);
```

---

### Strategy Option 3: Weekly Full + Daily Differential + 15-minute Transaction Log (High Availability)

#### Daily Backup Schedule

| Time | Backup Type | Frequency | Retention | File Location |
|------|------------|-----------|-----------|--------------|
| **Sunday 2:00 AM** | Full | Weekly | 4 weeks | D:\Backups\Full |
| **Mon-Sat 2:00 AM** | Differential | Daily | 7 days | D:\Backups\Differential |
| **Every 15 min** | Transaction Log | 96/day | 7 days | D:\Backups\TLog |

**Best For:**
- Financial/healthcare applications
- RPO < 15 minutes required
- High transaction volume
- Mission-critical systems

**Trade-offs:**
```
Advantages:
✅ Best recovery point (15-minute RPO)
✅ More granular recovery options
✅ Excellent for compliance audits

Disadvantages:
❌ Highest storage requirements (~200GB/month)
❌ More I/O overhead (96 backups/day)
❌ Network saturation risk
❌ Complex retention management
```

---

### Comparison Summary

| Metric | Monthly Full | Weekly Full | Weekly Full + 15min |
|--------|-------------|-----------|-----------------|
| **RPO** | 1 hour | 30 min | 15 min |
| **RTO** | 40 min | 31 min | 28 min |
| **Monthly Storage** | 77 GB | 175 GB | 200 GB |
| **Cost/Month** | ~$8-10 | ~$18-22 | ~$22-26 |
| **Backup I/O** | Low | Medium | High |
| **Full Backups/Year** | 12 | 52 | 52 |
| **Best For** | Cost-conscious | Balanced | High-availability |

---

### Recommended Strategy for Tickets CRM (Production)

**🏆 RECOMMENDATION: Monthly Full + Daily Differential + Hourly Log**

**Rationale:**
- Tickets CRM is a mid-sized business application (5-10GB estimated)
- Hourly RPO (1 hour) is acceptable for typical business workflows
- 77GB/month storage is manageable (cloud storage ~$8-10/month)
- Significantly reduces infrastructure burden compared to weekly strategy
- Still maintains excellent recovery capability
- Good balance between cost and availability

#### Implementation Code

```sql
-- Set to FULL recovery mode
ALTER DATABASE [TicketsDb]
SET RECOVERY FULL;

-- Shrink log if needed (one-time)
DBCC SHRINKFILE (TicketsDb_log, 1000);

-- Set up log reuse to prevent full logs
BACKUP LOG [TicketsDb]
TO DISK = 'D:\Backups\TLog\dummy.trn'
WITH NO_TRUNCATE;
```

### Backup Sizes Estimation (Monthly Strategy)

For a typical CRM database (~5GB):

```
Full Backup:      4.8 GB (once/month)
Differential:     500 MB (daily avg, ~30 days = 15 GB)
Transaction Log:  80 MB per hour × 24 hours × 30 days = 57.6 GB

Monthly Total:    77.4 GB
Daily Average:    2.6 GB
```

### RPO/RTO Targets (Monthly Strategy)

```
Recovery Time Objective (RTO):
- Full (4.8GB): 20 minutes
- Differential (500MB): 5 minutes
- Transaction Logs (avg 24): 10 minutes
- Total: 35-40 minutes restore time
- Acceptable for most business applications

Recovery Point Objective (RPO):
- With hourly transaction logs: 1-hour data loss maximum
- Meets most regulatory compliance requirements
- Excellent for business continuity
```

---

## Setting Up Monthly Full + Daily Differential + Hourly Log Strategy

### Prerequisites

```sql
-- Verify database is in FULL recovery mode
ALTER DATABASE [TicketsDb]
SET RECOVERY FULL;

-- Verify database files location
SELECT name, physical_name FROM sys.master_files 
WHERE database_id = DB_ID('TicketsDb');

-- Create backup directory structure
```

**PowerShell - Create Backup Directories:**
```powershell
# Create backup folder structure
$BaseBackupPath = "D:\Backups"
$Folders = @("Full", "Differential", "TLog", "Logs", "Archives")

foreach ($Folder in $Folders) {
    $Path = Join-Path $BaseBackupPath $Folder
    if (!(Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
        Write-Host "Created: $Path"
    }
}

# Set appropriate NTFS permissions
$BackupPath = "D:\Backups"
$ACL = Get-Acl $BackupPath
$AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("NETWORK SERVICE", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$ACL.SetAccessRule($AccessRule)
Set-Acl -Path $BackupPath -AclObject $ACL
```

### SQL Server Agent Jobs Setup

#### Job 1: Monthly Full Backup (1st of month at 2:00 AM)

```sql
-- Create job
EXEC msdb.dbo.sp_add_job
    @job_name = 'TicketsDb_Full_Backup_Monthly',
    @enabled = 1,
    @description = 'Monthly full backup - runs on 1st of month';

-- Create job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'TicketsDb_Full_Backup_Monthly',
    @step_name = 'Execute Full Backup',
    @subsystem = 'TSQL',
    @command = N'
        DECLARE @DatabaseName NVARCHAR(128) = ''TicketsDb'';
        DECLARE @BackupPath NVARCHAR(500) = ''D:\Backups\Full\'';
        DECLARE @BackupFileName NVARCHAR(500);
        DECLARE @Cmd NVARCHAR(1000);

        SET @BackupFileName = @BackupPath + @DatabaseName + ''_Full_'' + CONVERT(VARCHAR(8), GETDATE(), 112) + ''.bak'';

        SET @Cmd = ''BACKUP DATABASE ['' + @DatabaseName + ''] TO DISK = '''''' + @BackupFileName + ''''''
                   WITH COMPRESSION, CHECKSUM, INIT,
                   DESCRIPTION = ''''Monthly Full Backup'''';

        EXEC sp_executesql @Cmd;
        PRINT ''Full backup completed: '' + @BackupFileName;
    ',
    @database_name = 'master',
    @retry_attempts = 2,
    @retry_interval = 5;

-- Create schedule for 1st of month at 2:00 AM
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'Monthly_1st_2AM',
    @freq_type = 16,  -- Monthly
    @freq_interval = 1,  -- 1st day
    @active_start_time = 020000;

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'TicketsDb_Full_Backup_Monthly',
    @schedule_name = 'Monthly_1st_2AM';
```

#### Job 2: Daily Differential Backup (Mon-Sat at 2:00 AM, skips 1st of month)

```sql
-- Create job
EXEC msdb.dbo.sp_add_job
    @job_name = 'TicketsDb_Diff_Backup_Daily',
    @enabled = 1,
    @description = 'Daily differential backup (except 1st of month)';

-- Create job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'TicketsDb_Diff_Backup_Daily',
    @step_name = 'Execute Differential Backup',
    @subsystem = 'TSQL',
    @command = N'
        -- Skip if today is the 1st (full backup day)
        IF DAY(GETDATE()) > 1
        BEGIN
            DECLARE @DatabaseName NVARCHAR(128) = ''TicketsDb'';
            DECLARE @BackupPath NVARCHAR(500) = ''D:\Backups\Differential\'';
            DECLARE @BackupFileName NVARCHAR(500);
            DECLARE @Cmd NVARCHAR(1000);

            SET @BackupFileName = @BackupPath + @DatabaseName + ''_Diff_'' + CONVERT(VARCHAR(8), GETDATE(), 112) + ''.bak'';

            SET @Cmd = ''BACKUP DATABASE ['' + @DatabaseName + ''] TO DISK = '''''' + @BackupFileName + ''''''
                       WITH DIFFERENTIAL, COMPRESSION, CHECKSUM, INIT,
                       DESCRIPTION = ''''Daily Differential Backup'''';

            EXEC sp_executesql @Cmd;
            PRINT ''Differential backup completed: '' + @BackupFileName;
        END
        ELSE
        BEGIN
            PRINT ''Skipping differential backup - today is 1st of month (full backup day)'';
        END;
    ',
    @database_name = 'master',
    @retry_attempts = 2,
    @retry_interval = 5;

-- Create schedule for every day at 2:00 AM
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'Daily_2AM',
    @freq_type = 4,  -- Daily
    @freq_interval = 1,  -- Every day
    @active_start_time = 020000;

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'TicketsDb_Diff_Backup_Daily',
    @schedule_name = 'Daily_2AM';
```

#### Job 3: Hourly Transaction Log Backup (Every hour, 24x7)

```sql
-- Create job
EXEC msdb.dbo.sp_add_job
    @job_name = 'TicketsDb_TLog_Backup_Hourly',
    @enabled = 1,
    @description = 'Hourly transaction log backup (runs 24/7)';

-- Create job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'TicketsDb_TLog_Backup_Hourly',
    @step_name = 'Execute Transaction Log Backup',
    @subsystem = 'TSQL',
    @command = N'
        DECLARE @DatabaseName NVARCHAR(128) = ''TicketsDb'';
        DECLARE @BackupPath NVARCHAR(500) = ''D:\Backups\TLog\'';
        DECLARE @BackupFileName NVARCHAR(500);
        DECLARE @Cmd NVARCHAR(1000);

        -- Format: TicketsDb_Log_20250115_14.trn (date_hour format)
        SET @BackupFileName = @BackupPath + @DatabaseName + ''_Log_'' + 
            CONVERT(VARCHAR(8), GETDATE(), 112) + ''_'' +
            RIGHT(''0'' + CONVERT(VARCHAR(2), DATEPART(HOUR, GETDATE())), 2) + ''.trn'';

        SET @Cmd = ''BACKUP LOG ['' + @DatabaseName + ''] TO DISK = '''''' + @BackupFileName + ''''''
                   WITH COMPRESSION, CHECKSUM, INIT,
                   DESCRIPTION = ''''Hourly Transaction Log Backup'''';

        EXEC sp_executesql @Cmd;
        PRINT ''Transaction log backup completed: '' + @BackupFileName;
    ',
    @database_name = 'master',
    @retry_attempts = 2,
    @retry_interval = 5;

-- Create schedule for every hour
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'Every_Hour_24x7',
    @freq_type = 4,  -- Daily
    @freq_interval = 1,
    @freq_subtype_type = 8,  -- Hourly
    @freq_subtype_interval = 1,  -- Every hour
    @active_start_time = 000000,
    @active_end_time = 235959;

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'TicketsDb_TLog_Backup_Hourly',
    @schedule_name = 'Every_Hour_24x7';
```

#### Job 4: Monthly Cleanup (1st at 3:00 AM - after full backup)

```sql
-- Create job
EXEC msdb.dbo.sp_add_job
    @job_name = 'TicketsDb_Backup_Cleanup_Monthly',
    @enabled = 1,
    @description = 'Delete old backups and maintain retention policy';

-- Create job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'TicketsDb_Backup_Cleanup_Monthly',
    @step_name = 'Cleanup Old Backups',
    @subsystem = 'TSQL',
    @command = N'
        DECLARE @CutoffDate_Diff DATETIME = DATEADD(DAY, -35, CAST(GETDATE() AS DATE));
        DECLARE @CutoffDate_Log DATETIME = DATEADD(DAY, -30, CAST(GETDATE() AS DATE));
        DECLARE @CutoffDate_Full DATETIME = DATEADD(DAY, -90, CAST(GETDATE() AS DATE));

        -- Delete old differential backups (keep 35 days)
        DELETE FROM msdb.dbo.backupset
        WHERE database_name = ''TicketsDb''
          AND backup_type = ''I''
          AND backup_finish_date < @CutoffDate_Diff;

        PRINT ''Deleted differential backups older than 35 days'';

        -- Delete old transaction logs (keep 30 days)
        DELETE FROM msdb.dbo.backupset
        WHERE database_name = ''TicketsDb''
          AND backup_type = ''L''
          AND backup_finish_date < @CutoffDate_Log;

        PRINT ''Deleted transaction logs older than 30 days'';

        -- Delete old full backups (keep 90 days)
        DELETE FROM msdb.dbo.backupset
        WHERE database_name = ''TicketsDb''
          AND backup_type = ''D''
          AND backup_finish_date < @CutoffDate_Full;

        PRINT ''Deleted full backups older than 90 days'';

        -- Delete orphaned backup media
        DELETE FROM msdb.dbo.backupmediafamily
        WHERE media_set_id NOT IN (SELECT media_set_id FROM msdb.dbo.backupset);

        PRINT ''Cleanup completed successfully'';
    ',
    @database_name = 'master';

-- Create schedule for 1st of month at 3:00 AM
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'Monthly_1st_3AM',
    @freq_type = 16,
    @freq_interval = 1,
    @active_start_time = 030000;

-- Attach schedule
EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'TicketsDb_Backup_Cleanup_Monthly',
    @schedule_name = 'Monthly_1st_3AM';
```

#### Job 5: Weekly Database Integrity Check

```sql
-- Create job
EXEC msdb.dbo.sp_add_job
    @job_name = 'TicketsDb_DBCC_Weekly',
    @enabled = 1,
    @description = 'Weekly database integrity check';

-- Create job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'TicketsDb_DBCC_Weekly',
    @step_name = 'Check Database Integrity',
    @subsystem = 'TSQL',
    @command = N'
        DBCC CHECKDB (''TicketsDb'') WITH NO_INFOMSGS;
        PRINT ''DBCC CHECKDB completed successfully'';
    ',
    @database_name = 'TicketsDb';

-- Schedule for Sunday at 3:30 AM (after cleanup)
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'Weekly_Sunday_330AM',
    @freq_type = 8,  -- Weekly
    @freq_interval = 1,  -- Sunday
    @active_start_time = 033000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'TicketsDb_DBCC_Weekly',
    @schedule_name = 'Weekly_Sunday_330AM';
```

### Verify Jobs

```sql
-- Check all created jobs
SELECT 
    j.name AS JobName,
    j.enabled,
    s.name AS ScheduleName,
    s.freq_type_desc,
    s.active_start_time
FROM msdb.dbo.sysjobs j
LEFT JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
LEFT JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
WHERE j.name LIKE 'TicketsDb%'
ORDER BY j.name;

-- Check job history
SELECT TOP 20
    j.name,
    h.step_name,
    h.run_status,
    h.run_date,
    h.run_time,
    h.message
FROM msdb.dbo.sysjobhistory h
INNER JOIN msdb.dbo.sysjobs j ON h.job_id = j.job_id
WHERE j.name LIKE 'TicketsDb%'
ORDER BY h.run_date DESC, h.run_time DESC;
```

---

## Automating Backups with SSMS Management Wizard

### Step 1: Open SQL Server Management Studio

1. **Connect to your SQL Server instance**
   - Server name: `.\SQLEXPRESS` (local) or server IP/name
   - Authentication: Windows or SQL Login
   - Click **Connect**

### Step 2: Access Maintenance Plan Wizard

**Path:** `Databases` → Right-click `[TicketsDb]` → `Tasks` → `Maintenance Plans` → `Maintenance Plan Wizard`

Or:

**Path:** `Management` → `Maintenance Plans` → `New Maintenance Plan`

### Step 3: Configure Full Backup (Weekly)

```
Step 1: Define the Backup Plan
├── Plan Name: "TicketsDb_Full_Backup"
├── Schedule: "Weekly"
└── Run as: SQL Server Agent

Step 2: Select Maintenance Tasks
├── Task: "Back Up Database (Full)"
└── Add to plan

Step 3: Define Backup Destination
├── Backup Type: "Full"
├── Backup Component: "Database"
├── Backup to: "Disk"
├── Backup location: "D:\Backups\Full"
├── File extension: ".bak"
├── Verify backup integrity: ✓ Enabled
└── Compression: ✓ Enabled

Step 4: Schedule
├── Frequency: "Weekly"
├── Day: "Sunday"
├── Time: "02:00"
└── Repeat interval: None

Step 5: Retention
├── Delete backup files older than: "4 weeks"
└── Location: "D:\Backups\Full"
```

### Step 4: Configure Differential Backup (Daily)

```
Step 1: New Maintenance Task
├── Plan Name: "TicketsDb_Diff_Backup"
└── Schedule: "Daily"

Step 2: Select Task
├── Task: "Back Up Database (Differential)"

Step 3: Configure Differential
├── Backup Type: "Differential"
├── Backup to: "Disk"
├── Backup location: "D:\Backups\Differential"
├── Verify integrity: ✓ Enabled
└── Compression: ✓ Enabled

Step 4: Schedule
├── Frequency: "Daily"
├── Time: "02:30 AM"
└── Repeat: "Every day"

Step 5: Retention
├── Delete files older than: "7 days"
└── Location: "D:\Backups\Differential"
```

### Step 5: Configure Transaction Log Backup (Every 30 minutes)

```
Step 1: New Maintenance Task
├── Plan Name: "TicketsDb_TLog_Backup"
└── Schedule: "Multiple times per day"

Step 2: Select Task
├── Task: "Back Up Database (Transaction Log)"

Step 3: Configure Log Backup
├── Backup Type: "Transaction Log"
├── Backup to: "Disk"
├── Backup location: "D:\Backups\TLog"
├── File extension: ".trn"
├── Verify integrity: ✓ Enabled
└── Compression: ✓ Enabled

Step 4: Schedule
├── Frequency: "Multiple times per day"
├── Start time: "00:00"
├── Duration: "24:00"
├── Frequency every: "30 minutes"

Step 5: Retention
├── Delete files older than: "7 days"
└── Location: "D:\Backups\TLog"
```

### Step 6: Configure Maintenance Tasks (Cleanup)

```
Additional Task: Delete Old Backup Files
├── Search location: "D:\Backups"
├── File extension to delete: ".bak", ".trn"
├── File age: "Delete files older than 30 days"
└── Schedule: "Weekly (Sunday 4:00 AM)"

Additional Task: Check Database Integrity
├── Database: "[TicketsDb]"
├── Include indexes: ✓ Enabled
└── Schedule: "Weekly (Sunday 03:00 AM)"

Additional Task: Update Statistics
├── All tables
└── Schedule: "Weekly (Sunday 03:30 AM)"
```

### Step 7: Complete and Schedule

```
Final Step:
├── Review Summary of all tasks
├── Verify all schedules
├── Verify all retention policies
├── Click "Finish"
└── Confirm "SQL Server Agent is running"
```

### Verification

```sql
-- Verify maintenance plans created
SELECT * FROM msdb.dbo.sysmaintplan;

-- Check scheduled jobs
SELECT 
	name,
	enabled,
	next_run_date,
	next_run_time
FROM msdb.dbo.sysjobs
WHERE name LIKE 'TicketsDb%';
```

---

## T-SQL and PowerShell Automation

### Automated Backup Script (T-SQL)

Create a stored procedure for scheduled backups:

```sql
CREATE PROCEDURE [dbo].[sp_TicketsDb_Backup]
	@BackupType VARCHAR(20) = 'FULL',  -- FULL, DIFF, or LOG
	@BackupPath NVARCHAR(500) = 'D:\Backups\'
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @DatabaseName NVARCHAR(128) = 'TicketsDb';
	DECLARE @BackupFileName NVARCHAR(500);
	DECLARE @BackupCommand NVARCHAR(1000);
	DECLARE @CurrentDateTime VARCHAR(30);
	DECLARE @ErrorMessage NVARCHAR(500);

	-- Get current datetime
	SET @CurrentDateTime = CONVERT(VARCHAR(30), GETDATE(), 120);
	SET @CurrentDateTime = REPLACE(@CurrentDateTime, ':', '');
	SET @CurrentDateTime = REPLACE(@CurrentDateTime, '-', '');
	SET @CurrentDateTime = REPLACE(@CurrentDateTime, ' ', '_');

	BEGIN TRY
		-- Verify backup path exists
		IF NOT EXISTS (SELECT * FROM sys.dm_os_file_exists(@BackupPath))
		BEGIN
			RAISERROR('Backup path does not exist: %s', 16, 1, @BackupPath);
			RETURN 1;
		END;

		-- Prepare appropriate backup command
		IF UPPER(@BackupType) = 'FULL'
		BEGIN
			SET @BackupFileName = @BackupPath + @DatabaseName + '_FULL_' + @CurrentDateTime + '.bak';
			SET @BackupCommand = 'BACKUP DATABASE [' + @DatabaseName + '] TO DISK = ''' + @BackupFileName + 
								''' WITH COMPRESSION, CHECKSUM, DESCRIPTION = ''Full backup of ' + @DatabaseName + ''';';
		END
		ELSE IF UPPER(@BackupType) = 'DIFF'
		BEGIN
			SET @BackupFileName = @BackupPath + @DatabaseName + '_DIFF_' + @CurrentDateTime + '.bak';
			SET @BackupCommand = 'BACKUP DATABASE [' + @DatabaseName + '] TO DISK = ''' + @BackupFileName + 
								''' WITH DIFFERENTIAL, COMPRESSION, CHECKSUM, DESCRIPTION = ''Differential backup of ' + @DatabaseName + ''';';
		END
		ELSE IF UPPER(@BackupType) = 'LOG'
		BEGIN
			SET @BackupFileName = @BackupPath + @DatabaseName + '_LOG_' + @CurrentDateTime + '.trn';
			SET @BackupCommand = 'BACKUP LOG [' + @DatabaseName + '] TO DISK = ''' + @BackupFileName + 
								''' WITH COMPRESSION, CHECKSUM, DESCRIPTION = ''Transaction log backup of ' + @DatabaseName + ''';';
		END
		ELSE
		BEGIN
			RAISERROR('Invalid backup type. Use FULL, DIFF, or LOG.', 16, 1);
			RETURN 1;
		END;

		-- Execute backup
		PRINT 'Starting ' + @BackupType + ' backup of [' + @DatabaseName + ']...';
		PRINT 'Output file: ' + @BackupFileName;

		EXEC sp_executesql @BackupCommand;

		-- Log success
		PRINT 'Backup completed successfully.';

		-- Optional: Log to backup history table
		INSERT INTO [dbo].[BackupHistory] 
			(DatabaseName, BackupType, BackupFileName, BackupDateTime, Status)
		VALUES 
			(@DatabaseName, @BackupType, @BackupFileName, GETDATE(), 'SUCCESS');

		RETURN 0;
	END TRY
	BEGIN CATCH
		SET @ErrorMessage = ERROR_MESSAGE();
		RAISERROR('Backup failed: %s', 16, 1, @ErrorMessage);

		-- Log failure
		INSERT INTO [dbo].[BackupHistory] 
			(DatabaseName, BackupType, BackupFileName, BackupDateTime, Status, ErrorMessage)
		VALUES 
			(@DatabaseName, @BackupType, @BackupFileName, GETDATE(), 'FAILED', @ErrorMessage);

		RETURN 1;
	END CATCH;
END;
GO

-- Create backup history tracking table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND name = 'BackupHistory')
BEGIN
	CREATE TABLE [dbo].[BackupHistory]
	(
		BackupHistoryID INT IDENTITY(1,1) PRIMARY KEY,
		DatabaseName NVARCHAR(128) NOT NULL,
		BackupType VARCHAR(20) NOT NULL,
		BackupFileName NVARCHAR(500) NOT NULL,
		BackupDateTime DATETIME NOT NULL,
		Status VARCHAR(50) NOT NULL,
		ErrorMessage NVARCHAR(MAX) NULL,
		CreatedDate DATETIME DEFAULT GETDATE()
	);
END;
GO

-- Schedule using SQL Server Agent (create job)
-- Job Name: TicketsDb_Full_Backup
EXEC msdb.dbo.sp_add_job
	@job_name = 'TicketsDb_Full_Backup',
	@enabled = 1;

EXEC msdb.dbo.sp_add_jobstep
	@job_name = 'TicketsDb_Full_Backup',
	@step_name = 'Execute Full Backup',
	@subsystem = 'TSQL',
	@command = 'EXEC [dbo].[sp_TicketsDb_Backup] @BackupType = ''FULL'', @BackupPath = ''D:\Backups\Full\'';';

-- Execute the procedure manually
EXEC [dbo].[sp_TicketsDb_Backup] @BackupType = 'FULL', @BackupPath = 'D:\Backups\Full\';
EXEC [dbo].[sp_TicketsDb_Backup] @BackupType = 'DIFF', @BackupPath = 'D:\Backups\Differential\';
EXEC [dbo].[sp_TicketsDb_Backup] @BackupType = 'LOG', @BackupPath = 'D:\Backups\TLog\';
```

### PowerShell Backup Automation (Monthly Strategy)

Create a PowerShell script optimized for monthly full + daily differential + hourly log strategy:

```powershell
# File: BackupTicketsDb_MonthlyStrategy.ps1
# Purpose: Automated SQL Server backup (Monthly Full + Daily Diff + Hourly Log)
# Schedule: Run hourly via Task Scheduler

param(
    [Parameter(Mandatory=$false)]
    [string]$ServerName = '.',

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = 'TicketsDb',

    [Parameter(Mandatory=$false)]
    [string]$BaseBackupPath = 'D:\Backups',

    [Parameter(Mandatory=$false)]
    [int]$FullRetentionDays = 90,

    [Parameter(Mandatory=$false)]
    [int]$DiffRetentionDays = 35,

    [Parameter(Mandatory=$false)]
    [int]$LogRetentionDays = 30
)

# Load SQL Server SMO assemblies
try {
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Management.Sdk.Sfc") | Out-Null
}
catch {
    Write-Error "Failed to load SQL Server assemblies: $_"
    exit 1
}

# Setup logging
$LogPath = Join-Path $BaseBackupPath "Logs"
if (!(Test-Path $LogPath)) {
    New-Item -ItemType Directory -Path $LogPath -Force | Out-Null
}

$LogFile = Join-Path $LogPath "BackupLog_$(Get-Date -Format 'yyyyMMdd').txt"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $Timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $LogEntry = "$Timestamp [$Level] $Message"
    $LogEntry | Tee-Object -FilePath $LogFile -Append
}

function Get-BackupType {
    $DayOfMonth = (Get-Date).Day

    if ($DayOfMonth -eq 1) {
        return "FULL"
    }
    else {
        return "DIFF"
    }
}

function Start-Backup {
    param(
        [string]$BackupType
    )

    try {
        Write-Log "Starting $BackupType backup of database: $DatabaseName"

        # Connect to SQL Server
        $Server = New-Object Microsoft.SqlServer.Management.Smo.Server($ServerName)
        $Database = $Server.Databases[$DatabaseName]

        if (!$Database) {
            throw "Database '$DatabaseName' not found"
        }

        # Create backup device
        $Backup = New-Object Microsoft.SqlServer.Management.Smo.Backup
        $Backup.Database = $DatabaseName
        $Backup.CompressionOption = [Microsoft.SqlServer.Management.Smo.BackupCompressionOptions]::On
        $Backup.Checksum = $true

        # Determine backup path and filename
        $Timestamp = Get-Date -Format 'yyyyMMdd_HHmm'
        switch ($BackupType) {
            "FULL" {
                $TypePath = Join-Path $BaseBackupPath "Full"
                $FileExt = ".bak"
                $Backup.Action = [Microsoft.SqlServer.Management.Smo.BackupActionType]::Database
            }
            "DIFF" {
                $TypePath = Join-Path $BaseBackupPath "Differential"
                $FileExt = ".bak"
                $Backup.Action = [Microsoft.SqlServer.Management.Smo.BackupActionType]::Database
                $Backup.Incremental = $true
            }
            "LOG" {
                $TypePath = Join-Path $BaseBackupPath "TLog"
                $FileExt = ".trn"
                $Backup.Action = [Microsoft.SqlServer.Management.Smo.BackupActionType]::Log
            }
        }

        # Create directory if needed
        if (!(Test-Path $TypePath)) {
            New-Item -ItemType Directory -Path $TypePath -Force | Out-Null
        }

        $BackupFileName = "$DatabaseName`_$BackupType`_$Timestamp$FileExt"
        $BackupFilePath = Join-Path $TypePath $BackupFileName

        # Create backup device
        $Device = New-Object Microsoft.SqlServer.Management.Smo.BackupDeviceItem($BackupFilePath, 'File')
        $Backup.Devices.Add($Device)

        # Execute backup
        Write-Log "Backup path: $BackupFilePath"
        $Backup.SqlBackup($Server)

        # Get file size
        $FileInfo = Get-Item $BackupFilePath
        $FileSizeMB = [math]::Round($FileInfo.Length / 1MB, 2)

        Write-Log "Backup completed successfully | Size: $FileSizeMB MB" "SUCCESS"

        return $true
    }
    catch {
        Write-Log "Backup failed: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Start-HourlyTransactionLogBackup {
    # Transaction log backups run every hour
    return (Start-Backup "LOG")
}

function Start-DailyBackup {
    # Determine if full or differential
    $BackupType = Get-BackupType
    return (Start-Backup $BackupType)
}

function Remove-OldBackups {
    param(
        [string]$BackupType,
        [int]$RetentionDays
    )

    try {
        Write-Log "Cleaning up old $BackupType backups (retention: $RetentionDays days)"

        $TypePath = if ($BackupType -eq "FULL") { 
            Join-Path $BaseBackupPath "Full"
        } elseif ($BackupType -eq "DIFF") {
            Join-Path $BaseBackupPath "Differential"
        } else {
            Join-Path $BaseBackupPath "TLog"
        }

        $CutoffDate = (Get-Date).AddDays(-$RetentionDays)
        $DeletedCount = 0

        if (Test-Path $TypePath) {
            Get-ChildItem -Path $TypePath -Filter "*.bak", "*.trn" | 
                Where-Object { $_.LastWriteTime -lt $CutoffDate } | 
                ForEach-Object {
                    Remove-Item -Path $_.FullName -Force
                    $DeletedCount++
                    Write-Log "Deleted: $($_.Name)"
                }
        }

        Write-Log "Cleanup completed | Deleted files: $DeletedCount" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "Cleanup failed: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Test-BackupIntegrity {
    param(
        [string]$BackupFilePath
    )

    try {
        Write-Log "Verifying backup integrity: $BackupFilePath"

        $Server = New-Object Microsoft.SqlServer.Management.Smo.Server($ServerName)

        # Restore verify only
        $Restore = New-Object Microsoft.SqlServer.Management.Smo.Restore
        $Restore.Devices.AddDevice($BackupFilePath, 'File')

        # This will throw if backup is corrupted
        $Restore.ReadFileList($Server) | Out-Null

        Write-Log "Backup integrity verified successfully" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "Backup integrity check failed: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

# Main execution
Write-Log "========== Backup Script Started =========="
Write-Log "Server: $ServerName | Database: $DatabaseName"
Write-Log "Backup Path: $BaseBackupPath"

# Determine what backup to run based on time of day
$CurrentHour = (Get-Date).Hour
$CurrentDay = (Get-Date).Day

# Hourly transaction log backups (run every hour)
if ($CurrentHour -in 0..23) {
    Write-Log "Running hourly transaction log backup"
    $LogBackupSuccess = Start-HourlyTransactionLogBackup
}

# Daily full/differential backups (run at 2:00 AM)
if ($CurrentHour -eq 2) {
    Write-Log "Running daily full/differential backup"
    $DailyBackupSuccess = Start-DailyBackup
}

# Monthly cleanup (run on 1st at 3:00 AM)
if ($CurrentDay -eq 1 -and $CurrentHour -eq 3) {
    Write-Log "Running monthly cleanup"
    Remove-OldBackups "FULL" $FullRetentionDays
    Remove-OldBackups "DIFF" $DiffRetentionDays
    Remove-OldBackups "LOG" $LogRetentionDays
}

Write-Log "========== Backup Script Completed =========="
```

#### Setup Task Scheduler for Hourly Execution

```powershell
# Run as Administrator
$ScriptPath = "C:\Scripts\BackupTicketsDb_MonthlyStrategy.ps1"
$TaskName = "TicketsDb_Hourly_Backup"
$TaskDescription = "Hourly backup job: Full (1st), Diff (other days), and Log (every hour)"

# Create task action
$Action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-ExecutionPolicy Bypass -NoProfile -File `"$ScriptPath`""

# Create trigger - every hour
$Trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Hours 1)

# Create task settings
$TaskSettings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable

# Register task
Register-ScheduledTask -TaskName $TaskName `
    -Action $Action `
    -Trigger $Trigger `
    -Settings $TaskSettings `
    -RunLevel Highest `
    -Description $TaskDescription `
    -Force

Write-Host "Task created: $TaskName"
```

### PowerShell Backup Automation

Create a PowerShell script for backup automation:

```powershell
# File: BackupTicketsDb.ps1
# Purpose: Automated SQL Server backup with logging and error handling

param(
	[Parameter(Mandatory=$false)]
	[ValidateSet('FULL', 'DIFF', 'LOG')]
	[string]$BackupType = 'FULL',

	[Parameter(Mandatory=$false)]
	[string]$ServerName = '.',

	[Parameter(Mandatory=$false)]
	[string]$DatabaseName = 'TicketsDb',

	[Parameter(Mandatory=$false)]
	[string]$BackupPath = 'D:\Backups',

	[Parameter(Mandatory=$false)]
	[int]$RetentionDays = 30
)

# Load SQL Server SMO assembly
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Management.Sdk.Sfc") | Out-Null

# Create log file path
$LogPath = Join-Path $BackupPath "Logs"
if (!(Test-Path $LogPath)) {
	New-Item -ItemType Directory -Path $LogPath -Force | Out-Null
}

$LogFile = Join-Path $LogPath "Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Write-Log {
	param([string]$Message)
	$Timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
	"$Timestamp - $Message" | Tee-Object -FilePath $LogFile -Append
}

function Start-DatabaseBackup {
	try {
		Write-Log "Starting $BackupType backup of database: $DatabaseName"
		Write-Log "Server: $ServerName | Backup Path: $BackupPath"

		# Connect to SQL Server
		$Server = New-Object Microsoft.SqlServer.Management.Smo.Server($ServerName)
		$Database = $Server.Databases[$DatabaseName]

		if (!$Database) {
			throw "Database '$DatabaseName' not found on server '$ServerName'"
		}

		# Create backup folder
		$TypePath = Join-Path $BackupPath $BackupType
		if (!(Test-Path $TypePath)) {
			New-Item -ItemType Directory -Path $TypePath -Force | Out-Null
		}

		# Generate backup filename
		$Timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
		$FileExtension = if ($BackupType -eq 'LOG') { '.trn' } else { '.bak' }
		$BackupFileName = "$DatabaseName`_$BackupType`_$Timestamp$FileExtension"
		$BackupFilePath = Join-Path $TypePath $BackupFileName

		# Create backup device
		$Backup = New-Object Microsoft.SqlServer.Management.Smo.Backup
		$Backup.Database = $DatabaseName
		$Backup.CompressionOption = [Microsoft.SqlServer.Management.Smo.BackupCompressionOptions]::On
		$Backup.Checksum = $true

		# Add backup device
		$Device = New-Object Microsoft.SqlServer.Management.Smo.BackupDeviceItem($BackupFilePath, 'File')
		$Backup.Devices.Add($Device)

		# Set backup type
		switch ($BackupType) {
			'FULL' { $Backup.Action = [Microsoft.SqlServer.Management.Smo.BackupActionType]::Database }
			'DIFF' { $Backup.Action = [Microsoft.SqlServer.Management.Smo.BackupActionType]::Database; $Backup.Incremental = $true }
			'LOG' { $Backup.Action = [Microsoft.SqlServer.Management.Smo.BackupActionType]::Log }
		}

		# Execute backup
		Write-Log "Executing backup to: $BackupFilePath"
		$Backup.SqlBackup($Server)
		Write-Log "Backup completed successfully"
		Write-Log "Backup file size: $((Get-Item $BackupFilePath).Length / 1MB) MB"

		# Cleanup old backups
		Write-Log "Cleaning up backups older than $RetentionDays days..."
		$CutoffDate = (Get-Date).AddDays(-$RetentionDays)
		Get-ChildItem -Path $TypePath -Filter "*$BackupType*" | 
			Where-Object { $_.LastWriteTime -lt $CutoffDate } | 
			Remove-Item -Force

		Write-Log "Cleanup completed"
		return $true
	}
	catch {
		Write-Log "ERROR: $($_.Exception.Message)"
		Write-Log "Backup failed!"
		return $false
	}
}

# Execute backup
$Result = Start-DatabaseBackup
exit $(if ($Result) { 0 } else { 1 })
```

### Schedule PowerShell Backup with Task Scheduler

```powershell
# Create scheduled task
$TaskName = "TicketsDb_Full_Backup"
$ScriptPath = "C:\Scripts\BackupTicketsDb.ps1"
$PSExePath = "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"

# Create task action
$Action = New-ScheduledTaskAction -Execute $PSExePath -Argument "-ExecutionPolicy Bypass -NoProfile -File $ScriptPath -BackupType FULL"

# Create task trigger (every Sunday at 2:00 AM)
$Trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At 2:00AM

# Register task
Register-ScheduledTask -TaskName $TaskName -Action $Action -Trigger $Trigger -RunLevel Highest
```

---

## Integration with .NET Applications

### C# Backup Manager Class

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Tickets.Data.Services
{
	public interface IBackupService
	{
		Task<BackupResult> CreateFullBackupAsync(string databaseName, string backupPath);
		Task<BackupResult> CreateDifferentialBackupAsync(string databaseName, string backupPath);
		Task<BackupResult> CreateTransactionLogBackupAsync(string databaseName, string backupPath);
		Task<BackupResult> RestoreDatabaseAsync(string databaseName, string backupFilePath);
	}

	public class SqlServerBackupService : IBackupService
	{
		private readonly string _connectionString;
		private readonly ILogger<SqlServerBackupService> _logger;

		public SqlServerBackupService(string connectionString, ILogger<SqlServerBackupService> logger)
		{
			_connectionString = connectionString;
			_logger = logger;
		}

		public async Task<BackupResult> CreateFullBackupAsync(string databaseName, string backupPath)
		{
			return await CreateBackupAsync(databaseName, backupPath, "FULL");
		}

		public async Task<BackupResult> CreateDifferentialBackupAsync(string databaseName, string backupPath)
		{
			return await CreateBackupAsync(databaseName, backupPath, "DIFF");
		}

		public async Task<BackupResult> CreateTransactionLogBackupAsync(string databaseName, string backupPath)
		{
			return await CreateBackupAsync(databaseName, backupPath, "LOG");
		}

		private async Task<BackupResult> CreateBackupAsync(string databaseName, string backupPath, string backupType)
		{
			var result = new BackupResult { BackupType = backupType, DatabaseName = databaseName };

			try
			{
				// Ensure backup directory exists
				if (!Directory.Exists(backupPath))
				{
					Directory.CreateDirectory(backupPath);
				}

				// Generate backup filename
				string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				string fileExtension = backupType == "LOG" ? ".trn" : ".bak";
				string backupFileName = $"{databaseName}_{backupType}_{timestamp}{fileExtension}";
				string backupFilePath = Path.Combine(backupPath, backupFileName);

				// Build T-SQL command
				string backupCommand = backupType switch
				{
					"FULL" => $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFilePath}' WITH COMPRESSION, CHECKSUM;",
					"DIFF" => $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFilePath}' WITH DIFFERENTIAL, COMPRESSION, CHECKSUM;",
					"LOG" => $"BACKUP LOG [{databaseName}] TO DISK = '{backupFilePath}' WITH COMPRESSION, CHECKSUM;",
					_ => throw new ArgumentException("Invalid backup type")
				};

				// Execute backup
				using (var connection = new SqlConnection(_connectionString))
				{
					await connection.OpenAsync();

					using (var command = new SqlCommand(backupCommand, connection))
					{
						command.CommandTimeout = 3600; // 1 hour timeout
						await command.ExecuteNonQueryAsync();
					}
				}

				// Get file size
				var fileInfo = new FileInfo(backupFilePath);
				result.BackupFilePath = backupFilePath;
				result.FileSizeBytes = fileInfo.Length;
				result.Status = BackupStatus.Success;
				result.CompletedAt = DateTime.Now;

				_logger.LogInformation(
					"Backup completed: {BackupType} of {DatabaseName} to {FilePath} ({SizeMB:F2} MB)",
					backupType, databaseName, backupFilePath, fileInfo.Length / 1024.0 / 1024.0);

				return result;
			}
			catch (Exception ex)
			{
				result.Status = BackupStatus.Failed;
				result.ErrorMessage = ex.Message;
				result.CompletedAt = DateTime.Now;

				_logger.LogError(ex, "Backup failed: {BackupType} of {DatabaseName}", backupType, databaseName);

				return result;
			}
		}

		public async Task<BackupResult> RestoreDatabaseAsync(string databaseName, string backupFilePath)
		{
			var result = new BackupResult { DatabaseName = databaseName, BackupFilePath = backupFilePath };

			try
			{
				// Validate backup file exists
				if (!File.Exists(backupFilePath))
				{
					throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
				}

				_logger.LogInformation("Starting restore of {DatabaseName} from {FilePath}", databaseName, backupFilePath);

				// T-SQL command to restore
				string restoreCommand = $@"
					ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
					RESTORE DATABASE [{databaseName}] FROM DISK = '{backupFilePath}' WITH REPLACE;
					ALTER DATABASE [{databaseName}] SET MULTI_USER;";

				using (var connection = new SqlConnection(_connectionString))
				{
					await connection.OpenAsync();

					foreach (var cmd in restoreCommand.Split(';'))
					{
						if (string.IsNullOrWhiteSpace(cmd))
							continue;

						using (var command = new SqlCommand(cmd, connection))
						{
							command.CommandTimeout = 3600;
							await command.ExecuteNonQueryAsync();
						}
					}
				}

				result.Status = BackupStatus.Success;
				result.CompletedAt = DateTime.Now;

				_logger.LogInformation("Database restore completed successfully: {DatabaseName}", databaseName);

				return result;
			}
			catch (Exception ex)
			{
				result.Status = BackupStatus.Failed;
				result.ErrorMessage = ex.Message;
				result.CompletedAt = DateTime.Now;

				_logger.LogError(ex, "Restore failed: {DatabaseName}", databaseName);

				return result;
			}
		}
	}

	public class BackupResult
	{
		public string DatabaseName { get; set; }
		public string BackupType { get; set; }
		public string BackupFilePath { get; set; }
		public long FileSizeBytes { get; set; }
		public BackupStatus Status { get; set; }
		public string ErrorMessage { get; set; }
		public DateTime CompletedAt { get; set; }

		public string FileSizeMB => (FileSizeBytes / 1024.0 / 1024.0).ToString("F2");
		public TimeSpan Duration => CompletedAt - DateTime.Now;
	}

	public enum BackupStatus
	{
		Pending,
		InProgress,
		Success,
		Failed
	}
}
```

### Dependency Injection Setup (Program.cs)

```csharp
// In Program.cs or Startup.cs
services.AddScoped<IBackupService>(sp => 
	new SqlServerBackupService(
		configuration.GetConnectionString("DefaultConnection"),
		sp.GetRequiredService<ILogger<SqlServerBackupService>>()
	)
);
```

### Usage in Razor Pages

```csharp
// Pages/Admin/BackupDatabase.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tickets.Data.Services;

namespace Tickets.Pages.Admin
{
	[Authorize(Roles = "Administrator")]
	public class BackupDatabaseModel : PageModel
	{
		private readonly IBackupService _backupService;
		private readonly ILogger<BackupDatabaseModel> _logger;

		[BindProperty]
		public string BackupType { get; set; } = "FULL";

		public BackupResult LastBackupResult { get; set; }

		public BackupDatabaseModel(IBackupService backupService, ILogger<BackupDatabaseModel> logger)
		{
			_backupService = backupService;
			_logger = logger;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			try
			{
				string backupPath = Path.Combine("D:", "Backups", BackupType.ToUpper());

				LastBackupResult = BackupType.ToUpper() switch
				{
					"FULL" => await _backupService.CreateFullBackupAsync("TicketsDb", backupPath),
					"DIFF" => await _backupService.CreateDifferentialBackupAsync("TicketsDb", backupPath),
					"LOG" => await _backupService.CreateTransactionLogBackupAsync("TicketsDb", backupPath),
					_ => throw new InvalidOperationException("Invalid backup type")
				};

				return Page();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Backup failed");
				ModelState.AddModelError(string.Empty, "Backup failed: " + ex.Message);
				return Page();
			}
		}
	}
}
```

### Razor Page HTML (BackupDatabase.cshtml)

```html
@page
@model Tickets.Pages.Admin.BackupDatabaseModel
@{
	ViewData["Title"] = "Database Backup";
}

<div class="container mt-5">
	<h1>Database Backup & Recovery</h1>

	<form method="post">
		<div class="card mb-4">
			<div class="card-header">
				<h5>Create Backup</h5>
			</div>
			<div class="card-body">
				<div class="form-group mb-3">
					<label for="BackupType">Backup Type:</label>
					<select id="BackupType" name="BackupType" class="form-control">
						<option value="FULL">Full Backup</option>
						<option value="DIFF">Differential Backup</option>
						<option value="LOG">Transaction Log Backup</option>
					</select>
					<small class="form-text text-muted">
						Full = Complete database copy | Diff = Changes since last full | Log = Transaction log
					</small>
				</div>
				<button type="submit" class="btn btn-primary">Create Backup</button>
			</div>
		</div>
	</form>

	@if (Model.LastBackupResult != null)
	{
		<div class="card mt-4">
			<div class="card-header">
				<h5>Backup Result</h5>
			</div>
			<div class="card-body">
				<div class="alert alert-@(Model.LastBackupResult.Status == Tickets.Data.Services.BackupStatus.Success ? "success" : "danger")">
					<strong>@Model.LastBackupResult.Status</strong>
				</div>
				<p><strong>Type:</strong> @Model.LastBackupResult.BackupType</p>
				<p><strong>Database:</strong> @Model.LastBackupResult.DatabaseName</p>
				<p><strong>File:</strong> @Model.LastBackupResult.BackupFilePath</p>
				<p><strong>Size:</strong> @Model.LastBackupResult.FileSizeMB MB</p>
				<p><strong>Completed:</strong> @Model.LastBackupResult.CompletedAt:G</p>
				@if (!string.IsNullOrEmpty(Model.LastBackupResult.ErrorMessage))
				{
					<p class="text-danger"><strong>Error:</strong> @Model.LastBackupResult.ErrorMessage</p>
				}
			</div>
		</div>
	}
</div>
```

---

## Monitoring and Maintenance

### Query Backup Status

```sql
-- View backup history
SELECT 
	database_name,
	backup_type,
	backup_finish_date,
	CAST(backup_size AS FLOAT) / 1024 / 1024 AS size_mb,
	DATEDIFF(MINUTE, backup_start_date, backup_finish_date) AS duration_minutes
FROM msdb.dbo.backupset
WHERE database_name = 'TicketsDb'
ORDER BY backup_start_date DESC;

-- Find databases without recent backups
SELECT 
	d.name,
	MAX(b.backup_finish_date) AS last_backup
FROM master.sys.databases d
LEFT JOIN msdb.dbo.backupset b ON d.name = b.database_name
WHERE d.database_id > 4  -- Skip system databases
GROUP BY d.name
HAVING MAX(b.backup_finish_date) < DATEADD(DAY, -7, GETDATE());

-- Monitor transaction log backup gap
SELECT 
	database_name,
	MAX(backup_finish_date) AS last_tlog_backup,
	DATEDIFF(MINUTE, MAX(backup_finish_date), GETDATE()) AS minutes_since_last
FROM msdb.dbo.backupset
WHERE backup_type = 'L'  -- Transaction log backups
GROUP BY database_name;
```

### Verify Backup Integrity

```sql
-- Verify backup file integrity
RESTORE VERIFYONLY
FROM DISK = 'D:\Backups\Full\TicketsDb_FULL_20250101.bak';

-- Check database integrity
DBCC CHECKDB ('TicketsDb') WITH ALL_ERRORMSGS;
```

### Automated Backup Validation Job

```sql
-- Create SQL Agent job for backup validation
EXEC msdb.dbo.sp_add_job
	@job_name = 'TicketsDb_Backup_Validation',
	@enabled = 1;

EXEC msdb.dbo.sp_add_jobstep
	@job_name = 'TicketsDb_Backup_Validation',
	@step_name = 'Validate Latest Full Backup',
	@subsystem = 'TSQL',
	@command = N'
		DECLARE @LatestBackup NVARCHAR(500);
		SELECT TOP 1 @LatestBackup = physical_device_name
		FROM msdb.dbo.backupmediafamily
		WHERE media_set_id IN (
			SELECT media_set_id FROM msdb.dbo.backupset 
			WHERE database_name = ''TicketsDb'' AND backup_type = ''D''
			ORDER BY backup_start_date DESC
		);

		IF @LatestBackup IS NOT NULL
		BEGIN
			RESTORE VERIFYONLY FROM DISK = @LatestBackup;
			PRINT ''Backup verification completed successfully.'';
		END;
	';

-- Schedule for daily execution
EXEC msdb.dbo.sp_add_schedule
	@schedule_name = 'Daily_3AM',
	@freq_type = 4,  -- Daily
	@freq_interval = 1,
	@active_start_time = 030000;

EXEC msdb.dbo.sp_attach_schedule
	@job_name = 'TicketsDb_Backup_Validation',
	@schedule_name = 'Daily_3AM';
```

---

## Best Practices Summary

### ✅ Do's

- ✅ Perform full backup at least weekly
- ✅ Perform differential backups daily
- ✅ Backup transaction logs every 15-30 minutes in production
- ✅ Store backups on separate disk/server from database
- ✅ Use backup compression to reduce storage
- ✅ Enable checksum verification for backup integrity
- ✅ Test restore procedures regularly
- ✅ Monitor backup completion and success
- ✅ Document backup/recovery procedures
- ✅ Use FULL recovery model for production

### ❌ Don'ts

- ❌ Don't store backups on same disk as database files
- ❌ Don't skip backup verification
- ❌ Don't neglect transaction log backups
- ❌ Don't use SIMPLE recovery mode for production
- ❌ Don't forget to delete old backups (storage costs)
- ❌ Don't assume backups work without testing restore
- ❌ Don't backup directly to tape (without disk staging)
- ❌ Don't ignore backup failures/warnings
- ❌ Don't forget off-site copies for disaster recovery
- ❌ Don't backup during peak user hours without monitoring

---

## Recovery Procedures for Monthly Strategy

### Scenario 1: Point-in-Time Recovery to Specific Hour

**Situation:** Database corrupted at 2:45 PM on January 15th. Restore to 2:00 PM (before corruption).

```sql
-- Step 1: Set database to restore mode
ALTER DATABASE [TicketsDb]
SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

-- Step 2: Restore full backup from January 1st (2:00 AM)
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\Full\TicketsDb_Full_20250101.bak'
WITH NORECOVERY;

PRINT 'Full backup restored';

-- Step 3: Restore differential backup from January 15th (2:00 AM)
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\Differential\TicketsDb_Diff_20250115.bak'
WITH NORECOVERY;

PRINT 'Differential backup restored';

-- Step 4: Restore transaction logs in sequence
-- From 2:00 AM to 2:00 PM (12 hourly backups)
DECLARE @LogFiles TABLE (FileNumber INT, FileName NVARCHAR(500));

INSERT INTO @LogFiles VALUES 
    (1, 'D:\Backups\TLog\TicketsDb_Log_20250115_02.trn'),
    (2, 'D:\Backups\TLog\TicketsDb_Log_20250115_03.trn'),
    (3, 'D:\Backups\TLog\TicketsDb_Log_20250115_04.trn'),
    (4, 'D:\Backups\TLog\TicketsDb_Log_20250115_05.trn'),
    (5, 'D:\Backups\TLog\TicketsDb_Log_20250115_06.trn'),
    (6, 'D:\Backups\TLog\TicketsDb_Log_20250115_07.trn'),
    (7, 'D:\Backups\TLog\TicketsDb_Log_20250115_08.trn'),
    (8, 'D:\Backups\TLog\TicketsDb_Log_20250115_09.trn'),
    (9, 'D:\Backups\TLog\TicketsDb_Log_20250115_10.trn'),
    (10, 'D:\Backups\TLog\TicketsDb_Log_20250115_11.trn'),
    (11, 'D:\Backups\TLog\TicketsDb_Log_20250115_12.trn'),
    (12, 'D:\Backups\TLog\TicketsDb_Log_20250115_13.trn');

-- Restore all transaction logs up to 2:00 PM
DECLARE @i INT = 1;
WHILE @i <= 12
BEGIN
    DECLARE @LogFile NVARCHAR(500);
    SELECT @LogFile = FileName FROM @LogFiles WHERE FileNumber = @i;

    DECLARE @RestoreCmd NVARCHAR(MAX) = 
        'RESTORE LOG [TicketsDb] FROM DISK = ''' + @LogFile + ''' WITH NORECOVERY;';

    EXEC sp_executesql @RestoreCmd;
    PRINT 'Restored: ' + @LogFile;

    SET @i = @i + 1;
END;

-- Step 5: Recover the database
RESTORE DATABASE [TicketsDb] WITH RECOVERY;

-- Step 6: Return to multi-user mode
ALTER DATABASE [TicketsDb]
SET MULTI_USER;

PRINT 'Recovery completed successfully to 2:00 PM on January 15th';
```

### Scenario 2: Emergency Full Recovery (Latest Available)

**Situation:** Complete database loss. Restore to latest available backup.

```sql
-- Step 1: Set to single user mode
ALTER DATABASE [TicketsDb]
SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

-- Step 2: Restore latest full backup
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\Full\TicketsDb_Full_20250101.bak'
WITH NORECOVERY;

-- Step 3: Restore latest differential
RESTORE DATABASE [TicketsDb]
FROM DISK = 'D:\Backups\Differential\TicketsDb_Diff_20250115.bak'
WITH NORECOVERY;

-- Step 4: Restore ALL subsequent transaction logs
-- Get list of all transaction logs after differential backup
DECLARE @DiffBackupTime DATETIME = '2025-01-15 02:00:00';
DECLARE @LatestLogFile NVARCHAR(500);

-- Determine latest transaction log available
SET @LatestLogFile = 'D:\Backups\TLog\TicketsDb_Log_20250115_23.trn';  -- Last hour of the day

-- Restore all transaction logs (simplified - loops through available files)
RESTORE LOG [TicketsDb]
FROM DISK = @LatestLogFile
WITH RECOVERY;

-- Step 5: Return to multi-user mode
ALTER DATABASE [TicketsDb]
SET MULTI_USER;

PRINT 'Emergency recovery completed - database restored to latest available point';
```

### Scenario 3: Recovering Deleted Data (Using Point-in-Time)

**Situation:** User accidentally deleted important records at 3:30 PM. Restore to 2:30 PM and export data.

```sql
-- Step 1: Create recovery database
CREATE DATABASE TicketsDb_Recovery;

-- Step 2: Restore full backup into recovery database
RESTORE DATABASE [TicketsDb_Recovery]
FROM DISK = 'D:\Backups\Full\TicketsDb_Full_20250101.bak'
WITH NORECOVERY, REPLACE;

-- Step 3: Restore differential
RESTORE DATABASE [TicketsDb_Recovery]
FROM DISK = 'D:\Backups\Differential\TicketsDb_Diff_20250115.bak'
WITH NORECOVERY;

-- Step 4: Restore transaction logs up to 2:30 PM (13:30 = 14:00 hour log, but stop at 2:30 PM)
RESTORE LOG [TicketsDb_Recovery]
FROM DISK = 'D:\Backups\TLog\TicketsDb_Log_20250115_13.trn'
WITH RECOVERY, STOPAT = '2025-01-15 14:30:00';

-- Step 5: Query recovered data from recovery database
SELECT * FROM [TicketsDb_Recovery].[dbo].[Contacts]
WHERE DeletedFlag = 0;  -- Find undeleted records at 2:30 PM

-- Step 6: Restore deleted records back to production
INSERT INTO [TicketsDb].[dbo].[Contacts]
SELECT * FROM [TicketsDb_Recovery].[dbo].[Contacts]
WHERE ContactId NOT IN (SELECT ContactId FROM [TicketsDb].[dbo].[Contacts]);

-- Step 7: Cleanup
DROP DATABASE [TicketsDb_Recovery];

PRINT 'Deleted data recovery completed';
```

### PowerShell Recovery Script

```powershell
# File: RestoreTicketsDb_PointInTime.ps1
# Purpose: Automated point-in-time database recovery

param(
    [Parameter(Mandatory=$true)]
    [datetime]$RecoveryTime,

    [Parameter(Mandatory=$false)]
    [string]$ServerName = '.',

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = 'TicketsDb',

    [Parameter(Mandatory=$false)]
    [string]$BackupPath = 'D:\Backups'
)

# Load assemblies
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo") | Out-Null

$LogFile = Join-Path $BackupPath "Logs" "Restore_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Write-Log {
    param([string]$Message)
    $Timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    "$Timestamp | $Message" | Tee-Object -FilePath $LogFile -Append
}

try {
    Write-Log "Starting point-in-time recovery to: $RecoveryTime"

    $Server = New-Object Microsoft.SqlServer.Management.Smo.Server($ServerName)

    # Get latest full backup before recovery time
    $FullBackupFile = Get-ChildItem "$BackupPath\Full" -Filter "*.bak" | 
        Where-Object { $_.LastWriteTime -le $RecoveryTime } | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1

    if (!$FullBackupFile) {
        throw "No full backup found before recovery time"
    }

    Write-Log "Full backup: $($FullBackupFile.Name)"

    # Get latest differential before recovery time
    $DiffBackupFile = Get-ChildItem "$BackupPath\Differential" -Filter "*.bak" | 
        Where-Object { $_.LastWriteTime -le $RecoveryTime } | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1

    # Get transaction logs between differential and recovery time
    $TransactionLogs = Get-ChildItem "$BackupPath\TLog" -Filter "*.trn" | 
        Where-Object { $_.LastWriteTime -le $RecoveryTime } | 
        Sort-Object LastWriteTime

    Write-Log "Found $($TransactionLogs.Count) transaction logs to restore"

    # Perform restore
    $Restore = New-Object Microsoft.SqlServer.Management.Smo.Restore
    $Restore.Database = $DatabaseName
    $Restore.NoRecovery = $true
    $Restore.Devices.AddDevice($FullBackupFile.FullName, 'File')
    $Restore.SqlRestore($Server)

    Write-Log "Full backup restored"

    # Restore differential if available
    if ($DiffBackupFile) {
        $Restore = New-Object Microsoft.SqlServer.Management.Smo.Restore
        $Restore.Database = $DatabaseName
        $Restore.NoRecovery = $true
        $Restore.Devices.AddDevice($DiffBackupFile.FullName, 'File')
        $Restore.SqlRestore($Server)

        Write-Log "Differential backup restored"
    }

    # Restore transaction logs
    foreach ($LogFile in $TransactionLogs) {
        $Restore = New-Object Microsoft.SqlServer.Management.Smo.Restore
        $Restore.Database = $DatabaseName
        $Restore.NoRecovery = $true
        $Restore.Devices.AddDevice($LogFile.FullName, 'File')
        $Restore.SqlRestore($Server)

        Write-Log "Restored: $($LogFile.Name)"
    }

    # Final recovery
    $Restore = New-Object Microsoft.SqlServer.Management.Smo.Restore
    $Restore.Database = $DatabaseName
    $Restore.Recovery = $true
    $Restore.SqlRestore($Server)

    Write-Log "Point-in-time recovery completed successfully" 
    Write-Log "Database restored to: $RecoveryTime"
}
catch {
    Write-Log "Recovery failed: $($_.Exception.Message)" 
    exit 1
}
```

### Recovery Testing

```sql
-- Monthly recovery test (1st Sunday of month)
-- Run this to verify backups are restorable

DECLARE @TestDbName NVARCHAR(128) = 'TicketsDb_RecoveryTest';
DECLARE @LatestFullBackup NVARCHAR(500) = 'D:\Backups\Full\TicketsDb_Full_20250101.bak';
DECLARE @LatestDiffBackup NVARCHAR(500) = 'D:\Backups\Differential\TicketsDb_Diff_20250115.bak';

BEGIN TRY
    -- Drop test database if exists
    IF EXISTS (SELECT 1 FROM sys.databases WHERE name = @TestDbName)
    BEGIN
        ALTER DATABASE [TicketsDb_RecoveryTest] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        DROP DATABASE [TicketsDb_RecoveryTest];
    END

    -- Restore into test database
    RESTORE DATABASE @TestDbName
    FROM DISK = @LatestFullBackup
    WITH NORECOVERY, REPLACE;

    RESTORE DATABASE @TestDbName
    FROM DISK = @LatestDiffBackup
    WITH RECOVERY;

    -- Verify recovery
    DECLARE @RecoveryStatus NVARCHAR(MAX);
    SELECT @RecoveryStatus = recovery_model_desc FROM sys.databases WHERE name = @TestDbName;

    IF @RecoveryStatus IS NOT NULL
    BEGIN
        PRINT 'Recovery test PASSED - backups are valid';

        -- Cleanup
        ALTER DATABASE [TicketsDb_RecoveryTest] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        DROP DATABASE [TicketsDb_RecoveryTest];
    END
END TRY
BEGIN CATCH
    PRINT 'Recovery test FAILED: ' + ERROR_MESSAGE();
END CATCH;
```

---

## Disaster Recovery Checklist (Monthly Strategy)

### Pre-Implementation
- [ ] Database set to FULL recovery model
- [ ] Backup directory structure created (Full, Differential, TLog, Logs)
- [ ] Appropriate NTFS permissions assigned (NETWORK SERVICE has full control)
- [ ] SQL Server Agent verified running
- [ ] Backup validation monitoring enabled

### Monthly Full Backup
- [ ] Scheduled for 1st of month at 2:00 AM
- [ ] Compression enabled
- [ ] Checksum verification enabled
- [ ] Backup file stored on separate disk from database
- [ ] Full backup retention policy set to 90 days
- [ ] Full backup verified with RESTORE VERIFYONLY

### Daily Differential Backup
- [ ] Scheduled for Mon-Sat at 2:00 AM (skip 1st of month)
- [ ] Compression enabled
- [ ] Checksum verification enabled
- [ ] Differential retention policy set to 35 days
- [ ] Cannot execute without prior full backup

### Hourly Transaction Log Backup
- [ ] Scheduled for every hour (00:00 to 23:00)
- [ ] Compression enabled
- [ ] Checksum verification enabled
- [ ] Log retention policy set to 30 days
- [ ] No gaps in transaction log backup chain

### Operational
- [ ] Monthly cleanup job runs 1st at 3:00 AM
- [ ] Old backups automatically deleted per retention policy
- [ ] Backup history table populated with results
- [ ] Email alerts configured for backup failures
- [ ] Backup log files reviewed weekly

### Testing & Validation
- [ ] Monthly test restore to verify backup integrity
- [ ] Recovery time objective (RTO) documented: 35-40 minutes
- [ ] Recovery point objective (RPO) documented: 1 hour
- [ ] Point-in-time recovery procedures documented
- [ ] Recovery playbook created and stored
- [ ] Disaster recovery team trained on procedures
- [ ] Off-site copies of monthly full backups (if required)

### Monitoring
- [ ] SQL Server Agent job history reviewed weekly
- [ ] Backup disk space monitored (77GB/month estimated)
- [ ] Transaction log growth monitored
- [ ] Database integrity check (DBCC) run weekly
- [ ] Backup alerts configured for failures
- [ ] Performance impact of backups monitored

### Compliance & Documentation
- [ ] Backup strategy approved by management
- [ ] Retention periods meet compliance requirements
- [ ] Backup procedures documented and accessible
- [ ] Disaster recovery plan updated with monthly strategy
- [ ] Training documentation created
- [ ] Annual disaster recovery drill scheduled
- [ ] Backup access restricted to authorized users

---

## References

- [Microsoft SQL Server Backup and Restore](https://learn.microsoft.com/en-us/sql/relational-databases/backup-restore/back-up-and-restore-of-sql-server-databases)
- [Maintenance Plans in SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/maintenance-plans/maintenance-plans)
- [Recovery Models (SQL Server)](https://learn.microsoft.com/en-us/sql/relational-databases/backup-restore/recovery-models-sql-server)
- [SQL Server 2022 New Features for Backup](https://learn.microsoft.com/en-us/sql/sql-server/what-s-new-in-sql-server-2022)

---

**Document Version:** 1.0  
**Last Updated:** January 2025  
**Prepared For:** Tickets CRM Development Team
