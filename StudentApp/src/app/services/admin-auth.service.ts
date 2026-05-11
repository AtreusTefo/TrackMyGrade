import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Admin, AdminLogin, StudentSubmitAssessments } from '../models';

@Injectable({
    providedIn: 'root'
})
export class AdminAuthService {
    private apiUrl = 'http://localhost:5000/api/admin';
    private currentAdmin = new BehaviorSubject<Admin | null>(this.getStoredAdmin());
    public currentAdmin$ = this.currentAdmin.asObservable();

    constructor(private http: HttpClient) { }

    login(data: AdminLogin): Observable<Admin> {
        return this.http.post<Admin>(`${this.apiUrl}/login`, data);
    }

    getProfile(): Observable<Admin> {
        return this.http.get<Admin>(`${this.apiUrl}/profile`, {
            headers: this.getHeaders()
        });
    }

    submitAssessments(data: StudentSubmitAssessments): Observable<Admin> {
        return this.http.put<Admin>(`${this.apiUrl}/submit-assessments`, data, {
            headers: this.getHeaders()
        });
    }

    logout(): void {
        localStorage.removeItem('admin');
        localStorage.removeItem('adminToken');
        this.currentAdmin.next(null);
    }

    setCurrentAdmin(admin: any): void {
        // Build as a loose object first to avoid TS complaining about extra properties on Admin
        const normalizedAny: any = {
            id: admin.id ?? admin.Id ?? 0,
            firstName: admin.firstName ?? admin.FirstName ?? '',
            lastName: admin.lastName ?? admin.LastName ?? '',
            email: admin.email ?? admin.Email ?? '',
            phone: admin.phone ?? admin.Phone ?? '',
            omangOrPassport: admin.omangOrPassport ?? admin.OmangOrPassport ?? '',
            token: admin.token ?? admin.Token ?? ''
        };

        const normalized: Admin = normalizedAny;
        localStorage.setItem('admin', JSON.stringify(normalized));
        localStorage.setItem('adminToken', normalized.token);
        this.currentAdmin.next(normalized);
    }

    getCurrentAdmin(): Admin | null {
        return this.currentAdmin.value;
    }

    getToken(): string | null {
        return localStorage.getItem('adminToken');
    }

    isAuthenticated(): boolean {
        return !!this.getToken();
    }

    private getHeaders(): HttpHeaders {
        const token = this.getToken();
        let headers = new HttpHeaders();
        if (token) {
            headers = headers.set('Authorization', `Bearer ${token}`);
        }
        return headers;
    }

    private getStoredAdmin(): Admin | null {
        const stored = localStorage.getItem('admin');
        if (!stored) return null;
        try {
            return JSON.parse(stored);
        } catch {
            return null;
        }
    }
}