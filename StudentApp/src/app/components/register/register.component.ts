import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  firstName: string = '';
  lastName: string = '';
  email: string = '';
  phone: string = '';
  subject: string = '';
  password: string = '';
  confirmPassword: string = '';
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

  validate(): boolean {
    this.errors = [];

    if (!this.firstName || this.firstName.length < 2 || this.firstName.length > 50) {
      this.errors.push('First name must be between 2 and 50 characters');
    }
    if (!this.lastName || this.lastName.length < 2 || this.lastName.length > 50) {
      this.errors.push('Last name must be between 2 and 50 characters');
    }
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!this.email || !emailPattern.test(this.email)) {
      this.errors.push('Email must be a valid email address');
    }
    if (!this.phone || !/^\d{8}$/.test(this.phone)) {
      this.errors.push('Phone must be exactly 8 digits');
    }
    if (!this.subject || this.subject.length > 100) {
      this.errors.push('Subject is required and must not exceed 100 characters');
    }
    if (!this.password || this.password.length < 6 || this.password.length > 20) {
      this.errors.push('Password must be between 6 and 20 characters');
    }
    if (this.password !== this.confirmPassword) {
      this.errors.push('Passwords do not match');
    }

    return this.errors.length === 0;
  }

  onSubmit(): void {
    this.isSubmitting = true;

    if (!this.validate()) {
      this.isSubmitting = false;
      return;
    }

    const registerData = {
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      phone: this.phone,
      subject: this.subject,
      password: this.password
    };

    this.authService.register(registerData).subscribe(
      (response) => {
        this.router.navigate(['/login']);
      },
      (error) => {
        this.errors = [error.error || 'Registration failed. Please try again.'];
        this.isSubmitting = false;
      }
    );
  }
}
