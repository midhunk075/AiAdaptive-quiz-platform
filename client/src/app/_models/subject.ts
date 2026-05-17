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

export interface SubjectTopic {
  id: string;
  name: string;
  isSelected: boolean;
}

export interface TopicSelectionUpdate {
  topicId: string;
  isSelected: boolean;
}
