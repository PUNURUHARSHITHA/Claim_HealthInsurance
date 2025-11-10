import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

export interface PolicyholderDto {
  policyholderID: number;
  name: string;
  region: string;
  productType: string;
}

@Injectable({ providedIn: 'root' })
export class PolicyholderDashboardService {
  private baseUrl = 'http://localhost:5030/api/policyholder';

  constructor(private http: HttpClient, private auth: AuthService) {}

  getLoggedInPolicyholder(): Observable<PolicyholderDto> {
    const headers = this.auth.getAuthHeaders();
    return this.http.get<PolicyholderDto>(`${this.baseUrl}/self`, { headers }).pipe(
      catchError(err => {
        console.error('Failed to fetch policyholder:', err);
        return throwError(() => new Error('Unable to fetch policyholder details.'));
      })
    );
  }
}
