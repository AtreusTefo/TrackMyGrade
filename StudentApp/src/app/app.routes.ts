import { Routes, CanActivateFn, Router } from '@angular/router';
import { AdminLoginComponent } from './components/admin-login/admin-login.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { TeacherLoginComponent } from './components/teacher-login/teacher-login.component';
import { TeacherDashboardComponent } from './components/teacher-dashboard/teacher-dashboard.component';
import { RegisterComponent } from './components/register/register.component';
import { StudentListComponent } from './components/student-list/student-list.component';
import { StudentFormComponent } from './components/student-form/student-form.component';
import { StudentDetailComponent } from './components/student-detail/student-detail.component';
import { StudentLoginComponent } from './components/student-login/student-login.component';
import { StudentDashboardComponent } from './components/student-dashboard/student-dashboard.component';
import { HomeComponent } from './components/home/home.component';
import { TeacherAuthService } from './services/teacher-auth.service';
import { StudentAuthService } from './services/student-auth.service';
import { inject } from '@angular/core';

const teacherauthGuard: CanActivateFn = (route, state) => {
  const authService: TeacherAuthService = inject(TeacherAuthService);
  const router: Router = inject(Router);
  if (authService.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/teacher-login']);
};

const studentAuthGuard: CanActivateFn = (route, state) => {
  const studentAuthService: StudentAuthService = inject(StudentAuthService);
  const router: Router = inject(Router);
  if (studentAuthService.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/student-login']);
};

export const routes: Routes = [
  // Home Page
  { path: '', component: HomeComponent, pathMatch: 'full' },

  // Admin routes
  { path: 'admin', component: AdminLoginComponent },
  { path: 'admin-dashboard', component: AdminDashboardComponent },

  // Teacher routes
  { path: 'teacher-login', component: TeacherLoginComponent },
  { path: 'teacher-dashboard', component: TeacherDashboardComponent, canActivate: [teacherauthGuard] },
  { path: 'register', component: RegisterComponent },
  { path: 'list', component: StudentListComponent, canActivate: [teacherauthGuard] },
  { path: 'create', component: StudentFormComponent, canActivate: [teacherauthGuard] },
  { path: 'edit/:id', component: StudentFormComponent, canActivate: [teacherauthGuard] },
  { path: 'detail/:id', component: StudentDetailComponent, canActivate: [teacherauthGuard] },

  // Student routes (login only — accounts created by teachers)
  { path: 'student-login', component: StudentLoginComponent },
  { path: 'student-dashboard', component: StudentDashboardComponent, canActivate: [studentAuthGuard] },
  { path: '**', redirectTo: '/' }
];
