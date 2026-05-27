import { Component, OnInit, OnDestroy, ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminApiService } from '../../services/admin-api.service';
import DataTable, { Api } from 'datatables.net-dt';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.css']
})
export class AuditLogsComponent implements OnInit, OnDestroy {
  @ViewChild('auditTable') auditTableEl!: ElementRef;

  private dtAuditLogs: Api<any> | null = null;
  private destroy$ = new Subject<void>();

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
  entityTypes = ['Teacher', 'Student', 'ClassGroup', 'Subject', 'StudentEnrollment', 'Assignment'];
  actions = ['Created', 'Updated', 'Deleted'];

  constructor(
    private adminApi: AdminApiService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    if (!localStorage.getItem('admin_token')) {
      this.router.navigate(['/admin/login']);
      return;
    }
    this.loadAuditLogs();
  }

  ngOnDestroy(): void {
    this.destroyDataTable();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── DataTable initialization & cleanup ────────────────────────────────────────

  private destroyDataTable(): void {
    if (this.dtAuditLogs) {
      this.dtAuditLogs.destroy();
      this.dtAuditLogs = null;
    }
  }

  private initDataTable(): void {
    if (!this.auditTableEl) return;
    this.dtAuditLogs = new DataTable(this.auditTableEl.nativeElement, {
      pageLength: 25,
      lengthMenu: [10, 25, 50, 100],
      order: [[0, 'desc']],
      columnDefs: [{ orderable: false, searchable: false, targets: -1 }],
      language: { emptyTable: 'No audit logs found.' }
    });
  }

  loadAuditLogs(): void {
    this.loading = true;
    this.error = '';
    this.destroyDataTable();

    const filter = {
      entityType: this.filterEntityType || undefined,
      action: this.filterAction || undefined,
      performedBy: this.filterPerformedBy || undefined,
      startDate: this.filterStartDate ? new Date(this.filterStartDate).toISOString() : undefined,
      endDate: this.filterEndDate ? new Date(this.filterEndDate).toISOString() : undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    this.adminApi.getAuditLogs(filter)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.loading = false; })
      )
      .subscribe({
        next: (response: any) => {
          this.auditLogs = response.records || [];
          this.totalCount = response.totalCount || 0;
          this.cdr.detectChanges();
          this.initDataTable();
        },
        error: (err: any) => {
          this.error = err?.error?.message || 'Failed to load audit logs';
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
