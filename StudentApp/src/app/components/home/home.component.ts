import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  isLoggedIn = false;
  userName = '';
  userRole: string | null = null;
  dashboardRoute = '/login';

  readonly features = [
    {
      icon: '📈',
      title: 'Track Academic Performance',
      desc: 'Real-time grade tracking with visual progress indicators, percentage scores, and performance levels for every student.'
    },
    {
      icon: '🔒',
      title: 'Secure File Submissions',
      desc: 'End-to-end protected assessment submissions. Teachers control access while students get instant grade feedback.'
    },
    {
      icon: '📚',
      title: 'Courses & Subjects',
      desc: 'Manage multiple subjects per teacher, assign students to courses, and organise classes with ease.'
    },
    {
      icon: '📊',
      title: 'Instant Analytics',
      desc: 'Dashboard analytics at a glance — class averages, top performers, and areas that need attention.'
    },
    {
      icon: '🎓',
      title: 'Student Portfolios',
      desc: 'Each student has a personal profile showing all assessments, cumulative total, and performance level.'
    },
    {
      icon: '🛡️',
      title: 'Role-Based Access',
      desc: 'Admins, teachers, and students each see only what they need — no confusion, no unauthorised access.'
    }
  ];

  readonly stats = [
    { value: '3', label: 'User Roles' },
    { value: '100%', label: 'Secure JWT Auth' },
    { value: '3', label: 'Assessments / Student' },
    { value: '∞', label: 'Students Supported' }
  ];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    const role = this.authService.getActiveRole();
    if (role) {
      this.isLoggedIn = true;
      this.userRole = role;
      this.userName = this.authService.getCurrentUserName();
      switch (role) {
        case 'admin':   this.dashboardRoute = '/admin-dashboard';   break;
        case 'teacher': this.dashboardRoute = '/teacher-dashboard'; break;
        case 'student': this.dashboardRoute = '/student-dashboard'; break;
      }
    }
  }

  getRoleLabel(): string {
    switch (this.userRole) {
      case 'admin':   return 'Administrator';
      case 'teacher': return 'Teacher';
      case 'student': return 'Student';
      default:        return '';
    }
  }

  getRoleIcon(): string {
    switch (this.userRole) {
      case 'admin':   return '🛡️';
      case 'teacher': return '📚';
      case 'student': return '🎓';
      default:        return '👤';
    }
  }
}
