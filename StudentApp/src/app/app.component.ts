import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TeacherAuthService } from './services/teacher-auth.service';
import { StudentAuthService } from './services/student-auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'TrackMyGrade';
  isTeacherAuthenticated = false;
  isStudentAuthenticated = false;
  currentTeacher: any = null;
  currentStudent: any = null;

  constructor(
    private teacherAuthService: TeacherAuthService,
    private studentAuthService: StudentAuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.teacherAuthService.currentTeacher$.subscribe(teacher => {
      this.currentTeacher = teacher;
      this.isTeacherAuthenticated = !!teacher;
    });
    this.studentAuthService.currentStudent$.subscribe(student => {
      this.currentStudent = student;
      this.isStudentAuthenticated = !!student;
    });
  }

  logoutTeacher(): void {
    this.teacherAuthService.logout();
    this.router.navigate(['/login']);
  }

  logoutStudent(): void {
    this.studentAuthService.logout();
    this.router.navigate(['/student-login']);
  }
}
