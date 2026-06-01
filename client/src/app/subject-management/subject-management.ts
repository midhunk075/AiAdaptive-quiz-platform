import { Component, OnInit, inject, signal } from '@angular/core';
import { SubjectService } from '../_services/subject.service';
import { Subject, SubjectCreate, SubjectQuestion, SubjectTopic } from '../_models/subject';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-subject-management',
  standalone: true,
  imports: [FormsModule], 
  templateUrl: './subject-management.html',
  styleUrl: './subject-management.css',
})
export class SubjectManagement implements OnInit {
  private subjectService = inject(SubjectService);
  
  subjects = signal<Subject[]>([]);
  loading = signal(false);
  showModal = signal(false);
  selectedSubject = signal<Subject | null>(null);
  showSyllabusModal = signal(false);
  selectedFile = signal<File | null>(null);
  topics = signal<SubjectTopic[]>([]);
  uploading = signal(false);
  loadingTopics = signal(false);
  savingTopics = signal(false);
  syllabusError = signal('');
  showQuestionsModal = signal(false);
  questions = signal<SubjectQuestion[]>([]);
  loadingQuestions = signal(false);
  generatingQuestions = signal(false);
  updatingQuestion = signal(false);
  questionsError = signal('');
  previousScoreInput = signal<string>('');
  questionCountInput = signal<number>(5);

  newSubject: SubjectCreate = {
    subjectName: '',
    description: ''
  }

  ngOnInit() {
    this.loadSubjects();
  }

  loadSubjects() {
    this.loading.set(true);
    this.subjectService.getSubjects().subscribe({
      next: (response) => {
        this.subjects.set(response);
      },
      error: (err) => console.error('Error loading subjects:', err),
      complete: () => this.loading.set(false) // Always stops loading
    });
  }

  getStatusProgress(status: string) {
    const s = status?.toLowerCase();
    switch(s){
      case 'draft': return {width: '25%', color: 'bg-red-500', label: 'Initialization'};
      case 'syllabus-provided': return {width: '50%', color: 'bg-yellow-500', label: 'Syllabus Provided'};
      case 'topics-generated': return {width: '75%', color: 'bg-blue-500', label: 'In Progress'};
      case 'questions-generated': return {width: '90%', color: 'bg-green-500', label: 'Questions Generated'};
      case 'Live': return {width: '100%', color: 'bg-green-500', label: 'Completed'};
      default: return {width: '0%', color: 'bg-gray-500', label: 'Unknown'};
    }
  }

  openSyllabusModal(subject: Subject) {
    this.selectedSubject.set(subject);
    this.showSyllabusModal.set(true);
    this.selectedFile.set(null);
    this.syllabusError.set('');
    this.topics.set([]);
    this.loadTopics(subject.id);
  }

  closeSyllabusModal() {
    this.showSyllabusModal.set(false);
    this.selectedSubject.set(null);
    this.selectedFile.set(null);
    this.syllabusError.set('');
    this.topics.set([]);
    this.uploading.set(false);
    this.loadingTopics.set(false);
    this.savingTopics.set(false);
  }

  openQuestionsModal(subject: Subject) {
    this.selectedSubject.set(subject);
    this.showQuestionsModal.set(true);
    this.questions.set([]);
    this.questionsError.set('');
    this.previousScoreInput.set('');
    this.questionCountInput.set(5);
    this.loadQuestions(subject.id);
    this.loadTopics(subject.id);
  }

  closeQuestionsModal() {
    this.showQuestionsModal.set(false);
    this.selectedSubject.set(null);
    this.questions.set([]);
    this.questionsError.set('');
    this.loadingQuestions.set(false);
    this.generatingQuestions.set(false);
    this.updatingQuestion.set(false);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.selectedFile.set(null);
      return;
    }

    const fileName = file.name?.toLowerCase() ?? '';
    const isPdfMime = file.type === 'application/pdf';
    const isPdfExtension = fileName.endsWith('.pdf');
    if (!isPdfMime && !isPdfExtension) {
      this.syllabusError.set('Please select a PDF file only.');
      this.selectedFile.set(null);
      input.value = '';
      return;
    }

