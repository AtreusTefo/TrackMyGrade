import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private apiUrl = 'http://localhost:5000/api/admin';
  private authUrl = 'http://localhost:5000/api/auth';

  constructor(private http: HttpClient) {}

  // ── Auth header (real JWT Bearer token) ──────────────────────────────
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('admin_token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    });
  }

  // ── Admin login ───────────────────────────────────────────────────────
  login(email: string, password: string): Observable<{ email: string; token: string }> {
    return this.http.post<any>(`${this.apiUrl}/login`, { email, password });
  }

  // ── Teachers ──────────────────────────────────────────────────────────
  getAllTeachers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/teachers`, { headers: this.getHeaders() });
  }

  /** Creates a teacher account. Returns activationToken for the admin to share. */
  createTeacher(data: {
    firstName: string; lastName: string; email: string;
    phone?: string; subject: string;
  }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/teachers`, data, { headers: this.getHeaders() });
  }

  deleteTeacher(teacherId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/teachers/${teacherId}`, { headers: this.getHeaders() });
  }

  // ── Students ──────────────────────────────────────────────────────────
  getAllStudents(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/students`, { headers: this.getHeaders() });
  }

  /** Creates a student account. Returns activationToken for the admin to share. */
  createStudent(data: {
    firstName: string; lastName: string; email: string;
    phone?: string; omangOrPassport: string; grade: number; teacherId: number;
  }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/students`, data, { headers: this.getHeaders() });
  }

  updateStudent(studentId: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/students/${studentId}`, data, { headers: this.getHeaders() });
  }

  deleteStudent(studentId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/students/${studentId}`, { headers: this.getHeaders() });
  }

  // ── Courses ───────────────────────────────────────────────────────────
  getAllCourses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/courses`, { headers: this.getHeaders() });
  }

  createCourse(data: { name: string; code: string; description?: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/courses`, data, { headers: this.getHeaders() });
  }

  // ── Class Groups ──────────────────────────────────────────────────────
  getAllClassGroups(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/class-groups`, { headers: this.getHeaders() });
  }

  createClassGroup(data: { name: string; gradeLevel: number; courseId: number; teacherId: number }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/class-groups`, data, { headers: this.getHeaders() });
  }

  enrollStudent(classGroupId: number, studentId: number): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/class-groups/${classGroupId}/enroll`,
      { studentId },
      { headers: this.getHeaders() }
    );
  }

  unenrollStudent(classGroupId: number, studentId: number): Observable<any> {
    return this.http.delete<any>(
      `${this.apiUrl}/class-groups/${classGroupId}/enroll/${studentId}`,
      { headers: this.getHeaders() }
    );
  }

  // ── Activation (shared util) ──────────────────────────────────────────
  checkActivation(token: string, role: string): Observable<any> {
    return this.http.get<any>(`${this.authUrl}/check-activation?token=${token}&role=${role}`);
  }

  activateAccount(data: {
    role: string; activationToken: string; newPassword: string; confirmPassword: string;
  }): Observable<any> {
    return this.http.post<any>(`${this.authUrl}/activate`, data);
  }
}
