import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private apiUrl = 'http://localhost:5000/api/admin';
  private authUrl = 'http://localhost:5000/api/auth';

  constructor(private http: HttpClient) { }

  // ── Auth header (real JWT Bearer token) ──────────────────────────────
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('adminToken');
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

  // ── Subjects ───────────────────────────────────────────────────────────
  getAllSubjects(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/subjects`, { headers: this.getHeaders() });
  }

  createSubject(data: { name: string; code: string; description?: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/subjects`, data, { headers: this.getHeaders() });
  }

  deleteSubject(subjectId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/subjects/${subjectId}`, { headers: this.getHeaders() });
  }

  // ── Class Groups ──────────────────────────────────────────────────────
  getAllClassGroups(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/class-groups`, { headers: this.getHeaders() });
  }

  createClassGroup(data: { name: string; gradeLevel: number; subjectId: number; teacherId: number }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/class-groups`, data, { headers: this.getHeaders() });
  }

  deleteClassGroup(classGroupId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/class-groups/${classGroupId}`, { headers: this.getHeaders() });
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

  // ── Audit Logs ────────────────────────────────────────────────────────

  /**
   * Get paginated audit logs with optional filtering.
   * @param filter Filter criteria (entityType, action, performedBy, startDate, endDate, pageNumber, pageSize)
   */
  getAuditLogs(filter: any): Observable<any> {
    let url = `${this.apiUrl}/audit-logs?pageNumber=${filter.pageNumber || 1}&pageSize=${filter.pageSize || 50}`;

    if (filter.entityType) url += `&entityType=${encodeURIComponent(filter.entityType)}`;
    if (filter.action) url += `&action=${encodeURIComponent(filter.action)}`;
    if (filter.performedBy) url += `&performedBy=${encodeURIComponent(filter.performedBy)}`;
    if (filter.startDate) url += `&startDate=${encodeURIComponent(filter.startDate)}`;
    if (filter.endDate) url += `&endDate=${encodeURIComponent(filter.endDate)}`;

    return this.http.get<any>(url, { headers: this.getHeaders() });
  }

  /**
   * Get all audit logs for a specific entity.
   * @param entityType Entity type (e.g., "Teacher", "Student", "ClassGroup")
   * @param entityId Primary key of the entity
   */
  getAuditLogsByEntity(entityType: string, entityId: number): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/audit-logs/entity/${encodeURIComponent(entityType)}/${entityId}`,
      { headers: this.getHeaders() }
    );
  }

  /**
   * Get all audit logs performed by a specific admin user.
   * @param email Admin email address
   */
  getAuditLogsByUser(email: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/audit-logs/user/${encodeURIComponent(email)}`,
      { headers: this.getHeaders() }
    );
  }
}
