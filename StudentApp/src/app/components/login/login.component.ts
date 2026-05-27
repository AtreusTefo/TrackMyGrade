import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AdminAuthService } from '../../services/admin-auth.service';
import { TeacherAuthService } from '../../services/teacher-auth.service';
import { StudentAuthService } from '../../services/student-auth.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  email    = '';
  password = '';
  showPassword = false;
  isSubmitting = false;

  /** Inline validation errors */
  fieldErrors: { [key: string]: string } = {};
  /** Top-level error shown when all three login attempts fail */
  loginError = '';

  constructor(
    private adminAuthService: AdminAuthService,
    private teacherAuthService: TeacherAuthService,
    private studentAuthService: StudentAuthService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // If already authenticated, skip the form and go straight to the dashboard
    if (this.authService.isAnyUserLoggedIn()) {
      this.authService.redirectToDashboard();
    }
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  // ── Validation ──────────────────────────────────────────────────

  validateField(field: string): void {
    delete this.fieldErrors[field];
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (field === 'email') {
      if (!this.email.trim()) {
        this.fieldErrors['email'] = 'Email address is required';
      } else if (!emailPattern.test(this.email)) {
        this.fieldErrors['email'] = 'Enter a valid email address';
      }
    }

    if (field === 'password') {
      if (!this.password) {
        this.fieldErrors['password'] = 'Password is required';
      }
    }
  }

  private isValid(): boolean {
    this.fieldErrors = {};
    this.loginError  = '';
    ['email', 'password'].forEach(f => this.validateField(f));
    return Object.keys(this.fieldErrors).length === 0;
  }

  // ── Submit — try all three endpoints in sequence ────────────────

  onSubmit(): void {
    if (!this.isValid()) return;

    this.isSubmitting = true;
    this.loginError   = '';

    const creds = { email: this.email.trim(), password: this.password };

    // Try each role sequentially
    this.tryAdminLogin(creds);
  }

  private tryAdminLogin(creds: any): void {
    this.adminAuthService.login(creds).subscribe({
      next: (res) => {
        console.log('[AdminLogin] Login response received:', res);
        this.adminAuthService.setCurrentAdmin(res);
        
        // Verify token was actually stored
        const storedToken = this.adminAuthService.getToken();
        console.log('[AdminLogin] Token stored:', storedToken ? 'YES' : 'NO (EMPTY/NULL)');
        
        if (!storedToken) {
          this.loginError = 'Login failed: No authentication token received';
          this.isSubmitting = false;
          return;
        }
        
        this.isSubmitting = false;
        this.router.navigate(['/admin-dashboard']);
      },
      error: (err) => {
        console.error('[AdminLogin] Login error:', err);
        this.tryTeacherLogin(creds);
      }
    });
  }

  private tryTeacherLogin(creds: any): void {
    this.teacherAuthService.login(creds).subscribe({
      next: (res) => {
        this.teacherAuthService.setCurrentTeacher(res);
        this.isSubmitting = false;
        this.router.navigate(['/teacher-dashboard']);
      },
      error: () => this.tryStudentLogin(creds)
    });
  }

  private tryStudentLogin(creds: any): void {
    this.studentAuthService.login(creds).subscribe({
      next: (res) => {
        this.studentAuthService.setCurrentStudent(res);
        this.isSubmitting = false;
        this.router.navigate(['/student-dashboard']);
      },
      error: () => {
        this.loginError  = 'Invalid email or password. Please try again.';
        this.isSubmitting = false;
      }
    });
  }
}