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

export interface Subject {
  id: number;
  name: string;
  code: string;
  description?: string;
}

export interface ClassGroup {
  id: number;
  name: string;
  gradeLevel: number;
  subjectId: number;
  subject?: Subject;
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

export interface CreateSubjectRequest {
  name: string;
  code: string;
  description?: string;
}

export interface CreateClassGroupRequest {
  name: string;
  gradeLevel: number;
  subjectId: number;
  teacherId: number;
}

export interface EnrollStudentRequest {
  studentId: number;
}
