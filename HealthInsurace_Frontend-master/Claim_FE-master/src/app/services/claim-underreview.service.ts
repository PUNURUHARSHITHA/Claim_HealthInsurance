import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ClaimUnderReviewService {
  private baseUrl = 'http://localhost:5030/api/claim';

  constructor(private http: HttpClient, private auth: AuthService) {}

  getClaimById(claimID: number): Observable<any> {
    const headers = this.auth.getAuthHeaders();
    return this.http.get(`${this.baseUrl}/${claimID}`, { headers });
  }

  updateClaimToUnderReview(claimID: number): Observable<string> {
    const headers = this.auth.getAuthHeaders();
    const dto = {
      updatedBy: this.auth.getUserName() ?? 'Unknown',
      status: 'UnderReview'
    };

    return this.http.put(`${this.baseUrl}/${claimID}/Review`, dto, {
      headers,
      responseType: 'text' as const
    }).pipe(
      catchError(err => {
        const message = typeof err.error === 'string' ? err.error : 'Status update failed.';
        return throwError(() => new Error(message));
      })
    );
  }

  getAllClaims(): Observable<any[]> {
    const headers = this.auth.getAuthHeaders();
    return this.http.get<any[]>(`${this.baseUrl}`, { headers }).pipe(
      catchError(err => {
        console.error('Failed to fetch claims:', err);
        return throwError(() => new Error('Unable to load claims.'));
      })
    );
  }
}
