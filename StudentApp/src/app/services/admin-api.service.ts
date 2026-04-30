import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AdminApiService {
    private apiUrl = 'http://localhost:5000/api/admin';

    constructor(private http: HttpClient) { }

    private getHeaders(): HttpHeaders {
        const token = localStorage.getItem('admin_token');
        let headers = new HttpHeaders();
        if (token) {
            headers = headers.set('X-AdminToken', token);
        }
        return headers;
    }

    // Teachers
    getAllTeachers(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/teachers`, { headers: this.getHeaders() });
    }

    createTeacher(data: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/teachers`, data, { headers: this.getHeaders() });
    }

    updateTeacher(teacherId: number, data: any): Observable<any> {
        return this.http.put<any>(`${this.apiUrl}/teachers/${teacherId}`, data, { headers: this.getHeaders() });
    }

    deleteTeacher(teacherId: number): Observable<any> {
        return this.http.delete<any>(`${this.apiUrl}/teachers/${teacherId}`, { headers: this.getHeaders() });
    }

    resetTeacherPassword(teacherId: number): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/teachers/${teacherId}/reset-password`, {}, { headers: this.getHeaders() });
    }

    // Students
    getAllStudents(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/students`, { headers: this.getHeaders() });
    }

    createStudent(data: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/students`, data, { headers: this.getHeaders() });
    }

    updateStudent(studentId: number, data: any): Observable<any> {
        return this.http.put<any>(`${this.apiUrl}/students/${studentId}`, data, { headers: this.getHeaders() });
    }

    deleteStudent(studentId: number): Observable<any> {
        return this.http.delete<any>(`${this.apiUrl}/students/${studentId}`, { headers: this.getHeaders() });
    }

    resetStudentPassword(studentId: number): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/students/${studentId}/reset-password`, {}, { headers: this.getHeaders() });
    }

    // Teacher-Student Assignment
    assignStudentToTeacher(studentId: number, teacherId: number): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/students/${studentId}/assign-teacher`, { teacherId }, { headers: this.getHeaders() });
    }

    unassignStudentFromTeacher(studentId: number, teacherId: number): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/students/${studentId}/unassign-teacher`, { teacherId }, { headers: this.getHeaders() });
    }

    // Dropdowns & Lists
    getSubjects(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/subjects`, { headers: this.getHeaders() });
    }

    getGrades(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/grades`, { headers: this.getHeaders() });
    }

    // Audit Logs
    getAuditLogs(page: number, pageSize: number): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/audit-logs?page=${page}&pageSize=${pageSize}`, { headers: this.getHeaders() });
    }

    // Bulk Import
    bulkImportTeachers(data: any[]): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/teachers/bulk-import`, data, { headers: this.getHeaders() });
    }

    bulkImportStudents(data: any[]): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/students/bulk-import`, data, { headers: this.getHeaders() });
    }
}
