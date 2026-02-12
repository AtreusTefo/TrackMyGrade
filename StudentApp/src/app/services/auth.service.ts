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

  setCurrentTeacher(teacher: Teacher): void {
    localStorage.setItem('teacher', JSON.stringify(teacher));
    localStorage.setItem('token', teacher.token);
    this.currentTeacher.next(teacher);
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
    const teacher = localStorage.getItem('teacher');
    return teacher ? JSON.parse(teacher) : null;
  }
}
