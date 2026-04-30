import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TeacherAuthService } from '../../services/teacher-auth.service';
import { extractFieldErrors } from '../../services/error.util';

@Component({
  selector: 'app-teacher-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teacher-login.component.html',
  styleUrls: ['./teacher-login.component.css']
})
export class TeacherLoginComponent implements OnInit {
  email: string = '';
  password: string = '';
  showPassword = false;
  fieldErrors: { [key: string]: string } = {};
  serverErrors: string[] = [];
  isSubmitting = false;

  constructor(
    private teacherAuthService: TeacherAuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    if (this.teacherAuthService.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  validateField(field: string): void {
    delete this.fieldErrors[field];
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    switch (field) {
      case 'email':
        if (!this.email.trim()) {
          this.fieldErrors['email'] = 'Email is required';
        } else if (!emailPattern.test(this.email)) {
          this.fieldErrors['email'] = 'Email must be a valid email address';
        }
        break;
      case 'password':
        if (!this.password) {
          this.fieldErrors['password'] = 'Password is required';
        }
        break;
    }
  }

  private validate(): boolean {
    this.fieldErrors = {};
    ['email', 'password'].forEach(f => this.validateField(f));
    return Object.keys(this.fieldErrors).length === 0;
  }

  onSubmit(): void {
    this.serverErrors = [];
    this.isSubmitting = true;

    if (!this.validate()) {
      this.isSubmitting = false;
      return;
    }

    this.teacherAuthService.login({ email: this.email, password: this.password }).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.teacherAuthService.setCurrentTeacher(response);
        this.router.navigate(['/']);
      },
      error: (error) => {
        const { fieldErrors, generalErrors } = extractFieldErrors(error);

        if (error.status !== 0 && generalErrors.length > 0) {
          // Credentials error (e.g. "Invalid email or password") — show inline on password field
          this.fieldErrors = { ...fieldErrors, password: generalErrors[0] };
          this.serverErrors = [];
        } else {
          // Network/connection error — keep in banner
          this.fieldErrors = fieldErrors;
          this.serverErrors = generalErrors;
        }
        this.isSubmitting = false;
      }
    });
  }
}
