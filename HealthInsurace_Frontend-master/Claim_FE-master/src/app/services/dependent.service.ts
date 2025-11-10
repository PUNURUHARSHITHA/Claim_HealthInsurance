import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DependentDto {
  dependentID?: number;
  policyholderID: number;
  name: string;
  relationship: string;
}

@Injectable({
  providedIn: 'root'
})
export class DependentService {
  private baseUrl = '/api/policyholder'; // Adjust if your API route differs

  constructor(private http: HttpClient) {}

  addDependent(policyholderID: number, dto: DependentDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/${policyholderID}/dependents`, dto);
  }
}