    this.syllabusError.set('');
    this.selectedFile.set(file);
  }

  uploadSyllabus() {
    const subject = this.selectedSubject();
    const file = this.selectedFile();

    if (!subject || !file) {
      this.syllabusError.set('Choose a PDF file before uploading.');
      return;
    }

    this.uploading.set(true);
    this.syllabusError.set('');

    this.subjectService.uploadSyllabus(subject.id, file).subscribe({
      next: (response) => {
        this.topics.set(response);
        this.subjects.update(prev => prev.map(s => s.id === subject.id ? { ...s, hasSyllabus: true, status: 'topics-generated' } : s));
        this.syllabusError.set('');
      },
      error: (err) => {
        const message = err?.error?.message ?? err?.error ?? 'Failed to upload syllabus.';
        this.syllabusError.set(message);
      },
      complete: () => this.uploading.set(false)
    });
  }

  loadTopics(subjectId: string) {
    this.loadingTopics.set(true);
    this.subjectService.getTopics(subjectId).subscribe({
      next: (response) => {
        this.topics.set(response);
      },
      error: () => {
        this.topics.set([]);
      },
      complete: () => this.loadingTopics.set(false)
    });
  }

  toggleTopicSelection(topicId: string, isSelected: boolean) {
    const subject = this.selectedSubject();
    if (!subject) return;

    const previousTopics = this.topics();
    const updatedTopics = previousTopics.map(t => t.id === topicId ? { ...t, isSelected } : t);
    this.topics.set(updatedTopics);
    this.savingTopics.set(true);

    this.subjectService.updateTopicSelection(subject.id, [{ topicId, isSelected }]).subscribe({
      next: (response) => this.topics.set(response),
      error: () => this.topics.set(previousTopics),
      complete: () => this.savingTopics.set(false)
    });
  }

  loadQuestions(subjectId: string) {
    this.loadingQuestions.set(true);
    this.subjectService.getQuestions(subjectId).subscribe({
      next: (response) => this.questions.set(response),
      error: (err) => {
        const message = err?.error?.message ?? 'Failed to load questions.';
        this.questionsError.set(message);
        this.questions.set([]);
      },
      complete: () => this.loadingQuestions.set(false)
    });
  }

  generateQuestions() {
    const subject = this.selectedSubject();
    if (!subject) return;

    const selectedTopicIds = this.topics().filter(t => t.isSelected).map(t => t.id);
    if (selectedTopicIds.length === 0) {
      this.questionsError.set('Select at least one topic before generating questions.');
      return;
    }

    if (this.questionCountInput() <= 0) {
      this.questionsError.set('Question count must be greater than zero.');
      return;
    }

    let previousScore: number | null = null;
    const rawScore = this.previousScoreInput().trim();
    if (rawScore.length > 0) {
      const parsed = Number(rawScore);
      if (Number.isNaN(parsed) || parsed < 0 || parsed > 100) {
        this.questionsError.set('Previous score must be a number between 0 and 100.');
        return;
      }
      previousScore = parsed;
    }

    this.generatingQuestions.set(true);
    this.questionsError.set('');
    this.subjectService.generateQuiz(subject.id, {
      selectedTopicIds,
      userPreviousScorePercentage: previousScore,
      totalQuestionCount: this.questionCountInput(),
      generateAllDifficultyLevels: true
    }).subscribe({
      next: () => {
        this.loadQuestions(subject.id);
        this.subjects.update(prev => prev.map(s => s.id === subject.id ? {
          ...s,
          status: 'questions-generated',
          questionCount: s.questionCount + (this.questionCountInput() * 3)
        } : s));
      },
      error: (err) => {
        const message = err?.error?.message ?? 'Failed to generate questions.';
        this.questionsError.set(message);
      },
      complete: () => this.generatingQuestions.set(false)
    });
  }

  setApproval(question: SubjectQuestion, isApproved: boolean) {
    const subject = this.selectedSubject();
    if (!subject) return;

    this.updatingQuestion.set(true);
    this.subjectService.setQuestionApproval(subject.id, question.id, isApproved).subscribe({
      next: (updated) => {
        this.questions.update(prev => prev.map(q => q.id === updated.id ? updated : q));
      },
      error: (err) => {
        const message = err?.error?.message ?? 'Failed to update question approval.';
        this.questionsError.set(message);
      },
      complete: () => this.updatingQuestion.set(false)
    });
  }

  discardQuestion(questionId: string) {
    const subject = this.selectedSubject();
    if (!subject) return;
    if (!confirm('Discard this question permanently?')) return;

    this.updatingQuestion.set(true);
    this.subjectService.discardQuestion(subject.id, questionId).subscribe({
      next: () => this.questions.update(prev => prev.filter(q => q.id !== questionId)),
      error: (err) => {
        const message = err?.error?.message ?? 'Failed to discard question.';
        this.questionsError.set(message);
      },
      complete: () => this.updatingQuestion.set(false)
    });
  }

  createSubject() {
    if (!this.newSubject.subjectName.trim()) return;

    this.subjectService.createSubject(this.newSubject).subscribe({
      next: (createdSubject) => {
        // Correct way to add to a Signal array
        this.subjects.update(prev => [...prev, createdSubject]);
        this.closeModal();
      },
      error: (err) => console.error('Error creating subject:', err)
    });
  }

  deleteSubject(id: string) {
    if (!confirm('Are you sure you want to delete this subject?')) return;

    this.subjectService.deleteSubject(id).subscribe({
      next: () => {
        // Correct way to remove from a Signal array
        this.subjects.update(prev => prev.filter(s => s.id !== id));
      },
      error: (err) => console.error('Error deleting subject:', err)
    });
  }

  openModal() {
    this.showModal.set(true);
  }
  
  closeModal() {
    this.showModal.set(false);
    this.newSubject = { subjectName: '', description: '' };
  }
}
