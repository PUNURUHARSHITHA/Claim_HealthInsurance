import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface PolicyholderDto {
  policyholderID: number;
  name: string;
  policyTypeID: number;
  coverageDetails: string;
  agentID: number;
  region: string;
  productType: string;
  phoneNumber?: string;
  bankReferenceNumber: string;
  policyStartDate: string;
  policyEndDate: string;
}

@Injectable({ providedIn: 'root' })
export class PolicyholderListService {
  private baseUrl = 'http://localhost:5030/api/Policyholder';

  constructor(private http: HttpClient, private auth: AuthService) {}

  getAllPolicyholders(): Observable<PolicyholderDto[]> {
    const headers = this.auth.getAuthHeaders();
    return this.http.get<PolicyholderDto[]>(this.baseUrl, { headers });
  }
}
