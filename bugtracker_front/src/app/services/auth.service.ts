import { HttpClient } from "@angular/common/http";
import { inject, Injectable, signal } from "@angular/core";
import { AuthResponseDto } from "../models/auth.model";
import { Observable, tap } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7236/api/Auth'

    currentUser = signal<AuthResponseDto | null>(this.getUserFromStorage());

    protected getUserFromStorage(): AuthResponseDto | null {
        const data = localStorage.getItem('user_auth');
        return data ? JSON.parse(data) : null;
    }

    register(registerDto: any): Observable<AuthResponseDto> {
        return this.http.post<AuthResponseDto>(`${this.apiUrl}/register`, registerDto).pipe(
            tap(response => {
                localStorage.setItem('user_auth', JSON.stringify(response));
                this.currentUser.set(response);
            })
        )
    }

    login(loginDto: any):Observable<AuthResponseDto> {
        return this.http.post<AuthResponseDto>(`${this.apiUrl}/login`, loginDto).pipe(
            tap(response => {
                localStorage.setItem('user_auth', JSON.stringify(response));
                this.currentUser.set(response);
            })
        )
    }

    logout() {
        localStorage.removeItem('user_auth');
        this.currentUser.set(null);
    }

    getToken(): string | null {
        return this.currentUser()?.token || null;
    }
}