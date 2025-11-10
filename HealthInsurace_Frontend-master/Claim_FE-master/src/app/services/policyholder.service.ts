import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
 
@Injectable({ providedIn: 'root' })
export class PolicyholderService {
  private baseUrl = 'http://localhost:5030/api';
 
  constructor(private http: HttpClient, private auth: AuthService) {}
 
  getAgents(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Agent`, {
      headers: this.auth.getAuthHeaders()
    });
  }
 
  getPolicyTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/PolicyType`, {
      headers: this.auth.getAuthHeaders()
    });
  }
 
  create(dto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Policyholder`, dto, {
      headers: this.auth.getAuthHeaders()
    });
  }
 
  addDependents(policyholderID: number, dependents: any): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/Policyholder/${policyholderID}/dependents`,
      dependents,
      { headers: this.auth.getAuthHeaders() }
    );
  }
}
 
 