import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class Approval1Service {
  private apiUrl = 'http://localhost:5030/api/payout';

  constructor(private http: HttpClient, private auth: AuthService) {}

  approveLevel1(payoutId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${payoutId}/Manager_Approve-level1`, null, {
      headers: this.auth.getAuthHeaders()
    });
  }
}