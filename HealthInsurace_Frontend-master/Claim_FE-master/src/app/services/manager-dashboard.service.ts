import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';
 
export interface PayoutDto {
  payoutID: number;
  claimID: number;
  amount: number;
  approvalStatus: string;
  payoutStatus: string;
  approvedByLevel1: string;
  approvedByLevel2: string;
  level1ApprovalDate: string;
  level2ApprovalDate: string;
  transferDate: string;
  bankReferenceNumber: string;
  transferStatus: string;
}
 
@Injectable({ providedIn: 'root' })
export class ManagerDashboardService {
  private baseUrl = 'http://localhost:5030/api/Payout';
 
  constructor(private http: HttpClient, private auth: AuthService) {}
 
  getAllPayouts(): Observable<PayoutDto[]> {
    return this.http.get<PayoutDto[]>(`${this.baseUrl}`, {
      headers: this.auth.getAuthHeaders()
    });
  }
}
 
 