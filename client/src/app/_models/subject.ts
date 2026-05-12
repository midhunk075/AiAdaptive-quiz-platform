export interface Subject {
    id: string;
    subjectName: string;
    description: string;
    status: string;
    questionCount: number;
    hasSyllabus: boolean;
  }
  
  export interface SubjectCreate {
    subjectName: string;
    description: string;
  }