import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  email: string = '';
  password: string = '';
  errors: string[] = [];
  isSubmitting = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  onSubmit(): void {
    this.errors = [];
    this.isSubmitting = true;

    // Validation
    if (!this.email) {
      this.errors.push('Email is required');
    }
    if (!this.password) {
      this.errors.push('Password is required');
    }

    if (this.errors.length > 0) {
      this.isSubmitting = false;
      return;
    }

    this.authService.login({ email: this.email, password: this.password }).subscribe(
      (response) => {
        this.authService.setCurrentTeacher(response);
        this.router.navigate(['/']);
      },
      (error) => {
        this.errors = [error.error || 'Login failed. Please try again.'];
        this.isSubmitting = false;
      }
    );
  }
}
