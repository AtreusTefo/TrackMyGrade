import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Shared JWT header builder — reads the correct token key for the role. */
function bearerHeaders(tokenKey: string): HttpHeaders {
  const token = localStorage.getItem(tokenKey);
  return new HttpHeaders({
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {})
  });
}

// ── Teacher Assignment Service ──────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class TeacherAssignmentService {
  private base = 'http://localhost:5000/api/teacher';

  constructor(private http: HttpClient) {}

  private headers() { return bearerHeaders('teacher_token'); }

  getMyStudents(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/my-students`, { headers: this.headers() });
  }

  getAssignments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/assignments`, { headers: this.headers() });
  }

  createAssignment(data: {
    title: string; description?: string;
    dueDate: string; maxScore: number; classGroupId: number;
  }): Observable<any> {
    return this.http.post<any>(`${this.base}/assignments`, data, { headers: this.headers() });
  }

  getSubmissions(assignmentId: number): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.base}/assignments/${assignmentId}/submissions`,
      { headers: this.headers() }
    );
  }

  gradeSubmission(submissionId: number, score: number, feedback: string): Observable<any> {
    return this.http.put<any>(
      `${this.base}/submissions/${submissionId}/grade`,
      { score, feedback },
      { headers: this.headers() }
    );
  }
}

// ── Student Assignment Service ──────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class StudentAssignmentService {
  private base = 'http://localhost:5000/api/student';

  constructor(private http: HttpClient) {}

  private headers() { return bearerHeaders('student_token'); }

  getMyAssignments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/assignments`, { headers: this.headers() });
  }

  submitAssignment(assignmentId: number, content: string): Observable<any> {
    return this.http.post<any>(
      `${this.base}/assignments/${assignmentId}/submit`,
      { content },
      { headers: this.headers() }
    );
  }

  getMySubmissions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/submissions`, { headers: this.headers() });
  }
}

// ── Activation Service ──────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class ActivationService {
  private base = 'http://localhost:5000/api/auth';

  constructor(private http: HttpClient) {}

  checkStatus(token: string, role: string): Observable<{
    isActivated: boolean; fullName: string; email: string; role: string;
  }> {
    return this.http.get<any>(`${this.base}/check-activation?token=${token}&role=${role}`);
  }

  activate(payload: {
    role: string; activationToken: string; newPassword: string; confirmPassword: string;
  }): Observable<{
    userId: number; fullName: string; email: string; role: string;
    token: string; dashboard: string;
  }> {
    return this.http.post<any>(`${this.base}/activate`, payload);
  }
}
