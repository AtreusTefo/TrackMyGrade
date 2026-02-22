import { Routes, CanActivateFn, Router } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { StudentListComponent } from './components/student-list/student-list.component';
import { StudentFormComponent } from './components/student-form/student-form.component';
import { StudentDetailComponent } from './components/student-detail/student-detail.component';
import { AuthService } from './services/auth.service';
import { inject } from '@angular/core';

const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  if (authService.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/login']);
};

export const routes: Routes = [
  { path: '', redirectTo: '/list', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'list', component: StudentListComponent, canActivate: [authGuard] },
  { path: 'create', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'edit/:id', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'detail/:id', component: StudentDetailComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/list' }
];
