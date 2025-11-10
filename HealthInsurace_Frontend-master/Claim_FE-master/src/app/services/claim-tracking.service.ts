import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service'; // ✅ Inject AuthService

export interface ClaimActionLogDto {
  logID: number;
  claimID: number;
  action: string;
  performedBy: string;
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class ClaimTrackingService {
  private baseUrl = 'http://localhost:5030/api/claim';

  constructor(private http: HttpClient, private auth: AuthService) {}

  getActionLogs(claimId: number): Observable<ClaimActionLogDto[]> {
    const headers = this.auth.getAuthHeaders(); // ✅ Include Authorization
    return this.http.get<ClaimActionLogDto[]>(`${this.baseUrl}/${claimId}/actions`, { headers });
  }
}
