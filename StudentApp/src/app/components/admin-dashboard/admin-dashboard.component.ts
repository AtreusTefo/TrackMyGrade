import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin, Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { AdminApiService } from '../../services/admin-api.service';
import {
  Teacher, Student, Course, ClassGroup,
  CreateTeacherRequest, CreateStudentRequest,
  CreateCourseRequest, CreateClassGroupRequest
} from '../../models/admin.models';

// ── Token key constant (matches auth components) ──────────────────────────────
export const ADMIN_TOKEN_KEY = 'adminToken';

// ── Extended ClassGroup with per-row UI state ─────────────────────────────────
interface ClassGroupUI extends ClassGroup {
  selectedStudentId?: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {

  // ── Active tab ──────────────────────────────────────────────────────────────
  activeTab: 'teachers' | 'students' | 'courses' | 'classes' | 'audit' = 'teachers';

  // ── Admin identity ──────────────────────────────────────────────────────────
  adminName = 'Admin';

  // ── Global loading / alert state ────────────────────────────────────────────
  loading = false;
  error = '';
  success = '';

  // ── Per-form submitting flags ────────────────────────────────────────────────
  submittingTeacher = false;
  submittingStudent = false;
  submittingCourse  = false;
  submittingClass   = false;
  submittingEnroll  = false;

  // ── Data collections ─────────────────────────────────────────────────────────
  teachers:    Teacher[]       = [];
  students:    Student[]       = [];
  courses:     Course[]        = [];
  classGroups: ClassGroupUI[]  = [];

  // ── Teacher form ─────────────────────────────────────────────────────────────
  showTeacherForm = false;
  newTeacher: CreateTeacherRequest = this.blankTeacher();
  teacherErrors: Record<string, string> = {};

  // ── Student form ──────────────────────────────────────────────────────────────
  showStudentForm = false;
  newStudent: CreateStudentRequest = this.blankStudent();
  studentErrors: Record<string, string> = {};

  // ── Course form ───────────────────────────────────────────────────────────────
  showCourseForm = false;
  newCourse: CreateCourseRequest = this.blankCourse();
  courseErrors: Record<string, string> = {};

  // ── Class form ────────────────────────────────────────────────────────────────
  showClassForm = false;
  newClass: CreateClassGroupRequest = this.blankClass();
  classErrors: Record<string, string> = {};

  // ── Audit Logs ────────────────────────────────────────────────────────────────
  auditLogs: any[]         = [];
  filteredAuditLogs: any[] = [];
  auditEntityFilter        = '';
  auditActionFilter        = '';

  // ── Alert timer handles ───────────────────────────────────────────────────────
  private errorTimer: ReturnType<typeof setTimeout> | null = null;
  private successTimer: ReturnType<typeof setTimeout> | null = null;

  // ── Destroy signal for subscriptions ─────────────────────────────────────────
  private destroy$ = new Subject<void>();

  constructor(private adminApi: AdminApiService, private router: Router) {}

  // ── Lifecycle ─────────────────────────────────────────────────────────────────

  ngOnInit(): void {
    const token = localStorage.getItem(ADMIN_TOKEN_KEY);
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }
    this.adminName = this.extractNameFromToken(token);
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── JWT helper ────────────────────────────────────────────────────────────────

  private extractNameFromToken(token: string): string {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return (
        payload['name'] ||
        payload['email'] ||
        payload['sub'] ||
        'Admin'
      );
    } catch {
      return 'Admin';
    }
  }

  // ── Blank model factories ─────────────────────────────────────────────────────

  private blankTeacher(): CreateTeacherRequest {
    return { firstName: '', lastName: '', email: '', phone: '', subject: '' };
  }

  private blankStudent(): CreateStudentRequest {
    return { firstName: '', lastName: '', email: '', phone: '', omangOrPassport: '', grade: 1, teacherId: 0 };
  }

  private blankCourse(): CreateCourseRequest {
    return { name: '', code: '', description: '' };
  }

  private blankClass(): CreateClassGroupRequest {
    return { name: '', gradeLevel: 1, courseId: 0, teacherId: 0 };
  }

  // ── Primary data load using forkJoin ──────────────────────────────────────────

