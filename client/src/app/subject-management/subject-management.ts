import { Component, OnInit, inject, signal } from '@angular/core';
import { SubjectService } from '../_services/subject.service';
import { Subject, SubjectCreate, SubjectTopic } from '../_models/subject';
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
