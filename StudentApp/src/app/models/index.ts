export interface Teacher {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subject: string;
  token: string;
}

export interface Student {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: number;
  assessment1: number;
  assessment2: number;
  assessment3: number;
  total: number;
  average: number;
  percentage: number;
  performanceLevel: string;
}

export interface TeacherRegister {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subject: string;
  password: string;
}

export interface TeacherLogin {
  email: string;
  password: string;
}

export interface StudentCreate {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: number;
  assessment1: number;
  assessment2: number;
  assessment3: number;
}

export interface StudentUpdate {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: number;
  assessment1: number;
  assessment2: number;
  assessment3: number;
}
