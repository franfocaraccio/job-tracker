import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface User {
  id: string;
  name: string;
  email: string;
}

interface LoginResponse {
  accessToken: string;
}

const tokenKey = 'accessToken';

/**
 * Calls the users API and keeps the JWT of the signed-in user in localStorage.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  register(name: string, email: string, password: string): Observable<User> {
    return this.http.post<User>(`${environment.apiUrl}/auth/register`, { name, email, password });
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(
      tap((response) => {
        localStorage.setItem(tokenKey, response.accessToken);
      }),
    );
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${environment.apiUrl}/users/me`);
  }

  logout(): void {
    localStorage.removeItem(tokenKey);
  }

  getToken(): string | null {
    return localStorage.getItem(tokenKey);
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }
}
