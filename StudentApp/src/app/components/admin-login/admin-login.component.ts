import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { extractFieldErrors } from '../../services/error.util';

@Component({
    selector: 'app-admin-login',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule],
    templateUrl: './admin-login.component.html',
    styleUrls: ['./admin-login.component.css']
})

export class AdminLoginComponent implements OnInit {
    email: string = '';
    password: string = '';
    showPassword = false;
    fieldErrors: { [key: string]: string } = {};
    serverErrors: string[] = [];
    isSubmitting = false;

    constructor(
        private adminAuthService: AdminAuthService,
        private router: Router
    ) { }

    ngOnInit(): void {
        if (this.adminAuthService.isAuthenticated()) {
            this.router.navigate(['/admin-dashboard']);
        }
    }

    validateField(field: string): void {
        delete this.fieldErrors[field];
        const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        switch (field) {
            case 'email':
                if (!this.email.trim()) {
                    this.fieldErrors['email'] = 'Email is required';
                } else if (!emailPattern.test(this.email)) {
                    this.fieldErrors['email'] = 'Email must be a valid email address';
                }
                break;
            case 'password':
                if (!this.password) {
                    this.fieldErrors['password'] = 'Password is required';
                }
                break;
        }
    }

    private validate(): boolean {
        this.fieldErrors = {};
        ['email', 'password'].forEach(f => this.validateField(f));
        return Object.keys(this.fieldErrors).length === 0;
    }

    onSubmit(): void {
        this.serverErrors = [];
        this.isSubmitting = true;

        if (!this.validate()) {
            this.isSubmitting = false;
            return;
        }

        this.adminAuthService.login({ email: this.email, password: this.password }).subscribe({
            next: (response) => {
                this.isSubmitting = false;
                this.adminAuthService.setCurrentAdmin(response);
                this.router.navigate(['/admin-dashboard']);
            },
            error: (error) => {
                const { fieldErrors, generalErrors } = extractFieldErrors(error);

                if (error.status !== 0 && generalErrors.length > 0) {
                    this.fieldErrors = { ...fieldErrors, password: generalErrors[0] };
                    this.serverErrors = [];
                } else {
                    this.fieldErrors = fieldErrors;
                    this.serverErrors = generalErrors;
                }
                this.isSubmitting = false;
            }
        });
    }
}