import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { extractFieldErrors } from '../../services/error.util';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
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
  fieldErrors: { [key: string]: string } = {};
  serverErrors: string[] = [];
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

  validateField(field: string): void {
    delete this.fieldErrors[field];
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    switch (field) {
      case 'firstName':
        if (!this.firstName.trim()) {
          this.fieldErrors['firstName'] = 'First name is required';
        } else if (!/^[a-zA-Z '\-]+$/.test(this.firstName)) {
          this.fieldErrors['firstName'] = 'First name must contain only letters';
        } else if (this.firstName.length < 2 || this.firstName.length > 50) {
          this.fieldErrors['firstName'] = 'First name must be between 2 and 50 characters';
        }
        break;
      case 'lastName':
        if (!this.lastName.trim()) {
          this.fieldErrors['lastName'] = 'Last name is required';
        } else if (!/^[a-zA-Z '\-]+$/.test(this.lastName)) {
          this.fieldErrors['lastName'] = 'Last name must contain only letters';
        } else if (this.lastName.length < 2 || this.lastName.length > 50) {
          this.fieldErrors['lastName'] = 'Last name must be between 2 and 50 characters';
        }
        break;
      case 'email':
        if (!this.email.trim()) {
          this.fieldErrors['email'] = 'Email is required';
        } else if (!emailPattern.test(this.email)) {
          this.fieldErrors['email'] = 'Email must be a valid email address';
        }
        break;
      case 'phone':
        if (!this.phone.trim()) {
          this.fieldErrors['phone'] = 'Phone is required';
        } else if (!/^\d{8}$/.test(this.phone)) {
          this.fieldErrors['phone'] = 'Phone must be exactly 8 digits';
        }
        break;
      case 'subject':
        if (!this.subject.trim()) {
          this.fieldErrors['subject'] = 'Subject is required';
        } else if (this.subject.length > 100) {
          this.fieldErrors['subject'] = 'Subject must not exceed 100 characters';
        }
        break;
      case 'password':
        if (!this.password) {
          this.fieldErrors['password'] = 'Password is required';
        } else if (this.password.length < 6 || this.password.length > 20) {
          this.fieldErrors['password'] = 'Password must be between 6 and 20 characters';
        }
        if (this.confirmPassword) {
          delete this.fieldErrors['confirmPassword'];
          if (this.password !== this.confirmPassword) {
            this.fieldErrors['confirmPassword'] = 'Passwords do not match';
          }
        }
        break;
      case 'confirmPassword':
        if (!this.confirmPassword) {
          this.fieldErrors['confirmPassword'] = 'Please confirm your password';
        } else if (this.password !== this.confirmPassword) {
          this.fieldErrors['confirmPassword'] = 'Passwords do not match';
        }
        break;
    }
  }

  validate(): boolean {
    this.fieldErrors = {};
    ['firstName', 'lastName', 'email', 'phone', 'subject', 'password', 'confirmPassword']
      .forEach(f => this.validateField(f));
    return Object.keys(this.fieldErrors).length === 0;
  }

  filterLetters(event: KeyboardEvent): void {
    const allowedKeys = ['Backspace', 'Delete', 'Tab', 'Enter',
                         'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown',
                         'Home', 'End', 'Escape'];
    if (allowedKeys.includes(event.key) || event.ctrlKey || event.metaKey) return;
    if (!/^[a-zA-Z '\-]$/.test(event.key)) {
      event.preventDefault();
    }
  }

  filterDigits(event: KeyboardEvent): void {
    const allowedKeys = ['Backspace', 'Delete', 'Tab', 'Enter',
                         'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Escape'];
    if (allowedKeys.includes(event.key) || event.ctrlKey || event.metaKey) return;
    if (!/^\d$/.test(event.key)) {
      event.preventDefault();
    }
  }

  onSubmit(): void {
    this.isSubmitting = true;
    this.serverErrors = [];

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

    this.authService.register(registerData).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.router.navigate(['/login']);
      },
      error: (error) => {
        const { fieldErrors, generalErrors } = extractFieldErrors(error);
        this.fieldErrors = fieldErrors;
        this.serverErrors = generalErrors;
        this.isSubmitting = false;
      }
    });
  }
}
