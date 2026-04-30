import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TeacherAuthService } from '../../services/teacher-auth.service';
import { Teacher } from '../../models';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.css']
})
export class TeacherDashboardComponent implements OnInit {
  teacher: Teacher | null = null;
  isLoading = true;
  errors: string[] = [];

  constructor(
    private teacherAuthService: TeacherAuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadTeacherProfile();
  }

  loadTeacherProfile(): void {
    this.isLoading = true;
    this.errors = [];

    this.teacher = this.teacherAuthService.getCurrentTeacher();
    this.isLoading = false;
  }

  logout(): void {
    this.teacherAuthService.logout();
    this.router.navigate(['/teacher-login']);
  }
}
