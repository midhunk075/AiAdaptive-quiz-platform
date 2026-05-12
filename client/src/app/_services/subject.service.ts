import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { environment } from "../../environments/environment";
import { Subject, SubjectCreate } from "../_models/subject";
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
}