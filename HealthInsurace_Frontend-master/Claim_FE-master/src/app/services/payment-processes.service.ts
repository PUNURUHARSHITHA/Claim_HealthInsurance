import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';
 
@Injectable({ providedIn: 'root' })
export class PaymentProcessesService {
  private baseUrl = 'http://localhost:5030/api/Payout';
 
  constructor(private http: HttpClient, private auth: AuthService) {}
 
  calculatePayout(claimID: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/calculate`, { claimID }, {
      headers: this.auth.getAuthHeaders()
    });
  }
 
  approveLevel2(payoutId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/${payoutId}/Admin_Approve-level2`, {}, {
      headers: this.auth.getAuthHeaders()
    });
  }
 
  markAsTransferred(payoutId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/${payoutId}/transfer`, {}, {
      headers: this.auth.getAuthHeaders()
    });
  }
 
  // ✅ New: Get all payouts
  getAllPayouts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}`, {
      headers: this.auth.getAuthHeaders()
    });
  }
}
 
 