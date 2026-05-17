import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { environment } from "../../environments/environment";
import { Subject, SubjectCreate, SubjectTopic, TopicSelectionUpdate } from "../_models/subject";
import { map } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class SubjectService {
    private http = inject(HttpClient);
    baseUrl = environment.apiUrl;

    getSubjects() {
        return this.http.get<Subject[]>(`${this.baseUrl}/subjects`).pipe(
            map(subjects => subjects.map(subject => ({
                ...subject,
                hasSyllabus: subject.hasSyllabus || false
            })))
        );
    }

    getSubjectById(id: string) {
        return this.http.get<Subject>(`${this.baseUrl}/subjects/${id}`);
    }

    createSubject(subject: SubjectCreate) {
        return this.http.post<Subject>(`${this.baseUrl}/subjects`, subject);
    }

    deleteSubject(id: string) {
        return this.http.delete(`${this.baseUrl}/subjects/${id}`);
    }

    uploadSyllabus(subjectId: string, file: File) {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<SubjectTopic[]>(`${this.baseUrl}/subjects/${subjectId}/upload-syllabus`, formData);
    }

    getTopics(subjectId: string) {
        return this.http.get<SubjectTopic[]>(`${this.baseUrl}/subjects/${subjectId}/topics`);
    }

    updateTopicSelection(subjectId: string, updates: TopicSelectionUpdate[]) {
        return this.http.put<SubjectTopic[]>(`${this.baseUrl}/subjects/${subjectId}/topics/selection`, {
            topics: updates
        });
    }
}
