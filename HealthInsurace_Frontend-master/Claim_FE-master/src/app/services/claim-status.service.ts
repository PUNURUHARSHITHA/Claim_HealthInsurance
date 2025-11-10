import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ClaimStatusService {
  private baseUrl = 'http://localhost:5030/api/claim';

  constructor(private http: HttpClient, private auth: AuthService) {}

  approveClaim(claimID: number): Observable<string> {
    const headers = this.auth.getAuthHeaders();
    return this.http.put(`${this.baseUrl}/${claimID}/status`, null, {
      headers,
      responseType: 'text' as const
    }).pipe(
      catchError(err => {
        const message = typeof err.error === 'string' ? err.error : 'Approval failed.';
        return throwError(() => new Error(message));
      })
    );
  }
}
