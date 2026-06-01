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

export interface AdaptiveQuizRequest {
  selectedTopicIds: string[];
  userPreviousScorePercentage?: number | null;
  totalQuestionCount: number;
  generateAllDifficultyLevels?: boolean;
}

export interface GeneratedQuestion {
  questionText: string;
  options: string[];
  correctAnswer: string;
  explanation: string;
}

export interface QuizGenerationResponse {
  questions: GeneratedQuestion[];
}

export interface SubjectQuestion {
  id: string;
  questionText: string;
  difficultyLevel: number;
  isApproved: boolean;
  topicId: string;
  topicName: string;
  correctAnswer: string;
  explanation: string;
  options: string[];
}
