import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ClaimService {
  private baseUrl = 'http://localhost:5030/api';

  constructor(private http: HttpClient, private auth: AuthService) {}

  submitClaim(formData: FormData): Observable<any> {
    const headers = this.auth.getAuthHeaders();
    return this.http.post(`${this.baseUrl}/Claim/submit`, formData, { headers });
  }

  getHospitalIDs(): Observable<number[]> {
    return this.http.get<number[]>(`${this.baseUrl}/Hospital`);
  }

  getTreatmentIDs(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${this.baseUrl}/TreatmentHospital`);
  }
}
