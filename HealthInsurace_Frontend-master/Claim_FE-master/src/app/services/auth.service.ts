import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5030/api/auth';

  constructor(private http: HttpClient) {}

  // 🔐 Login
  login(payload: { username: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, payload);
  }

  // 🔓 Logout
  logoutFromServer(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken || refreshToken.trim() === '') {
      return of({ message: 'No refresh token found.' });
    }

    return this.http.post(`${this.apiUrl}/logout`, { refreshToken }, {
      headers: this.getAuthHeaders()
    });
  }

  // 💾 Store tokens
  storeTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem('token', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  // 🔍 Get tokens
  getAccessToken(): string | null {
    return localStorage.getItem('token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  // 🧹 Clear tokens
  clearTokens(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
  }

  // ✅ Check login status
  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  // 🔓 Decode JWT
  getDecodedToken(): any {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = token.split('.')[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }

  // 🧑‍💼 Get user role
  getUserRole(): string | null {
    const decoded = this.getDecodedToken();
    if (!decoded) return null;

    const roleKeys = Object.keys(decoded).filter(k => k.toLowerCase().includes('role'));
    for (const key of roleKeys) {
      const value = decoded[key];
      if (typeof value === 'string') return value;
      if (Array.isArray(value)) return value[0];
    }

    return null;
  }

  // 🧑 Get user name
  getUserName(): string | null {
    const decoded = this.getDecodedToken();
    if (!decoded) return null;

    const nameKeys = Object.keys(decoded).filter(k => k.toLowerCase().includes('name'));
    for (const key of nameKeys) {
      const value = decoded[key];
      if (typeof value === 'string') return value;
    }

    return null;
  }

  // 🆔 Get Policyholder ID
  getPolicyholderID(): number | null {
    const decoded = this.getDecodedToken();
    if (!decoded) return null;

    const idKeys = Object.keys(decoded).filter(k => k.toLowerCase().includes('policyholderid'));
    for (const key of idKeys) {
      const value = decoded[key];
      if (typeof value === 'number') return value;
      if (typeof value === 'string' && /^\d+$/.test(value)) return parseInt(value);
    }

    return null;
  }

  // 🔐 Auth headers for API calls
  getAuthHeaders(): HttpHeaders {
    const token = this.getAccessToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }
}
