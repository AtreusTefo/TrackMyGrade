import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Student, StudentCreate, StudentUpdate } from '../models';
import { TeacherAuthService } from './teacher-auth.service';

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private apiUrl = 'http://localhost:5000/api/students';

  constructor(
    private http: HttpClient,
    private teacherAuthService: TeacherAuthService
  ) { }

  private getHeaders(): HttpHeaders {
    const teacher = this.teacherAuthService.getCurrentTeacher();
    let headers = new HttpHeaders();
    if (teacher?.id != null) {
      headers = headers.set('X-TeacherId', teacher.id.toString());
    }
    return headers;
  }

  getAllStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  getStudentById(id: number): Observable<Student> {
    return this.http.get<Student>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  createStudent(data: StudentCreate): Observable<Student> {
    return this.http.post<Student>(this.apiUrl, data, {
      headers: this.getHeaders()
    });
  }

  updateStudent(id: number, data: StudentUpdate): Observable<Student> {
    return this.http.put<Student>(`${this.apiUrl}/${id}`, data, {
      headers: this.getHeaders()
    });
  }

  deleteStudent(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }
}
