import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TeacherAuthService } from '../../services/teacher-auth.service';
import { TeacherAssignmentService } from '../../services/assignment.service';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.css']
})
export class TeacherDashboardComponent implements OnInit {
  teacher: any = null;

  // Tab state
  activeTab: 'students' | 'assignments' | 'grade' = 'students';

  // Students
  students: any[] = [];
  studentsLoading = false;

  // Assignments
  assignments: any[] = [];
  assignmentsLoading = false;
  showCreateForm = false;
  newAssignment = { title: '', description: '', dueDate: '', maxScore: 20, classGroupId: 0 };
  createError = '';
  createSuccess = '';

  // Submissions / grading
  selectedAssignment: any = null;
  submissions: any[] = [];
  submissionsLoading = false;
  gradingMap: { [submissionId: number]: { score: number; feedback: string } } = {};
  gradeSuccess = '';

  constructor(
    private teacherAuthService: TeacherAuthService,
    private assignmentService: TeacherAssignmentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.teacher = this.teacherAuthService.getCurrentTeacher();
    this.loadStudents();
    this.loadAssignments();
  }

  // ── Students ─────────────────────────────────────────────────────────

  loadStudents(): void {
    this.studentsLoading = true;
    this.assignmentService.getMyStudents().subscribe({
      next: (data) => { this.students = data; this.studentsLoading = false; },
      error: ()    => { this.studentsLoading = false; }
    });
  }

  // ── Assignments ───────────────────────────────────────────────────────

  loadAssignments(): void {
    this.assignmentsLoading = true;
    this.assignmentService.getAssignments().subscribe({
      next: (data) => { this.assignments = data; this.assignmentsLoading = false; },
      error: ()    => { this.assignmentsLoading = false; }
    });
  }

  createAssignment(): void {
    this.createError = '';
    this.createSuccess = '';
    if (!this.newAssignment.title || !this.newAssignment.dueDate || !this.newAssignment.classGroupId) {
      this.createError = 'Title, due date, and class are required.';
      return;
    }
    this.assignmentService.createAssignment({
      ...this.newAssignment,
      dueDate: new Date(this.newAssignment.dueDate).toISOString()
    }).subscribe({
      next: (a) => {
        this.assignments.unshift(a);
        this.newAssignment = { title: '', description: '', dueDate: '', maxScore: 20, classGroupId: 0 };
        this.showCreateForm = false;
        this.createSuccess = 'Assignment created successfully!';
        setTimeout(() => this.createSuccess = '', 3000);
      },
      error: (err) => { this.createError = err?.error || 'Failed to create assignment.'; }
    });
  }

  // ── Submissions / Grading ─────────────────────────────────────────────

  viewSubmissions(assignment: any): void {
    this.selectedAssignment = assignment;
    this.activeTab = 'grade';
    this.submissionsLoading = true;
    this.gradingMap = {};
    this.assignmentService.getSubmissions(assignment.id).subscribe({
      next: (data) => {
        this.submissions = data;
        // Pre-populate grading map with existing scores
        data.forEach((s: any) => {
          this.gradingMap[s.id] = { score: s.score ?? 0, feedback: s.feedback ?? '' };
        });
        this.submissionsLoading = false;
      },
      error: () => { this.submissionsLoading = false; }
    });
  }

  saveGrade(submissionId: number): void {
    const g = this.gradingMap[submissionId];
    if (!g) return;
    this.assignmentService.gradeSubmission(submissionId, g.score, g.feedback).subscribe({
      next: (updated) => {
        const idx = this.submissions.findIndex(s => s.id === submissionId);
        if (idx >= 0) this.submissions[idx] = updated;
        this.gradeSuccess = 'Grade saved!';
        setTimeout(() => this.gradeSuccess = '', 2500);
      },
      error: () => {}
    });
  }

  isOverdue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }

  logout(): void {
    this.teacherAuthService.logout();
    this.router.navigate(['/login']);
  }
}
