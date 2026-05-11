import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AdminAuthService } from './admin-auth.service';
import { TeacherAuthService } from './teacher-auth.service';
import { StudentAuthService } from './student-auth.service';

export type UserRole = 'admin' | 'teacher' | 'student' | null;

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private lastRedirectTime = 0;
  private readonly REDIRECT_DEBOUNCE_MS = 500;

  constructor(
    private adminAuthService: AdminAuthService,
    private teacherAuthService: TeacherAuthService,
    private studentAuthService: StudentAuthService,
    private router: Router
  ) {}

  /**
   * Determines which role is currently logged in by checking stored tokens.
   * Priority: admin > teacher > student
   */
  getActiveRole(): UserRole {
    if (this.adminAuthService.isAuthenticated()) return 'admin';
    if (this.teacherAuthService.isAuthenticated()) return 'teacher';
    if (this.studentAuthService.isAuthenticated()) return 'student';
    return null;
  }

  /**
   * Returns true if ANY role is currently authenticated.
   */
  isAnyUserLoggedIn(): boolean {
    return this.getActiveRole() !== null;
  }

  /**
   * Redirects the current user to their role-specific dashboard.
   * Returns true if a redirect occurred (user was logged in), false otherwise.
   * Uses debouncing to prevent rapid successive redirects.
   */
  redirectToDashboard(): boolean {
    const now = Date.now();
    if (now - this.lastRedirectTime < this.REDIRECT_DEBOUNCE_MS) {
      return false; // Debounce: ignore rapid successive calls
    }
    this.lastRedirectTime = now;

    const role = this.getActiveRole();
    switch (role) {
      case 'admin':
        this.router.navigate(['/admin-dashboard']);
        return true;
      case 'teacher':
        this.router.navigate(['/teacher-dashboard']);
        return true;
      case 'student':
        this.router.navigate(['/student-dashboard']);
        return true;
      default:
        return false;
    }
  }

  /**
   * Decodes a JWT payload (base64) without verifying the signature.
   * Used only for reading the `role` claim from the token.
   */
  decodeTokenPayload(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch {
      return null;
    }
  }

  /**
   * Logs out all active sessions and navigates to /login.
   */
  logoutAll(): void {
    this.adminAuthService.logout();
    this.teacherAuthService.logout();
    this.studentAuthService.logout();
    this.router.navigate(['/login']);
  }

  /**
   * Gets the current user's display name across all roles.
   */
  getCurrentUserName(): string {
    const admin = this.adminAuthService.getCurrentAdmin();
    if (admin) return `${admin.firstName} ${admin.lastName}`;

    const teacher = this.teacherAuthService.getCurrentTeacher();
    if (teacher) return `${teacher.firstName} ${teacher.lastName}`;

    const student = this.studentAuthService.getCurrentStudent();
    if (student) return `${student.firstName} ${student.lastName}`;

    return '';
  }
}
