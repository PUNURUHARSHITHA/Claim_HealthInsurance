import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

export interface EligibilityResult {
  claimID: number;
  result: string;
  rulesApplied: string[];
}

@Injectable({ providedIn: 'root' })
export class EligibilityCheckService {
  private baseUrl = 'http://localhost:5030/api/EligibilityCheck';

  constructor(private http: HttpClient, private auth: AuthService) {}

  runEligibilityCheck(claimID: number): Observable<EligibilityResult> {
    const headers = this.auth.getAuthHeaders();
    const dto = { claimID };

    return this.http.post<EligibilityResult>(`${this.baseUrl}/check`, dto, { headers }).pipe(
      catchError(err => {
        const message = typeof err.error === 'string' ? err.error : 'Eligibility check failed.';
        return throwError(() => new Error(message));
      })
    );
  }
}
