import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { BugModel } from "../models/bug.model";
import { Observable } from "rxjs";
import { Bugs } from "../pages/bugs/bugs";

@Injectable({
    providedIn: 'root'
})
export class BugService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7236/api/Bug'

    addBug(data: any) {
        return this.http.post<BugModel[]>(`${this.apiUrl}`, data)
    }

    getBugsByProject(id: number, search?: string): Observable<BugModel[]> {
        let params: any = {};
        if (search && search.trim() !== '') params['search'] = search;

        return this.http.get<BugModel[]>(`${this.apiUrl}/project/${id}`, { params })
    }

    getMyBugs(id: number): Observable<BugModel[]> {
        return this.http.get<BugModel[]>(`${this.apiUrl}/project/my/` + id)
    }

    updateBug(data: any, id: number): Observable<BugModel> {
        return this.http.put<BugModel>(`${this.apiUrl}/` + id, data)
    }

    deleteBug(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/` + id)
    }
}