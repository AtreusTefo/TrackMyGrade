import { Routes, CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AdminAuthService } from './services/admin-auth.service';
import { TeacherAuthService } from './services/teacher-auth.service';
import { StudentAuthService } from './services/student-auth.service';

import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { TeacherDashboardComponent } from './components/teacher-dashboard/teacher-dashboard.component';
import { StudentListComponent } from './components/student-list/student-list.component';
import { StudentDetailComponent } from './components/student-detail/student-detail.component';
import { StudentDashboardComponent } from './components/student-dashboard/student-dashboard.component';
import { ActivateComponent } from './components/activate/activate.component';

// ── Guards ────────────────────────────────────────────────────────────────────

/** Redirects to /login if no admin token is present */
const adminAuthGuard: CanActivateFn = () => {
  const auth   = inject(AdminAuthService);
  const router = inject(Router);
  return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

/** Redirects to /login if no teacher token is present */
const teacherAuthGuard: CanActivateFn = () => {
  const auth   = inject(TeacherAuthService);
  const router = inject(Router);
  return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

/** Redirects to /login if no student token is present */
const studentAuthGuard: CanActivateFn = () => {
  const auth   = inject(StudentAuthService);
  const router = inject(Router);
  return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

// ── Routes ────────────────────────────────────────────────────────────────────

export const routes: Routes = [
  // Public pages
  { path: '',        component: HomeComponent,        pathMatch: 'full' },
  { path: 'login',   component: LoginComponent },
  // Account activation (public — no guard, token is the auth mechanism)
  { path: 'activate', component: ActivateComponent },

  // Admin routes (guarded)
  { path: 'admin-dashboard', component: AdminDashboardComponent, canActivate: [adminAuthGuard] },

  // Teacher routes (guarded)
  { path: 'teacher-dashboard', component: TeacherDashboardComponent, canActivate: [teacherAuthGuard] },
  { path: 'list',              component: StudentListComponent,       canActivate: [teacherAuthGuard] },
  { path: 'detail/:id',        component: StudentDetailComponent,     canActivate: [teacherAuthGuard] },

  // Student routes (guarded)
  { path: 'student-dashboard', component: StudentDashboardComponent, canActivate: [studentAuthGuard] },

  // Legacy login redirects → unified /login
  { path: 'admin',         redirectTo: '/login', pathMatch: 'full' },
  { path: 'admin-login',   redirectTo: '/login', pathMatch: 'full' },
  { path: 'teacher-login', redirectTo: '/login', pathMatch: 'full' },
  { path: 'student-login', redirectTo: '/login', pathMatch: 'full' },

  // Catch-all
  { path: '**', redirectTo: '/' }
];
