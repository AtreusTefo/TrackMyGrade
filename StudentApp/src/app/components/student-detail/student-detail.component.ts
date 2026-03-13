import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { Student } from '../../models';
import { extractErrors } from '../../services/error.util';

@Component({
  selector: 'app-student-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-detail.component.html',
  styleUrls: ['./student-detail.component.css']
})
export class StudentDetailComponent implements OnInit {
  student: Student | null = null;
  errors: string[] = [];
  isLoading = true;
  showDeleteModal = false;
  isDeleting = false;

  constructor(
    private studentService: StudentService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadStudent(parseInt(id, 10));
    }
  }

  loadStudent(id: number): void {
    this.isLoading = true;
    this.studentService.getStudentById(id).subscribe({
      next: (data) => {
        this.student = data;
        this.isLoading = false;
      },
      error: (error) => {
        this.errors = extractErrors(error);
        this.isLoading = false;
      }
    });
  }

  openDeleteModal(): void {
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
  }

  confirmDelete(): void {
    if (!this.student) return;
    this.isDeleting = true;
    this.studentService.deleteStudent(this.student.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.closeDeleteModal();
        this.router.navigate(['/']);
      },
      error: (error) => {
        this.errors = extractErrors(error);
        this.isDeleting = false;
        this.closeDeleteModal();
      }
    });
  }

  getPerformanceLevelClass(level: string): string {
    switch (level) {
      case 'Excellent': return 'excellent';
      case 'Good': return 'good';
      case 'Satisfactory': return 'satisfactory';
      case 'Needs Support': return 'needs-support';
      default: return '';
    }
  }
}
