import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentAuthService } from '../../services/student-auth.service';
import { StudentAuthResponse } from '../../models';
import { extractFieldErrors, extractErrors } from '../../services/error.util';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-dashboard.component.html',
  styleUrls: ['./student-dashboard.component.css']
})
export class StudentDashboardComponent implements OnInit {
  student: StudentAuthResponse | null = null;
  isLoading = true;
  errors: string[] = [];

  // Assessment submission
  assessment1: number | null = null;
  assessment2: number | null = null;
  assessment3: number | null = null;
  fieldErrors: { [key: string]: string } = {};
  submitErrors: string[] = [];
  isSubmitting = false;
  submitSuccess = false;

  constructor(
    private studentAuthService: StudentAuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errors = [];
    this.studentAuthService.getProfile().subscribe({
      next: (data) => {
        this.student = data;
        this.studentAuthService.setCurrentStudent(data);
        this.assessment1 = data.assessment1;
        this.assessment2 = data.assessment2;
        this.assessment3 = data.assessment3;
        this.isLoading = false;
      },
      error: (error) => {
        this.errors = extractErrors(error);
        this.isLoading = false;
      }
    });
  }

  get total(): number {
    return (this.assessment1 ?? 0) + (this.assessment2 ?? 0) + (this.assessment3 ?? 0);
  }

  get average(): number {
    return this.total / 3;
  }

  get percentage(): number {
    return (this.total / 60) * 100;
  }

  get performanceLevel(): string {
    if (this.percentage < 50) return 'Needs Support';
    if (this.percentage <= 55) return 'Satisfactory';
    if (this.percentage <= 75) return 'Good';
    return 'Excellent';
  }

  getPerformanceLevelClass(level: string): string {
    switch (level) {
      case 'Excellent': return 'excellent';
      case 'Good': return 'good';
      case 'Satisfactory': return 'satisfactory';
      case 'Needs Support': return 'needs-support';
      default: return '';
    }
  }

  validateField(field: string): void {
    delete this.fieldErrors[field];
    switch (field) {
      case 'assessment1':
        if (this.assessment1 === null || this.assessment1 < 0 || this.assessment1 > 20) {
          this.fieldErrors['assessment1'] = 'Must be between 0 and 20';
        }
        break;
      case 'assessment2':
        if (this.assessment2 === null || this.assessment2 < 0 || this.assessment2 > 20) {
          this.fieldErrors['assessment2'] = 'Must be between 0 and 20';
        }
        break;
      case 'assessment3':
        if (this.assessment3 === null || this.assessment3 < 0 || this.assessment3 > 20) {
          this.fieldErrors['assessment3'] = 'Must be between 0 and 20';
        }
        break;
    }
  }

  private validate(): boolean {
    this.fieldErrors = {};
    ['assessment1', 'assessment2', 'assessment3'].forEach(f => this.validateField(f));
    return Object.keys(this.fieldErrors).length === 0;
  }

  onSubmitAssessments(): void {
    this.isSubmitting = true;
    this.submitErrors = [];
    this.submitSuccess = false;

    if (!this.validate()) {
      this.isSubmitting = false;
      return;
    }

    this.studentAuthService.submitAssessments({
      assessment1: this.assessment1!,
      assessment2: this.assessment2!,
      assessment3: this.assessment3!
    }).subscribe({
      next: (data) => {
        this.student = data;
        this.studentAuthService.setCurrentStudent(data);
        this.isSubmitting = false;
        this.submitSuccess = true;
        setTimeout(() => this.submitSuccess = false, 3000);
      },
      error: (error) => {
        const { fieldErrors, generalErrors } = extractFieldErrors(error);
        this.fieldErrors = fieldErrors;
        this.submitErrors = generalErrors;
        this.isSubmitting = false;
      }
    });
  }
}
