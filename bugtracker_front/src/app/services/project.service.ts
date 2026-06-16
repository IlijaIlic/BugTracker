import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AddProjectModel, ProjectModel } from "../models/project.model";

@Injectable({
    providedIn: 'root'
})
export class ProjectService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7236/api/Project'

    getAll(search?: string): Observable<ProjectModel[]> {
        const params = search ? `?search=${search}` : '';
        return this.http.get<ProjectModel[]>(`${this.apiUrl}${params}`)
    }

    getMine(): Observable<ProjectModel[]> {
        return this.http.get<ProjectModel[]>(`${this.apiUrl}/my`)
    }

    getById(id: number): Observable<ProjectModel> {
        return this.http.get<ProjectModel>(`${this.apiUrl}/` + id)
    }

    addProject(addProject: AddProjectModel): Observable<ProjectModel> {
        return this.http.post<ProjectModel>(`${this.apiUrl}`, addProject)
    }

    updateProject(data: AddProjectModel, id: number): Observable<ProjectModel> {
        return this.http.put<ProjectModel>(`${this.apiUrl}/` + id, data)
    }

    deleteProject(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/` + id)
    }
}