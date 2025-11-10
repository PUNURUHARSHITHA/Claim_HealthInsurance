import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {
  private baseUrl = 'http://localhost:5030/api/Claim';

  constructor(private http: HttpClient, private auth: AuthService) {}

  getStatusCounts(): Observable<{ [key: string]: number }> {
    return this.http.get<{ [key: string]: number }>(`${this.baseUrl}/status-counts`, {
      headers: this.auth.getAuthHeaders()
    });
  }

  getClaimsByStatus(status: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Filtered claims?status=${status}`, {
      headers: this.auth.getAuthHeaders()
    });
  }
}
