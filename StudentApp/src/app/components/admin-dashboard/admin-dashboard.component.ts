import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminApiService } from '../../services/admin-api.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  activeTab: 'teachers' | 'students' | 'courses' | 'classes' = 'teachers';
  adminName = 'Administrator';
  loading = false;
  error = '';
  success = '';
  submitting = false;

  teachers: any[] = [];
  students: any[] = [];
  courses: any[] = [];
  classGroups: any[] = [];

  // Create forms
  showTeacherForm = false;
  newTeacher = { firstName: '', lastName: '', email: '', phone: '', subject: '' };
  teacherErrors: { [key: string]: string } = {};

  showStudentForm = false;
  newStudent = { firstName: '', lastName: '', email: '', phone: '', omangOrPassport: '', grade: 1, teacherId: 0 };
  studentErrors: { [key: string]: string } = {};

  showCourseForm = false;
  newCourse = { name: '', code: '', description: '' };
  courseErrors: { [key: string]: string } = {};

  showClassForm = false;
  newClass = { name: '', gradeLevel: 1, courseId: 0, teacherId: 0 };
  classErrors: { [key: string]: string } = {};

  constructor(private adminApi: AdminApiService, private router: Router) {}

  ngOnInit(): void {
    if (!localStorage.getItem('admin_token')) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.error = '';
    this.adminApi.getAllTeachers().subscribe(data => this.teachers = data);
    this.adminApi.getAllStudents().subscribe(data => this.students = data);
    this.adminApi.getAllCourses().subscribe(data => this.courses = data);
    this.adminApi.getAllClassGroups().subscribe(data => {
      this.classGroups = data;
      this.loading = false;
    });
  }

  // ── Validation Helpers ────────────────────────────────────────────────

  private validateEmail(email: string): string {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email) return 'Email is required';
    if (!emailRegex.test(email)) return 'Invalid email format';
    return '';
  }

  private validatePhone(phone: string): string {
    if (!phone) return '';
    const phoneRegex = /^\+?[0-9\-\(\)\s]{7,}$/;
    if (!phoneRegex.test(phone)) return 'Invalid phone format';
    if (phone.length > 20) return 'Phone cannot exceed 20 characters';
    return '';
  }

  private validateName(name: string, fieldName: string): string {
    if (!name || !name.trim()) return `${fieldName} is required`;
    if (name.length > 100) return `${fieldName} cannot exceed 100 characters`;
    return '';
  }

  private clearErrors(errorObj: any): void {
    Object.keys(errorObj).forEach(key => errorObj[key] = '');
  }

  // ── Teachers ──────────────────────────────────────────────────────────

  validateTeacherForm(): boolean {
    this.clearErrors(this.teacherErrors);
    this.teacherErrors['firstName'] = this.validateName(this.newTeacher.firstName, 'First name');
    this.teacherErrors['lastName'] = this.validateName(this.newTeacher.lastName, 'Last name');
    this.teacherErrors['email'] = this.validateEmail(this.newTeacher.email);
    this.teacherErrors['phone'] = this.validatePhone(this.newTeacher.phone);

    if (this.newTeacher.subject && this.newTeacher.subject.length > 100) {
      this.teacherErrors['subject'] = 'Subject cannot exceed 100 characters';
    }

    return !Object.values(this.teacherErrors).some(e => e);
  }

  createTeacher(): void {
    if (!this.validateTeacherForm()) return;
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.createTeacher(this.newTeacher).subscribe({
      next: (t) => {
        this.teachers.push(t);
        this.showTeacherForm = false;
        this.newTeacher = { firstName: '', lastName: '', email: '', phone: '', subject: '' };
        this.clearErrors(this.teacherErrors);
        this.showSuccess('Teacher created. Ask them to check their email for activation.');
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  deleteTeacher(id: number, name: string): void {
    if (!confirm(`Delete teacher "${name}"? This will fail if they have active classes or assignments.`)) return;
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.deleteTeacher(id).subscribe({
      next: () => {
        this.teachers = this.teachers.filter(t => t.id !== id);
        this.showSuccess('Teacher deleted.');
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  // ── Students ──────────────────────────────────────────────────────────

  validateStudentForm(): boolean {
    this.clearErrors(this.studentErrors);
    this.studentErrors['firstName'] = this.validateName(this.newStudent.firstName, 'First name');
    this.studentErrors['lastName'] = this.validateName(this.newStudent.lastName, 'Last name');
    this.studentErrors['email'] = this.validateEmail(this.newStudent.email);
    this.studentErrors['phone'] = this.validatePhone(this.newStudent.phone);

    if (!this.newStudent.omangOrPassport || !this.newStudent.omangOrPassport.trim()) {
      this.studentErrors['omangOrPassport'] = 'OMANG or Passport is required';
    } else if (this.newStudent.omangOrPassport.length > 20) {
      this.studentErrors['omangOrPassport'] = 'OMANG/Passport cannot exceed 20 characters';
    }

    if (this.newStudent.grade < 1 || this.newStudent.grade > 12) {
      this.studentErrors['grade'] = 'Grade must be between 1 and 12';
    }

    if (this.newStudent.teacherId <= 0) {
      this.studentErrors['teacherId'] = 'Please select a teacher';
    }

    return !Object.values(this.studentErrors).some(e => e);
  }

  createStudent(): void {
    if (!this.validateStudentForm()) return;
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.createStudent(this.newStudent).subscribe({
      next: (s) => {
        this.students.push(s);
        this.showStudentForm = false;
        this.newStudent = { firstName: '', lastName: '', email: '', phone: '', omangOrPassport: '', grade: 1, teacherId: 0 };
        this.clearErrors(this.studentErrors);
        this.showSuccess('Student created. Ask them to check their email for activation.');
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  deleteStudent(id: number, name: string): void {
    if (!confirm(`Delete student "${name}"? All enrollments and submissions will be removed.`)) return;
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.deleteStudent(id).subscribe({
      next: () => {
        this.students = this.students.filter(s => s.id !== id);
        this.showSuccess('Student deleted.');
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  // ── Courses ───────────────────────────────────────────────────────────

  validateCourseForm(): boolean {
    this.clearErrors(this.courseErrors);

    if (!this.newCourse.name || !this.newCourse.name.trim()) {
      this.courseErrors['name'] = 'Course name is required';
    } else if (this.newCourse.name.length > 200) {
      this.courseErrors['name'] = 'Course name cannot exceed 200 characters';
    }

    if (!this.newCourse.code || !this.newCourse.code.trim()) {
      this.courseErrors['code'] = 'Course code is required';
    } else if (this.newCourse.code.length > 20) {
      this.courseErrors['code'] = 'Course code cannot exceed 20 characters';
    }

    if (this.newCourse.description && this.newCourse.description.length > 500) {
      this.courseErrors['description'] = 'Description cannot exceed 500 characters';
    }

    return !Object.values(this.courseErrors).some(e => e);
  }

  createCourse(): void {
    if (!this.validateCourseForm()) return;
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.createCourse(this.newCourse).subscribe({
      next: (c) => {
        this.courses.push(c);
        this.showCourseForm = false;
        this.newCourse = { name: '', code: '', description: '' };
        this.clearErrors(this.courseErrors);
        this.showSuccess('Course created.');
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  // ── Classes ────────────────────────────────────────────────────────────

  validateClassForm(): boolean {
    this.clearErrors(this.classErrors);

    if (!this.newClass.name || !this.newClass.name.trim()) {
      this.classErrors['name'] = 'Class name is required';
    } else if (this.newClass.name.length > 100) {
      this.classErrors['name'] = 'Class name cannot exceed 100 characters';
    }

    if (this.newClass.gradeLevel < 1 || this.newClass.gradeLevel > 12) {
      this.classErrors['gradeLevel'] = 'Grade level must be between 1 and 12';
    }

    if (this.newClass.courseId <= 0) {
      this.classErrors['courseId'] = 'Please select a course';
    }

    if (this.newClass.teacherId <= 0) {
      this.classErrors['teacherId'] = 'Please select a teacher';
    }

    return !Object.values(this.classErrors).some(e => e);
  }

  createClassGroup(): void {
    if (!this.validateClassForm()) return;
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.createClassGroup(this.newClass).subscribe({
      next: (c) => {
        this.classGroups.push(c);
        this.showClassForm = false;
        this.newClass = { name: '', gradeLevel: 1, courseId: 0, teacherId: 0 };
        this.clearErrors(this.classErrors);
        this.showSuccess('Class created.');
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  enrollStudent(classGroupId: number, studentId: number): void {
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.enrollStudent(classGroupId, studentId).subscribe({
      next: () => {
        this.showSuccess('Student enrolled in class.');
        this.loadData();
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  unenrollStudent(classGroupId: number, studentId: number): void {
    if (!confirm('Remove this student from the class?')) return;
    if (this.submitting) return;

    this.submitting = true;
    this.adminApi.unenrollStudent(classGroupId, studentId).subscribe({
      next: () => {
        this.showSuccess('Student removed from class.');
        this.loadData();
        this.submitting = false;
      },
      error: (e) => {
        this.showError(e);
        this.submitting = false;
      }
    });
  }

  // ── Helpers ──────────────────────────────────────────────────────────

  private showSuccess(msg: string): void {
    this.success = msg;
    setTimeout(() => this.success = '', 3000);
  }

  private showError(err: any): void {
    this.error = err?.error?.message || err?.error || 'An error occurred';
    setTimeout(() => this.error = '', 5000);
  }

  logout(): void {
    localStorage.removeItem('admin_token');
    this.router.navigate(['/login']);
  }
}