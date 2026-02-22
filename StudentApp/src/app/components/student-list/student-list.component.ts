import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { Student } from '../../models';
import { extractErrors } from '../../services/error.util';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.css']
})
export class StudentListComponent implements OnInit {
  students: Student[] = [];
  errors: string[] = [];
  isLoading = true;

  constructor(private studentService: StudentService) { }

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.isLoading = true;
    this.errors = [];
    this.studentService.getAllStudents().subscribe(
      (data) => {
        this.students = data;
        this.isLoading = false;
      },
      (error) => {
        this.errors = extractErrors(error);
        this.isLoading = false;
      }
    );
  }

  deleteStudent(id: number, firstName: string, lastName: string): void {
    if (confirm(`Are you sure you want to delete ${firstName} ${lastName}?`)) {
      this.studentService.deleteStudent(id).subscribe(
        () => {
          this.loadStudents();
        },
        (error) => {
          this.errors = extractErrors(error);
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
