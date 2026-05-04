import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentAuthService } from '../../services/student-auth.service';
import { StudentAssignmentService } from '../../services/assignment.service';
import { extractErrors } from '../../services/error.util';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-dashboard.component.html',
  styleUrls: ['./student-dashboard.component.css']
})
export class StudentDashboardComponent implements OnInit {
  student: any = null;
  isLoading = true;
  errors: string[] = [];

  // Tab state
  activeTab: 'assignments' | 'results' = 'assignments';

  // Assignments list
  assignments: any[] = [];
  assignmentsLoading = false;

  // Submission
  submittingId: number | null = null;
  submissionContent: { [assignmentId: number]: string } = {};
  submitSuccess: { [assignmentId: number]: boolean } = {};
  submitError:   { [assignmentId: number]: string  } = {};

  // My results
  mySubmissions: any[] = [];
  resultsLoading = false;

  constructor(
    private studentAuthService: StudentAuthService,
    private assignmentService: StudentAssignmentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.student = this.studentAuthService.getCurrentStudent();
    this.loadProfile();
    this.loadAssignments();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.studentAuthService.getProfile().subscribe({
      next: (data) => {
        this.student = data;
        this.studentAuthService.setCurrentStudent(data);
        this.isLoading = false;
      },
      error: (err) => {
        this.errors = extractErrors(err);
        this.isLoading = false;
      }
    });
  }

  // ── Assignments ───────────────────────────────────────────────────────

  loadAssignments(): void {
    this.assignmentsLoading = true;
    this.assignmentService.getMyAssignments().subscribe({
      next: (data) => { this.assignments = data; this.assignmentsLoading = false; },
      error: ()    => { this.assignmentsLoading = false; }
    });
  }

  submitAssignment(assignmentId: number): void {
    const content = this.submissionContent[assignmentId];
    if (!content?.trim()) {
      this.submitError[assignmentId] = 'Please write your answer before submitting.';
      return;
    }
    this.submittingId = assignmentId;
    this.submitError[assignmentId] = '';

    this.assignmentService.submitAssignment(assignmentId, content).subscribe({
      next: () => {
        this.submittingId = null;
        this.submitSuccess[assignmentId] = true;
        // Update assignment status in list
        const a = this.assignments.find(x => x.id === assignmentId);
        if (a) a.studentSubmissionStatus = 'Pending';
        setTimeout(() => this.submitSuccess[assignmentId] = false, 3000);
      },
      error: (err) => {
        this.submittingId = null;
        this.submitError[assignmentId] = err?.error || 'Submission failed. Please try again.';
      }
    });
  }

  // ── My Results ────────────────────────────────────────────────────────

  loadResults(): void {
    this.activeTab = 'results';
    if (this.mySubmissions.length > 0) return;  // already loaded
    this.resultsLoading = true;
    this.assignmentService.getMySubmissions().subscribe({
      next: (data) => { this.mySubmissions = data; this.resultsLoading = false; },
      error: ()    => { this.resultsLoading = false; }
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────

  isOverdue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Graded':       return 'badge-graded';
      case 'Pending':      return 'badge-pending';
      case 'Late':         return 'badge-late';
      case 'Not Submitted': return 'badge-none';
      default: return '';
    }
  }

  scorePercent(score: number, max: number): number {
    if (!max) return 0;
    return Math.round((score / max) * 100);
  }

  logout(): void {
    this.studentAuthService.logout();
    this.router.navigate(['/login']);
  }
}
