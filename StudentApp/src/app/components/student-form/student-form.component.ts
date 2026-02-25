import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { StudentCreate, StudentUpdate } from '../../models';
import { extractFieldErrors } from '../../services/error.util';

@Component({
  selector: 'app-student-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-form.component.html',
  styleUrls: ['./student-form.component.css']
})
export class StudentFormComponent implements OnInit {
  isEditMode = false;
  studentId: number | null = null;
  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  grade: number | null = null;
  assessment1: number | null = null;
  assessment2: number | null = null;
  assessment3: number | null = null;

  fieldErrors: { [key: string]: string } = {};
  serverErrors: string[] = [];

  isSubmitting = false;
  isLoading = false;

  total = 0;
  average = 0;
  percentage = 0;
  performanceLevel = '';

  constructor(
    private studentService: StudentService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.studentId = parseInt(id, 10);
      this.loadStudent();
    }
  }

  loadStudent(): void {
    if (!this.studentId) return;
    this.isLoading = true;
    this.studentService.getStudentById(this.studentId).subscribe({
      next: (student) => {
        this.firstName = student.firstName;
        this.lastName = student.lastName;
        this.email = student.email;
        this.phone = student.phone;
        this.grade = student.grade;
        this.assessment1 = student.assessment1;
        this.assessment2 = student.assessment2;
        this.assessment3 = student.assessment3;
        this.isLoading = false;
        this.calculateValues();
      },
      error: (error) => {
        const { generalErrors } = extractFieldErrors(error);
        this.serverErrors = generalErrors;
        this.isLoading = false;
      }
    });
  }

  calculateValues(): void {
    const a1 = this.assessment1 ?? 0;
    const a2 = this.assessment2 ?? 0;
    const a3 = this.assessment3 ?? 0;
    this.total = a1 + a2 + a3;
    this.average = this.total / 3;
    this.percentage = (this.total / 60) * 100;

    if (this.percentage < 50) {
      this.performanceLevel = 'Needs Support';
    } else if (this.percentage <= 55) {
      this.performanceLevel = 'Satisfactory';
    } else if (this.percentage <= 75) {
      this.performanceLevel = 'Good';
    } else {
      this.performanceLevel = 'Excellent';
    }
  }

  onAssessmentChange(): void {
    this.calculateValues();
  }

  clearFieldError(field: string): void {
    delete this.fieldErrors[field];
  }

  validate(): boolean {
    this.fieldErrors = {};

    if (!this.firstName || this.firstName.length < 2 || this.firstName.length > 50) {
      this.fieldErrors['firstName'] = 'First name must be between 2 and 50 characters';
    }
    if (!this.lastName || this.lastName.length < 2 || this.lastName.length > 50) {
      this.fieldErrors['lastName'] = 'Last name must be between 2 and 50 characters';
    }
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!this.email || !emailPattern.test(this.email)) {
      this.fieldErrors['email'] = 'Email must be a valid email address';
    }
    if (!this.phone || !/^\d{8}$/.test(this.phone)) {
      this.fieldErrors['phone'] = 'Phone must be exactly 8 digits';
    }
    if (this.grade === null || this.grade < 1 || this.grade > 12) {
      this.fieldErrors['grade'] = 'Grade must be between 1 and 12';
    }
    if (this.assessment1 === null || this.assessment1 < 0 || this.assessment1 > 20) {
      this.fieldErrors['assessment1'] = 'Assessment 1 must be between 0 and 20';
    }
    if (this.assessment2 === null || this.assessment2 < 0 || this.assessment2 > 20) {
      this.fieldErrors['assessment2'] = 'Assessment 2 must be between 0 and 20';
    }
    if (this.assessment3 === null || this.assessment3 < 0 || this.assessment3 > 20) {
      this.fieldErrors['assessment3'] = 'Assessment 3 must be between 0 and 20';
    }

    return Object.keys(this.fieldErrors).length === 0;
  }

  onSubmit(): void {
    this.isSubmitting = true;
    this.serverErrors = [];

    if (!this.validate()) {
      this.isSubmitting = false;
      return;
    }

    const data = {
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      phone: this.phone,
      grade: this.grade!,
      assessment1: this.assessment1!,
      assessment2: this.assessment2!,
      assessment3: this.assessment3!
    };

    if (this.isEditMode && this.studentId) {
      this.studentService.updateStudent(this.studentId, data as StudentUpdate).subscribe({
        next: () => {
          this.isSubmitting = false;
          this.router.navigate(['/detail', this.studentId]);
        },
        error: (error) => {
          const { fieldErrors, generalErrors } = extractFieldErrors(error);
          this.fieldErrors = fieldErrors;
          this.serverErrors = generalErrors;
          this.isSubmitting = false;
        }
      });
    } else {
      this.studentService.createStudent(data as StudentCreate).subscribe({
        next: () => {
          this.isSubmitting = false;
          this.router.navigate(['/list']);
        },
        error: (error) => {
          const { fieldErrors, generalErrors } = extractFieldErrors(error);
          this.fieldErrors = fieldErrors;
          this.serverErrors = generalErrors;
          this.isSubmitting = false;
        }
      });
    }
  }
}
