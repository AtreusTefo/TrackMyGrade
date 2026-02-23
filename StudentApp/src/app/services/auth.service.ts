import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Teacher, TeacherRegister, TeacherLogin } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/teachers';
  private currentTeacher = new BehaviorSubject<Teacher | null>(this.getStoredTeacher());
  public currentTeacher$ = this.currentTeacher.asObservable();

  constructor(private http: HttpClient) { }

  register(data: TeacherRegister): Observable<Teacher> {
    return this.http.post<Teacher>(`${this.apiUrl}/register`, data);
  }

  login(data: TeacherLogin): Observable<Teacher> {
    return this.http.post<Teacher>(`${this.apiUrl}/login`, data).pipe();
  }

  logout(): void {
    localStorage.removeItem('teacher');
    localStorage.removeItem('token');
    this.currentTeacher.next(null);
  }

  setCurrentTeacher(teacher: any): void {
    const normalized: Teacher = {
      id: teacher.id ?? teacher.Id ?? 0,
      firstName: teacher.firstName ?? teacher.FirstName ?? '',
      lastName: teacher.lastName ?? teacher.LastName ?? '',
      email: teacher.email ?? teacher.Email ?? '',
      phone: teacher.phone ?? teacher.Phone ?? '',
      subject: teacher.subject ?? teacher.Subject ?? '',
      token: teacher.token ?? teacher.Token ?? ''
    };
    localStorage.setItem('teacher', JSON.stringify(normalized));
    localStorage.setItem('token', normalized.token);
    this.currentTeacher.next(normalized);
  }

  getCurrentTeacher(): Teacher | null {
    return this.currentTeacher.value;
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private getStoredTeacher(): Teacher | null {
    const stored = localStorage.getItem('teacher');
    if (!stored) return null;
    try {
      const parsed = JSON.parse(stored);
      return {
        id: parsed.id ?? parsed.Id ?? 0,
        firstName: parsed.firstName ?? parsed.FirstName ?? '',
        lastName: parsed.lastName ?? parsed.LastName ?? '',
        email: parsed.email ?? parsed.Email ?? '',
        phone: parsed.phone ?? parsed.Phone ?? '',
        subject: parsed.subject ?? parsed.Subject ?? '',
        token: parsed.token ?? parsed.Token ?? ''
      };
    } catch {
      return null;
    }
  }
}
