import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin, Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { AdminApiService } from '../../services/admin-api.service';
import {
  Teacher, Student, Subject as SubjectModel, ClassGroup,
  CreateTeacherRequest, CreateStudentRequest,
  CreateSubjectRequest, CreateClassGroupRequest
} from '../../models/admin.models';
import DataTable, { Api } from 'datatables.net-dt';
import { ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';

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

  // ── ViewChild references for DataTables ──────────────────────────────────────
  @ViewChild('teachersTable') teachersTableEl!: ElementRef;
  @ViewChild('studentsTable') studentsTableEl!: ElementRef;
  @ViewChild('subjectsTable') subjectsTableEl!: ElementRef;
  @ViewChild('auditTable') auditTableEl!: ElementRef;

  // ── DataTable instances ──────────────────────────────────────────────────────
  private dtTeachers: Api<any> | null = null;
  private dtStudents: Api<any> | null = null;
  private dtSubjects: Api<any> | null = null;
  private dtAuditLogs: Api<any> | null = null;

  // ── Active tab ──────────────────────────────────────────────────────────────
  activeTab: 'teachers' | 'students' | 'subjects' | 'classes' | 'audit' = 'teachers';

  // ── Admin identity ──────────────────────────────────────────────────────────
  adminName = 'Admin';

  // ── Global loading / alert state ────────────────────────────────────────────
  loading = false;
  error = '';
  success = '';

  // ── Per-form submitting flags ────────────────────────────────────────────────
  submittingTeacher = false;
  submittingStudent = false;
  submittingSubject = false;
  submittingClass = false;
  submittingEnroll = false;
  bulkUploading = false;

  // ── Data collections ─────────────────────────────────────────────────────────
  teachers: Teacher[] = [];
  students: Student[] = [];
  subjects: SubjectModel[] = [];
  classGroups: ClassGroupUI[] = [];

  // ── Teacher form ─────────────────────────────────────────────────────────────
  showTeacherForm = false;
  newTeacher: CreateTeacherRequest = this.blankTeacher();
  teacherErrors: Record<string, string> = {};
  teacherPhoneCountryCode = '+267';
  teacherPhoneNumber = '';

  // ── Student form ──────────────────────────────────────────────────────────────
  showStudentForm = false;
  newStudent: CreateStudentRequest = this.blankStudent();
  studentErrors: Record<string, string> = {};
  studentPhoneCountryCode = '+267';
  studentPhoneNumber = '';

  // ── Subject form ───────────────────────────────────────────────────────────────
  showSubjectForm = false;
  newSubject: CreateSubjectRequest = this.blankSubject();
  subjectErrors: Record<string, string> = {};

  // ── Class form ────────────────────────────────────────────────────────────────
  showClassForm = false;
  newClass: CreateClassGroupRequest = this.blankClass();
  classErrors: Record<string, string> = {};

  // ── Audit Logs ────────────────────────────────────────────────────────────────
  auditLogs: any[] = [];
  filteredAuditLogs: any[] = [];
  auditEntityFilter = '';
  auditActionFilter = '';

  // ── Alert timer handles ───────────────────────────────────────────────────────
  private errorTimer: ReturnType<typeof setTimeout> | null = null;
  private successTimer: ReturnType<typeof setTimeout> | null = null;

  // ── Destroy signal for subscriptions ─────────────────────────────────────────
  private destroy$ = new Subject<void>();

  constructor(
    private adminApi: AdminApiService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

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
    this.destroyAllDataTables();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── DataTable initialization & cleanup ────────────────────────────────────────

  private destroyAllDataTables(): void {
    if (this.dtTeachers) {
      this.dtTeachers.destroy();
      this.dtTeachers = null;
    }
    if (this.dtStudents) {
      this.dtStudents.destroy();
      this.dtStudents = null;
    }
    if (this.dtSubjects) {
      this.dtSubjects.destroy();
      this.dtSubjects = null;
    }
    if (this.dtAuditLogs) {
      this.dtAuditLogs.destroy();
      this.dtAuditLogs = null;
    }
  }

  private initDataTableTeachers(): void {
    if (!this.teachersTableEl) return;
    this.dtTeachers = new DataTable(this.teachersTableEl.nativeElement, {
      pageLength: 10,
      lengthMenu: [5, 10, 25, 50],
      order: [[0, 'asc']],
      columnDefs: [{ orderable: false, searchable: false, targets: -1 }],
      language: { emptyTable: 'No teachers found.' }
    });
  }

  private initDataTableStudents(): void {
    if (!this.studentsTableEl) return;
    this.dtStudents = new DataTable(this.studentsTableEl.nativeElement, {
      pageLength: 10,
      lengthMenu: [5, 10, 25, 50],
      order: [[0, 'asc']],
      columnDefs: [{ orderable: false, searchable: false, targets: -1 }],
      language: { emptyTable: 'No students found.' }
    });
  }

  private initDataTableSubjects(): void {
    if (!this.subjectsTableEl) return;
    this.dtSubjects = new DataTable(this.subjectsTableEl.nativeElement, {
      pageLength: 10,
      lengthMenu: [5, 10, 25, 50],
      order: [[0, 'asc']],
      columnDefs: [{ orderable: false, searchable: false, targets: -1 }],
      language: { emptyTable: 'No subjects found.' }
    });
  }

  private initDataTableAuditLogs(): void {
    if (!this.auditTableEl) return;
    this.dtAuditLogs = new DataTable(this.auditTableEl.nativeElement, {
      pageLength: 25,
      lengthMenu: [10, 25, 50, 100],
      order: [[5, 'desc']],
      columnDefs: [{ orderable: false, searchable: false, targets: -1 }],
      language: { emptyTable: 'No audit logs found.' }
    });
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

  private blankSubject(): CreateSubjectRequest {
    return { name: '', code: '', description: '' };
  }

  private blankClass(): CreateClassGroupRequest {
    return { name: '', gradeLevel: 1, subjectId: 0, teacherId: 0 };
  }

  // ── Primary data load using forkJoin ──────────────────────────────────────────

  loadData(): void {
    this.loading = true;
    this.error = '';
    this.destroyAllDataTables();

    forkJoin({
      teachers: this.adminApi.getAllTeachers(),
      students: this.adminApi.getAllStudents(),
      subjects: this.adminApi.getAllSubjects(),
      classGroups: this.adminApi.getAllClassGroups()
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.loading = false; })
      )
      .subscribe({
        next: ({ teachers, students, subjects, classGroups }) => {
          this.teachers = teachers;
          this.students = students;
          this.subjects = subjects;
          this.classGroups = classGroups.map((cg: ClassGroup) => ({ ...cg, selectedStudentId: 0 }));
          this.cdr.detectChanges();
          this.initDataTableTeachers();
          this.initDataTableStudents();
          this.initDataTableSubjects();
        },
        error: (e) => { this.showError(e); }
      });
  }

  // ── Tab lazy-load helpers ─────────────────────────────────────────────────────
  // Guard: only fetch when collection is empty (called after activeTab is updated)

  loadTeachersIfNeeded(): void {
    if (this.teachers.length > 0) return;

    this.loading = true;
    this.destroyAllDataTables();
    this.adminApi.getAllTeachers()
      .pipe(takeUntil(this.destroy$), finalize(() => { this.loading = false; }))
      .subscribe({
        next: (data) => {
          this.teachers = data;
          this.cdr.detectChanges();
          this.initDataTableTeachers();
        },
        error: (e) => { this.showError(e); }
      });
  }

  loadStudentsIfNeeded(): void {
    if (this.students.length > 0) return;

    this.loading = true;
    this.destroyAllDataTables();
    this.adminApi.getAllStudents()
      .pipe(takeUntil(this.destroy$), finalize(() => { this.loading = false; }))
      .subscribe({
        next: (data) => {
          this.students = data;
          this.cdr.detectChanges();
          this.initDataTableStudents();
        },
        error: (e) => { this.showError(e); }
      });
  }

  loadAuditLogsIfNeeded(): void {
    if (this.auditLogs.length > 0) return;
    this.refreshAuditLogs();
  }

  refreshAuditLogs(): void {
    this.loading = true;
    this.destroyAllDataTables();
    this.adminApi.getAuditLogs({ pageNumber: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$), finalize(() => { this.loading = false; }))
      .subscribe({
        next: (response: any) => {
          // API may return { records: [] } or a plain array
          this.auditLogs = response?.records ?? (Array.isArray(response) ? response : []);
          this.filteredAuditLogs = this.auditLogs;
          this.applyAuditFilter();
          this.cdr.detectChanges();
          this.initDataTableAuditLogs();
        },
        error: (e) => { this.showError(e); }
      });
  }

  // ── Validation helpers ────────────────────────────────────────────────────────

  private validateEmail(email: string): string {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email || !email.trim()) return 'Email is required';
    if (!emailRegex.test(email)) return 'Invalid email format';
    return '';
  }

  private validatePhone(phone: string | undefined): string {
    if (!phone || !phone.trim()) return '';
    const phoneRegex = /^\+?[0-9\-\(\)\s]{7,}$/;
    if (!phoneRegex.test(phone)) return 'Invalid phone format (digits, spaces, +, -, () allowed)';
    if (phone.length > 20) return 'Phone cannot exceed 20 characters';
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
    this.teacherErrors['lastName'] = this.validateName(this.newTeacher.lastName, 'Last name');
    this.teacherErrors['email'] = this.validateEmail(this.newTeacher.email);
    
    this.newTeacher.phone = this.teacherPhoneNumber ? `${this.teacherPhoneCountryCode} ${this.teacherPhoneNumber.trim()}` : '';
    this.teacherErrors['phone'] = this.validatePhone(this.newTeacher.phone);

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
          this.teacherPhoneNumber = '';
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
    this.studentErrors['firstName'] = this.validateName(this.newStudent.firstName, 'First name');
    this.studentErrors['lastName'] = this.validateName(this.newStudent.lastName, 'Last name');
    this.studentErrors['email'] = this.validateEmail(this.newStudent.email);
    
    this.newStudent.phone = this.studentPhoneNumber ? `${this.studentPhoneCountryCode} ${this.studentPhoneNumber.trim()}` : '';
    this.studentErrors['phone'] = this.validatePhone(this.newStudent.phone);

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
          this.studentPhoneNumber = '';
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

  // ── Subjects ───────────────────────────────────────────────────────────────────

  validateSubjectForm(): boolean {
    this.clearErrors(this.subjectErrors);

    if (!this.newSubject.name || !this.newSubject.name.trim()) {
      this.subjectErrors['name'] = 'Subject name is required';
    } else if (this.newSubject.name.trim().length > 200) {
      this.subjectErrors['name'] = 'Subject name cannot exceed 200 characters';
    }

    const code = this.newSubject.code?.trim().toUpperCase() ?? '';
    if (!code) {
      this.subjectErrors['code'] = 'Subject code is required';
    } else if (code.length > 20) {
      this.subjectErrors['code'] = 'Subject code cannot exceed 20 characters';
    } else if (!/^[A-Z0-9_\-]+$/.test(code)) {
      this.subjectErrors['code'] = 'Subject code may only contain letters, digits, hyphens and underscores';
    } else if (this.subjects.some(c => c.code.toUpperCase() === code)) {
      // Client-side duplicate check for referential integrity
      this.subjectErrors['code'] = `Subject code "${code}" already exists`;
    }

    if (this.newSubject.description && this.newSubject.description.length > 500) {
      this.subjectErrors['description'] = 'Description cannot exceed 500 characters';
    }

    return !Object.values(this.subjectErrors).some(e => e);
  }

  createSubject(): void {
    if (!this.validateSubjectForm() || this.submittingSubject) return;

    this.submittingSubject = true;
    this.adminApi.createSubject(this.newSubject)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingSubject = false; }))
      .subscribe({
        next: (c: SubjectModel) => {
          this.subjects.unshift(c);
          this.showSubjectForm = false;
          this.newSubject = this.blankSubject();
          this.clearErrors(this.subjectErrors);
          this.showSuccess('Subject created successfully.');
          this.scheduleAuditRefresh();
        },
        error: (e) => { this.showError(e); }
      });
  }

  deleteSubject(id: number, name: string): void {
    if (!confirm(`Delete subject "${name}"? This will fail if there are classes assigned to it.`)) return;
    if (this.submittingSubject) return;

    this.submittingSubject = true;
    this.adminApi.deleteSubject(id)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingSubject = false; }))
      .subscribe({
        next: () => {
          this.subjects = this.subjects.filter(s => s.id !== id);
          this.showSuccess('Subject deleted successfully.');
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

    if (!this.newClass.subjectId || this.newClass.subjectId <= 0) {
      this.classErrors['subjectId'] = 'Please select a subject';
    }

    if (!this.newClass.teacherId || this.newClass.teacherId <= 0) {
      this.classErrors['teacherId'] = 'Please select a teacher';
    }

    return !Object.values(this.classErrors).some(e => e);
  }

  createClassGroup(): void {
    if (!this.validateClassForm() || this.submittingClass) return;

    // Referential integrity: verify both FK targets still exist locally
    const selectedSubject = this.subjects.find(c => c.id === this.newClass.subjectId);
    const selectedTeacher = this.teachers.find(t => t.id === this.newClass.teacherId);

    if (!selectedSubject) {
      this.showError('Selected subject is no longer available. Please refresh and try again.');
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
            subject: c.subject ?? selectedSubject,
            teacher: c.teacher ?? selectedTeacher,
            students: c.students ?? [],
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

  deleteClassGroup(id: number, name: string): void {
    if (!confirm(`Delete class group "${name}"? This will fail if there are active enrollments or assignments.`)) return;
    if (this.submittingClass) return;

    this.submittingClass = true;
    this.adminApi.deleteClassGroup(id)
      .pipe(takeUntil(this.destroy$), finalize(() => { this.submittingClass = false; }))
      .subscribe({
        next: () => {
          this.classGroups = this.classGroups.filter(cg => cg.id !== id);
          this.showSuccess('Class group deleted successfully.');
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
    const student = this.students.find(s => s.id === sid);

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

  // ── Bulk Import ───────────────────────────────────────────────────────────────

  triggerFileInput(inputId: string): void {
    const el = document.getElementById(inputId) as HTMLInputElement;
    if (el) el.click();
  }

  handleBulkImport(event: any, type: 'teachers' | 'students' | 'subjects' | 'classes'): void {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      const text = e.target.result;
      this.processBulkCsv(text, type);
    };
    reader.readAsText(file);
    // Reset input
    event.target.value = '';
  }

  private processBulkCsv(csvText: string, type: string): void {
    const lines = csvText.split(/\r?\n/).filter(l => l.trim().length > 0);
    if (lines.length <= 1) {
      this.showError('CSV file is empty or only contains headers.');
      return;
    }

    const headers = lines[0].split(',').map(h => h.trim().toLowerCase());
    const dataRows = lines.slice(1);
    
    this.bulkUploading = true;
    this.loading = true;

    const createPromises: Promise<any>[] = [];

    for (const row of dataRows) {
      // Basic CSV split, ignores quotes (assuming simple CSV)
      const cols = row.split(',').map(c => c.trim().replace(/^"|"$/g, ''));
      const record: any = {};
      headers.forEach((h, i) => {
        record[h] = cols[i] || '';
      });

      if (type === 'teachers') {
        const payload: CreateTeacherRequest = {
          firstName: record['firstname'] || record['first name'] || '',
          lastName: record['lastname'] || record['last name'] || '',
          email: record['email'] || '',
          phone: record['phone'] || '',
          subject: record['subject'] || ''
        };
        if (payload.firstName && payload.lastName && payload.email && payload.subject) {
          createPromises.push(new Promise((resolve) => {
            this.adminApi.createTeacher(payload).subscribe({
              next: (res) => resolve({ success: true, data: res }),
              error: (err) => resolve({ success: false, error: err })
            });
          }));
        }
      } else if (type === 'students') {
        const payload: CreateStudentRequest = {
          firstName: record['firstname'] || record['first name'] || '',
          lastName: record['lastname'] || record['last name'] || '',
          email: record['email'] || '',
          phone: record['phone'] || '',
          omangOrPassport: record['omang'] || record['passport'] || record['omangorpassport'] || '',
          grade: parseInt(record['grade'], 10) || 1,
          teacherId: parseInt(record['teacherid'], 10) || 0
        };
        if (payload.firstName && payload.lastName && payload.email && payload.omangOrPassport && payload.teacherId > 0) {
          createPromises.push(new Promise((resolve) => {
            this.adminApi.createStudent(payload).subscribe({
              next: (res) => resolve({ success: true, data: res }),
              error: (err) => resolve({ success: false, error: err })
            });
          }));
        }
      } else if (type === 'subjects') {
        const payload: CreateSubjectRequest = {
          name: record['name'] || record['subjectname'] || '',
          code: record['code'] || record['subjectcode'] || '',
          description: record['description'] || ''
        };
        if (payload.name && payload.code) {
          createPromises.push(new Promise((resolve) => {
            this.adminApi.createSubject(payload).subscribe({
              next: (res) => resolve({ success: true, data: res }),
              error: (err) => resolve({ success: false, error: err })
            });
          }));
        }
      } else if (type === 'classes') {
        const payload: CreateClassGroupRequest = {
          name: record['name'] || record['classname'] || '',
          gradeLevel: parseInt(record['gradelevel'] || record['grade'], 10) || 1,
          subjectId: parseInt(record['subjectid'], 10) || 0,
          teacherId: parseInt(record['teacherid'], 10) || 0
        };
        if (payload.name && payload.subjectId > 0 && payload.teacherId > 0) {
          createPromises.push(new Promise((resolve) => {
            this.adminApi.createClassGroup(payload).subscribe({
              next: (res) => resolve({ success: true, data: res }),
              error: (err) => resolve({ success: false, error: err })
            });
          }));
        }
      }
    }

    if (createPromises.length === 0) {
      this.bulkUploading = false;
      this.loading = false;
      this.showError('No valid rows found to import. Check required columns.');
      return;
    }

    Promise.all(createPromises).then(results => {
      this.bulkUploading = false;
      const successes = results.filter(r => r.success);
      const failures = results.filter(r => !r.success);
      
      if (failures.length === 0) {
        this.showSuccess(`Successfully imported ${successes.length} ${type}.`);
      } else {
        this.showError(`Import finished: ${successes.length} succeeded, ${failures.length} failed. Check console for details.`);
        console.warn('Bulk import failures:', failures);
      }
      this.loadData();
    });
  }

  // ── Audit filter ──────────────────────────────────────────────────────────────

  applyAuditFilter(): void {
    this.filteredAuditLogs = this.auditLogs.filter(log => {
      // Align property names with API response (entityType / action)
      const entityMatch = !this.auditEntityFilter || log.entityType === this.auditEntityFilter;
      const actionMatch = !this.auditActionFilter || log.action === this.auditActionFilter;
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
      this.teacherPhoneNumber = '';
      this.clearErrors(this.teacherErrors);
    }
  }

  toggleStudentForm(): void {
    this.showStudentForm = !this.showStudentForm;
    if (!this.showStudentForm) {
      this.newStudent = this.blankStudent();
      this.studentPhoneNumber = '';
      this.clearErrors(this.studentErrors);
    }
  }

  toggleSubjectForm(): void {
    this.showSubjectForm = !this.showSubjectForm;
    if (!this.showSubjectForm) {
      this.newSubject = this.blankSubject();
      this.clearErrors(this.subjectErrors);
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