  loadData(): void {
    this.loading = true;
    this.error   = '';

    forkJoin({
      teachers:    this.adminApi.getAllTeachers(),
      students:    this.adminApi.getAllStudents(),
      courses:     this.adminApi.getAllCourses(),
      classGroups: this.adminApi.getAllClassGroups()
    })
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => { this.loading = false; })
    )
    .subscribe({
      next: ({ teachers, students, courses, classGroups }) => {
        this.teachers    = teachers;
        this.students    = students;
        this.courses     = courses;
        this.classGroups = classGroups.map((cg: ClassGroup) => ({ ...cg, selectedStudentId: 0 }));
      },
      error: (e) => { this.showError(e); }
    });
  }

  // ── Tab lazy-load helpers ─────────────────────────────────────────────────────
  // Guard: only fetch when collection is empty (called after activeTab is updated)

  loadTeachersIfNeeded(): void {
    if (this.teachers.length > 0) return;

    this.loading = true;
    this.adminApi.getAllTeachers()
      .pipe(takeUntil(this.destroy$), finalize(() => { this.loading = false; }))
      .subscribe({
        next: (data) => { this.teachers = data; },
        error: (e)   => { this.showError(e); }
      });
  }

  loadStudentsIfNeeded(): void {
    if (this.students.length > 0) return;

    this.loading = true;
    this.adminApi.getAllStudents()
      .pipe(takeUntil(this.destroy$), finalize(() => { this.loading = false; }))
      .subscribe({
        next: (data) => { this.students = data; },
        error: (e)   => { this.showError(e); }
      });
  }

  loadAuditLogsIfNeeded(): void {
    if (this.auditLogs.length > 0) return;
    this.refreshAuditLogs();
  }

  refreshAuditLogs(): void {
    this.loading = true;
    this.adminApi.getAuditLogs({ pageNumber: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$), finalize(() => { this.loading = false; }))
      .subscribe({
        next: (response: any) => {
          // API may return { records: [] } or a plain array
          this.auditLogs         = response?.records ?? (Array.isArray(response) ? response : []);
          this.filteredAuditLogs = this.auditLogs;
          this.applyAuditFilter();
        },
        error: (e) => { this.showError(e); }
      });
  }

  // ── Validation helpers ────────────────────────────────────────────────────────

  private validateEmail(email: string): string {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email || !email.trim()) return 'Email is required';
    if (!emailRegex.test(email))  return 'Invalid email format';
    return '';
  }

  private validatePhone(phone: string | undefined): string {
    if (!phone || !phone.trim()) return '';
    const phoneRegex = /^\+?[0-9\-\(\)\s]{7,}$/;
    if (!phoneRegex.test(phone)) return 'Invalid phone format (digits, spaces, +, -, () allowed)';
    if (phone.length > 20)       return 'Phone cannot exceed 20 characters';
    return '';
  }

  private validateName(name: string, fieldName: string): string {
    if (!name || !name.trim()) return `${fieldName} is required`;
    if (name.trim().length > 100) return `${fieldName} cannot exceed 100 characters`;
    return '';
  }

  private clearErrors(errorObj: Record<string, string>): void {
    Object.keys(errorObj).forEach(key => { errorObj[key] = ''; });
  }

  // ── Teachers ──────────────────────────────────────────────────────────────────

  validateTeacherForm(): boolean {
    this.clearErrors(this.teacherErrors);
    this.teacherErrors['firstName'] = this.validateName(this.newTeacher.firstName, 'First name');
    this.teacherErrors['lastName']  = this.validateName(this.newTeacher.lastName, 'Last name');
    this.teacherErrors['email']     = this.validateEmail(this.newTeacher.email);
    this.teacherErrors['phone']     = this.validatePhone(this.newTeacher.phone);

    if (!this.newTeacher.subject || !this.newTeacher.subject.trim()) {
      this.teacherErrors['subject'] = 'Subject is required';
    } else if (this.newTeacher.subject.length > 100) {
      this.teacherErrors['subject'] = 'Subject cannot exceed 100 characters';
    }

    // Client-side duplicate email check
    const emailLower = this.newTeacher.email.toLowerCase().trim();
    if (!this.teacherErrors['email'] && this.teachers.some(t => t.email.toLowerCase() === emailLower)) {
      this.teacherErrors['email'] = 'A teacher with this email already exists';
    }

    return !Object.values(this.teacherErrors).some(e => e);
  }

  createTeacher(): void {
    if (!this.validateTeacherForm() || this.submittingTeacher) return;

    this.submittingTeacher = true;
    this.adminApi.createTeacher(this.newTeacher)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingTeacher = false; }))
      .subscribe({
        next: (t: Teacher) => {
          this.teachers.unshift(t);
          this.showTeacherForm = false;
          this.newTeacher = this.blankTeacher();
          this.clearErrors(this.teacherErrors);
          this.showSuccess('Teacher created. Share the activation link from the list below.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  deleteTeacher(id: number, name: string): void {
    if (!confirm(`Delete teacher "${name}"? This will fail if they have active classes.`)) return;
    if (this.submittingTeacher) return;

    this.submittingTeacher = true;
    this.adminApi.deleteTeacher(id)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingTeacher = false; }))
      .subscribe({
        next: () => {
          // Remove teacher from teachers list
          this.teachers = this.teachers.filter(t => t.id !== id);
          // Cascade: clear teacher reference on any class groups that referenced this teacher
          this.classGroups = this.classGroups.filter(cg => cg.teacherId !== id);
          this.showSuccess('Teacher deleted successfully.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  // ── Students ──────────────────────────────────────────────────────────────────

  validateStudentForm(): boolean {
    this.clearErrors(this.studentErrors);
    this.studentErrors['firstName']       = this.validateName(this.newStudent.firstName, 'First name');
    this.studentErrors['lastName']        = this.validateName(this.newStudent.lastName, 'Last name');
    this.studentErrors['email']           = this.validateEmail(this.newStudent.email);
    this.studentErrors['phone']           = this.validatePhone(this.newStudent.phone);

    // OMANG / Passport validation - alphanumeric only, required, 4-20 chars
    const omang = this.newStudent.omangOrPassport?.trim() ?? '';
    if (!omang) {
      this.studentErrors['omangOrPassport'] = 'OMANG or Passport number is required';
    } else if (!/^[a-zA-Z0-9]+$/.test(omang)) {
      this.studentErrors['omangOrPassport'] = 'OMANG/Passport must contain only letters and digits';
    } else if (omang.length < 4) {
      this.studentErrors['omangOrPassport'] = 'OMANG/Passport must be at least 4 characters';
    } else if (omang.length > 20) {
      this.studentErrors['omangOrPassport'] = 'OMANG/Passport cannot exceed 20 characters';
    }

    if (this.newStudent.grade < 1 || this.newStudent.grade > 12) {
      this.studentErrors['grade'] = 'Grade must be between 1 and 12';
    }

    if (!this.newStudent.teacherId || this.newStudent.teacherId <= 0) {
      this.studentErrors['teacherId'] = 'Please select a teacher';
    }

    // Client-side duplicate email check
    const emailLower = this.newStudent.email.toLowerCase().trim();
    if (!this.studentErrors['email'] && this.students.some(s => s.email.toLowerCase() === emailLower)) {
      this.studentErrors['email'] = 'A student with this email already exists';
    }

    // Client-side duplicate OMANG check
    if (!this.studentErrors['omangOrPassport'] &&
        this.students.some(s => s.omangOrPassport?.toLowerCase() === omang.toLowerCase())) {
      this.studentErrors['omangOrPassport'] = 'A student with this OMANG/Passport already exists';
    }

    return !Object.values(this.studentErrors).some(e => e);
  }

  createStudent(): void {
    if (!this.validateStudentForm() || this.submittingStudent) return;

    // Referential integrity: confirm teacher still exists locally
    const selectedTeacher = this.teachers.find(t => t.id === this.newStudent.teacherId);
    if (!selectedTeacher) {
      this.showError('Selected teacher is no longer available. Please refresh and try again.');
      return;
    }

    this.submittingStudent = true;
    this.adminApi.createStudent(this.newStudent)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingStudent = false; }))
      .subscribe({
        next: (s: Student) => {
          this.students.unshift(s);
          this.showStudentForm = false;
          this.newStudent = this.blankStudent();
          this.clearErrors(this.studentErrors);
          this.showSuccess('Student created. Share the activation link from the list below.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  deleteStudent(id: number, name: string): void {
    if (!confirm(`Delete student "${name}"? All enrollments and submissions will be removed.`)) return;
    if (this.submittingStudent) return;

    this.submittingStudent = true;
    this.adminApi.deleteStudent(id)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingStudent = false; }))
      .subscribe({
        next: () => {
          // Remove from students list
          this.students = this.students.filter(s => s.id !== id);
          // Cascade: remove from all class group student lists
          this.classGroups.forEach(cg => {
            if (cg.students) {
              cg.students = cg.students.filter(st => st.id !== id);
            }
          });
          this.showSuccess('Student deleted successfully.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  // ── Courses ───────────────────────────────────────────────────────────────────

  validateCourseForm(): boolean {
    this.clearErrors(this.courseErrors);

    if (!this.newCourse.name || !this.newCourse.name.trim()) {
      this.courseErrors['name'] = 'Course name is required';
    } else if (this.newCourse.name.trim().length > 200) {
      this.courseErrors['name'] = 'Course name cannot exceed 200 characters';
    }

    const code = this.newCourse.code?.trim().toUpperCase() ?? '';
    if (!code) {
      this.courseErrors['code'] = 'Course code is required';
    } else if (code.length > 20) {
      this.courseErrors['code'] = 'Course code cannot exceed 20 characters';
    } else if (!/^[A-Z0-9_\-]+$/.test(code)) {
      this.courseErrors['code'] = 'Course code may only contain letters, digits, hyphens and underscores';
    } else if (this.courses.some(c => c.code.toUpperCase() === code)) {
      // Client-side duplicate check for referential integrity
      this.courseErrors['code'] = `Course code "${code}" already exists`;
    }

    if (this.newCourse.description && this.newCourse.description.length > 500) {
      this.courseErrors['description'] = 'Description cannot exceed 500 characters';
    }

    return !Object.values(this.courseErrors).some(e => e);
  }

  createCourse(): void {
    if (!this.validateCourseForm() || this.submittingCourse) return;

    this.submittingCourse = true;
    this.adminApi.createCourse(this.newCourse)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingCourse = false; }))
      .subscribe({
        next: (c: Course) => {
          this.courses.unshift(c);
          this.showCourseForm = false;
          this.newCourse = this.blankCourse();
          this.clearErrors(this.courseErrors);
          this.showSuccess('Course created successfully.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  // ── Classes ───────────────────────────────────────────────────────────────────

  validateClassForm(): boolean {
    this.clearErrors(this.classErrors);

    if (!this.newClass.name || !this.newClass.name.trim()) {
      this.classErrors['name'] = 'Class name is required';
    } else if (this.newClass.name.trim().length > 100) {
      this.classErrors['name'] = 'Class name cannot exceed 100 characters';
    }

    if (this.newClass.gradeLevel < 1 || this.newClass.gradeLevel > 12) {
      this.classErrors['gradeLevel'] = 'Grade level must be between 1 and 12';
    }

    if (!this.newClass.courseId || this.newClass.courseId <= 0) {
      this.classErrors['courseId'] = 'Please select a course';
    }

    if (!this.newClass.teacherId || this.newClass.teacherId <= 0) {
      this.classErrors['teacherId'] = 'Please select a teacher';
    }

    return !Object.values(this.classErrors).some(e => e);
  }

  createClassGroup(): void {
    if (!this.validateClassForm() || this.submittingClass) return;

    // Referential integrity: verify both FK targets still exist locally
    const selectedCourse  = this.courses.find(c => c.id === this.newClass.courseId);
    const selectedTeacher = this.teachers.find(t => t.id === this.newClass.teacherId);

    if (!selectedCourse) {
      this.showError('Selected course is no longer available. Please refresh and try again.');
      return;
    }
    if (!selectedTeacher) {
      this.showError('Selected teacher is no longer available. Please refresh and try again.');
      return;
    }

    this.submittingClass = true;
    this.adminApi.createClassGroup(this.newClass)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingClass = false; }))
      .subscribe({
        next: (c: ClassGroup) => {
          // Augment with populated navigation objects for immediate display
          const enriched: ClassGroupUI = {
            ...c,
            course:           c.course  ?? selectedCourse,
            teacher:          c.teacher ?? selectedTeacher,
            students:         c.students ?? [],
            selectedStudentId: 0
          };
          this.classGroups.unshift(enriched);
          this.showClassForm = false;
          this.newClass = this.blankClass();
          this.clearErrors(this.classErrors);
          this.showSuccess('Class group created successfully.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  enrollStudent(classGroupId: number, studentId: number | undefined): void {
    const sid = Number(studentId ?? 0);
    if (!sid || sid <= 0) {
      this.showError('Please select a student to enroll.');
      return;
    }
    if (this.submittingEnroll) return;

    // Referential integrity checks
    const classGroup = this.classGroups.find(cg => cg.id === classGroupId);
    const student    = this.students.find(s => s.id === sid);

    if (!classGroup) {
      this.showError('Class group not found. Please refresh.');
      return;
    }
    if (!student) {
      this.showError('Student not found. Please refresh.');
      return;
    }

    // Duplicate enrollment check (client-side guard)
    if (classGroup.students?.some(st => st.id === sid)) {
      this.showError(`${student.firstName} ${student.lastName} is already enrolled in this class.`);
      return;
    }

    this.submittingEnroll = true;
    this.adminApi.enrollStudent(classGroupId, sid)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingEnroll = false; }))
      .subscribe({
        next: () => {
          if (!classGroup.students) classGroup.students = [];
          classGroup.students.push(student);
          classGroup.selectedStudentId = 0;
          this.showSuccess(`${student.firstName} ${student.lastName} enrolled successfully.`);
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  unenrollStudent(classGroupId: number, studentId: number, studentName: string): void {
    if (!confirm(`Remove "${studentName}" from this class?`)) return;
    if (this.submittingEnroll) return;

    this.submittingEnroll = true;
    this.adminApi.unenrollStudent(classGroupId, studentId)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingEnroll = false; }))
      .subscribe({
        next: () => {
          const classGroup = this.classGroups.find(cg => cg.id === classGroupId);
          if (classGroup) {
            classGroup.students = classGroup.students?.filter(s => s.id !== studentId);
          }
          this.showSuccess('Student removed from class successfully.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  // ── Enrollment helpers ────────────────────────────────────────────────────

  /**
   * Returns true if the given student is already enrolled in the given class group.
   * Called from the template in place of an inline arrow function, which Angular's
   * template parser rejects (NG5002 - bindings cannot contain assignments).
   */
  isStudentEnrolled(cg: ClassGroupUI, studentId: number): boolean {
    return cg.students?.some(st => st.id === studentId) ?? false;
  }

  // ── Audit filter ──────────────────────────────────────────────────────────────

  applyAuditFilter(): void {
    this.filteredAuditLogs = this.auditLogs.filter(log => {
      // Align property names with API response (entityType / action)
      const entityMatch = !this.auditEntityFilter || log.entityType === this.auditEntityFilter;
      const actionMatch = !this.auditActionFilter || log.action      === this.auditActionFilter;
      return entityMatch && actionMatch;
    });
  }

  get uniqueAuditEntities(): string[] {
    return Array.from(new Set(this.auditLogs.map(log => log.entityType).filter(Boolean)));
  }

  get uniqueAuditActions(): string[] {
    return Array.from(new Set(this.auditLogs.map(log => log.action).filter(Boolean)));
  }

  resetAuditFilters(): void {
    this.auditEntityFilter = '';
    this.auditActionFilter = '';
    this.filteredAuditLogs = this.auditLogs;
  }

  // ── Form toggle helpers ───────────────────────────────────────────────────────

  toggleTeacherForm(): void {
    this.showTeacherForm = !this.showTeacherForm;
    if (!this.showTeacherForm) {
      this.newTeacher = this.blankTeacher();
      this.clearErrors(this.teacherErrors);
    }
  }

  toggleStudentForm(): void {
    this.showStudentForm = !this.showStudentForm;
    if (!this.showStudentForm) {
      this.newStudent = this.blankStudent();
      this.clearErrors(this.studentErrors);
    }
  }

  toggleCourseForm(): void {
    this.showCourseForm = !this.showCourseForm;
    if (!this.showCourseForm) {
      this.newCourse = this.blankCourse();
      this.clearErrors(this.courseErrors);
    }
  }

  toggleClassForm(): void {
    this.showClassForm = !this.showClassForm;
    if (!this.showClassForm) {
      this.newClass = this.blankClass();
      this.clearErrors(this.classErrors);
    }
  }

  // ── Auth ──────────────────────────────────────────────────────────────────────

  logout(): void {
    localStorage.removeItem(ADMIN_TOKEN_KEY);
    this.router.navigate(['/login']);
  }

  // ── Private helpers ───────────────────────────────────────────────────────────

  private scheduleAuditRefresh(): void {
    // Refresh audit logs only if the tab has already been opened to avoid
    // an unnecessary request on tabs the user has not visited.
    if (this.auditLogs.length > 0) {
      this.refreshAuditLogs();
    }
  }

  private showSuccess(msg: string): void {
    if (this.successTimer) clearTimeout(this.successTimer);
    this.success = msg;
    this.successTimer = setTimeout(() => { this.success = ''; }, 4000);
  }

  private showError(err: any): void {
    if (this.errorTimer) clearTimeout(this.errorTimer);
    let errorMsg = 'An unexpected error occurred. Please try again.';
    if (typeof err === 'string') {
      errorMsg = err;
    } else if (err?.error?.errors) {
      // FluentValidation array format
      const errs = err.error.errors;
      errorMsg = Object.values(errs).flat().join(' ');
    } else if (err?.error?.message) {
      errorMsg = err.error.message;
    } else if (err?.error) {
      errorMsg = typeof err.error === 'string' ? err.error : JSON.stringify(err.error);
    } else if (err?.message) {
      errorMsg = err.message;
    }
    this.error = errorMsg;
    this.errorTimer = setTimeout(() => { this.error = ''; }, 6000);
  }
}