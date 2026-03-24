import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { StudentAuthResponse, StudentLogin, StudentSubmitAssessments } from '../models';

@Injectable({
  providedIn: 'root'
})
export class StudentAuthService {
  private apiUrl = 'http://localhost:5000/api/student-auth';
  private currentStudent = new BehaviorSubject<StudentAuthResponse | null>(this.getStoredStudent());
  public currentStudent$ = this.currentStudent.asObservable();

  constructor(private http: HttpClient) { }

  login(data: StudentLogin): Observable<StudentAuthResponse> {
    return this.http.post<StudentAuthResponse>(`${this.apiUrl}/login`, data);
  }

  getProfile(): Observable<StudentAuthResponse> {
    return this.http.get<StudentAuthResponse>(`${this.apiUrl}/profile`, {
      headers: this.getHeaders()
    });
  }

  submitAssessments(data: StudentSubmitAssessments): Observable<StudentAuthResponse> {
    return this.http.put<StudentAuthResponse>(`${this.apiUrl}/submit-assessments`, data, {
      headers: this.getHeaders()
    });
  }

  logout(): void {
    localStorage.removeItem('student');
    localStorage.removeItem('studentToken');
    this.currentStudent.next(null);
  }

  setCurrentStudent(student: any): void {
    const normalized: StudentAuthResponse = {
      id: student.id ?? student.Id ?? 0,
      studentNumber: student.studentNumber ?? student.StudentNumber ?? '',
      firstName: student.firstName ?? student.FirstName ?? '',
      lastName: student.lastName ?? student.LastName ?? '',
      email: student.email ?? student.Email ?? '',
      phone: student.phone ?? student.Phone ?? '',
      omangOrPassport: student.omangOrPassport ?? student.OmangOrPassport ?? '',
      grade: student.grade ?? student.Grade ?? 0,
      assessment1: student.assessment1 ?? student.Assessment1 ?? 0,
      assessment2: student.assessment2 ?? student.Assessment2 ?? 0,
      assessment3: student.assessment3 ?? student.Assessment3 ?? 0,
      total: student.total ?? student.Total ?? 0,
      average: student.average ?? student.Average ?? 0,
      percentage: student.percentage ?? student.Percentage ?? 0,
      performanceLevel: student.performanceLevel ?? student.PerformanceLevel ?? '',
      token: student.token ?? student.Token ?? ''
    };
    localStorage.setItem('student', JSON.stringify(normalized));
    localStorage.setItem('studentToken', normalized.token);
    this.currentStudent.next(normalized);
  }

  getCurrentStudent(): StudentAuthResponse | null {
    return this.currentStudent.value;
  }

  getToken(): string | null {
    return localStorage.getItem('studentToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private getHeaders(): HttpHeaders {
    const token = this.getToken();
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('X-StudentToken', token);
    }
    return headers;
  }

  private getStoredStudent(): StudentAuthResponse | null {
    const stored = localStorage.getItem('student');
    if (!stored) return null;
    try {
      return JSON.parse(stored);
    } catch {
      return null;
    }
  }
}
