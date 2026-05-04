import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AdminAuthService } from './services/admin-auth.service';
import { TeacherAuthService } from './services/teacher-auth.service';
import { StudentAuthService } from './services/student-auth.service';
import { AuthService, UserRole } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'TrackMyGrade';

  activeRole: UserRole = null;
  userName = '';

  // Per-role data exposed for template
  currentAdmin: any   = null;
  currentTeacher: any = null;
  currentStudent: any = null;

  get isAdminLoggedIn():   boolean { return this.activeRole === 'admin';   }
  get isTeacherLoggedIn(): boolean { return this.activeRole === 'teacher'; }
  get isStudentLoggedIn(): boolean { return this.activeRole === 'student'; }
  get isAnyoneLoggedIn():  boolean { return this.activeRole !== null;      }

  constructor(
    private adminAuthService: AdminAuthService,
    private teacherAuthService: TeacherAuthService,
    private studentAuthService: StudentAuthService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Reactively update navbar whenever any auth state changes
    this.adminAuthService.currentAdmin$.subscribe(admin => {
      this.currentAdmin = admin;
      this.refreshRole();
    });
    this.teacherAuthService.currentTeacher$.subscribe(teacher => {
      this.currentTeacher = teacher;
      this.refreshRole();
    });
    this.studentAuthService.currentStudent$.subscribe(student => {
      this.currentStudent = student;
      this.refreshRole();
    });
  }

  private refreshRole(): void {
    this.activeRole = this.authService.getActiveRole();
    this.userName   = this.authService.getCurrentUserName();
  }

  logout(): void {
    this.authService.logoutAll();
  }
}
