import { Component, OnInit, inject, signal } from '@angular/core';
import { SubjectService } from '../_services/subject.service';
import { Subject, SubjectCreate } from '../_models/subject';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-subject-management',
  standalone: true,
  imports: [FormsModule], // CommonModule is no longer needed with @if/@for
  templateUrl: './subject-management.html',
  styleUrl: './subject-management.css',
})
export class SubjectManagement implements OnInit {
  private subjectService = inject(SubjectService);
  
  // Use signals for everything to ensure UI reactivity
  subjects = signal<Subject[]>([]);
  loading = signal(false);
  showModal = signal(false);

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