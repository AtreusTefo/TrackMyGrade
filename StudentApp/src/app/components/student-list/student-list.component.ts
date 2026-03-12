import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { Student } from '../../models';
import { extractErrors } from '../../services/error.util';
import DataTable from 'datatables.net-dt';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.css']
})
export class StudentListComponent implements OnInit, OnDestroy {
  @ViewChild('studentsTable') tableEl!: ElementRef;

  students: Student[] = [];
  errors: string[] = [];
  isLoading = true;
  showDeleteModal = false;
  studentToDelete: { id: number; firstName: string; lastName: string } | null = null;
  isDeleting = false;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private dtInstance: any = null;

  constructor(private studentService: StudentService) { }

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.destroyDataTable();
    this.isLoading = true;
    this.errors = [];
    this.studentService.getAllStudents().subscribe({
      next: (data) => {
        this.students = data;
        this.isLoading = false;
        setTimeout(() => this.initDataTable());
      },
      error: (error) => {
        this.errors = extractErrors(error);
        this.isLoading = false;
      }
    });
  }

  private initDataTable(): void {
    if (!this.tableEl) return;
    this.dtInstance = new DataTable(this.tableEl.nativeElement, {
      pageLength: 10,
      order: [[0, 'asc']],
      columnDefs: [{ orderable: false, searchable: false, targets: 3 }],
      language: { emptyTable: 'No students found.' }
    });
  }

  private destroyDataTable(): void {
    if (this.dtInstance) {
      this.dtInstance.destroy();
      this.dtInstance = null;
    }
  }

  openDeleteModal(id: number, firstName: string, lastName: string): void {
    this.studentToDelete = { id, firstName, lastName };
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.studentToDelete = null;
  }

  confirmDelete(): void {
    if (!this.studentToDelete) return;
    this.isDeleting = true;
    this.studentService.deleteStudent(this.studentToDelete.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.closeDeleteModal();
        this.loadStudents();
      },
      error: (error) => {
        this.errors = extractErrors(error);
        this.isDeleting = false;
        this.closeDeleteModal();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroyDataTable();
  }
}
