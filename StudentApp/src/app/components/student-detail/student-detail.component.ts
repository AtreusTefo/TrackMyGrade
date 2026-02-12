import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { Student } from '../../models';

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
    this.studentService.getStudentById(id).subscribe(
      (data) => {
        this.student = data;
        this.isLoading = false;
      },
      (error) => {
        this.errors = [error.error || 'Failed to load student'];
        this.isLoading = false;
      }
    );
  }

  deleteStudent(): void {
    if (this.student && confirm(`Are you sure you want to delete ${this.student.firstName} ${this.student.lastName}?`)) {
      this.studentService.deleteStudent(this.student.id).subscribe(
        () => {
          this.router.navigate(['/']);
        },
        (error) => {
          this.errors = [error.error || 'Failed to delete student'];
        }
      );
    }
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
