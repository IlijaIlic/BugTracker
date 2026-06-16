import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { UserProfileModel } from "../models/user.model";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7236/api/User'

    getMe() : Observable<UserProfileModel>{
        return this.http.get<UserProfileModel>(`${this.apiUrl}/me`)
    }

    
    updateMe(data: any){
        return this.http.put(`${this.apiUrl}/me`, data)
    }
    
    deleteMe(){
        return this.http.delete(`${this.apiUrl}/me`)
    }
}