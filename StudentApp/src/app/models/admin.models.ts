export interface Teacher {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  subject: string;
  activationToken?: string;
}

export interface Student {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  omangOrPassport: string;
  grade: number;
  teacherId: number;
  teacher?: Teacher;
  activationToken?: string;
}

export interface Course {
  id: number;
  name: string;
  code: string;
  description?: string;
}

export interface ClassGroup {
  id: number;
  name: string;
  gradeLevel: number;
  courseId: number;
  course?: Course;
  teacherId: number;
  teacher?: Teacher;
  students?: Student[];
}

export interface CreateTeacherRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  subject: string;
}

export interface CreateStudentRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  omangOrPassport: string;
  grade: number;
  teacherId: number;
}

export interface CreateCourseRequest {
  name: string;
  code: string;
  description?: string;
}

export interface CreateClassGroupRequest {
  name: string;
  gradeLevel: number;
  courseId: number;
  teacherId: number;
}

export interface EnrollStudentRequest {
  studentId: number;
}
