import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminApiService } from '../../services/admin-api.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.css']
})
export class AuditLogsComponent implements OnInit {
  auditLogs: any[] = [];
  loading = false;
  error = '';
  totalCount = 0;
  currentPage = 1;
  pageSize = 50;

  // Filter criteria
  filterEntityType = '';
  filterAction = '';
  filterPerformedBy = '';
  filterStartDate = '';
  filterEndDate = '';

  // Entity types and actions for dropdown
  entityTypes = ['Teacher', 'Student', 'ClassGroup', 'Course', 'StudentEnrollment', 'Assignment'];
  actions = ['Created', 'Updated', 'Deleted'];

  constructor(private adminApi: AdminApiService, private router: Router) {}

  ngOnInit(): void {
    if (!localStorage.getItem('admin_token')) {
      this.router.navigate(['/admin/login']);
      return;
    }
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.loading = true;
    this.error = '';

    const filter = {
      entityType: this.filterEntityType || undefined,
      action: this.filterAction || undefined,
      performedBy: this.filterPerformedBy || undefined,
      startDate: this.filterStartDate ? new Date(this.filterStartDate).toISOString() : undefined,
      endDate: this.filterEndDate ? new Date(this.filterEndDate).toISOString() : undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    this.adminApi.getAuditLogs(filter).subscribe({
      next: (response: any) => {
        this.auditLogs = response.records || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err?.error?.message || 'Failed to load audit logs';
        this.loading = false;
      }
    });
  }

  resetFilters(): void {
    this.filterEntityType = '';
    this.filterAction = '';
    this.filterPerformedBy = '';
    this.filterStartDate = '';
    this.filterEndDate = '';
    this.currentPage = 1;
    this.loadAuditLogs();
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadAuditLogs();
  }

  nextPage(): void {
    const totalPages = Math.ceil(this.totalCount / this.pageSize);
    if (this.currentPage < totalPages) {
      this.currentPage++;
      this.loadAuditLogs();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadAuditLogs();
    }
  }

  getPageCount(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  parseChanges(changesJson: string): any {
    try {
      return JSON.parse(changesJson || '{}');
    } catch {
      return { raw: changesJson };
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString();
  }
}
