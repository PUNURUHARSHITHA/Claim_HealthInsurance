import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PolicyTypeService {
  private baseUrl = 'http://localhost:5030/api/PolicyType';

  constructor(private http: HttpClient) {}

  // ✅ Centralized header builder with token
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
      Accept: 'application/json'
    });
  }

  // ✅ Fetch all policy types
  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl, {
      headers: this.getHeaders()
    });
  }

  // ✅ Fetch a single policy type by ID
  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  // ✅ Create a new policy type
  create(payload: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, payload, {
      headers: this.getHeaders()
    });
  }

  // ✅ Update an existing policy type
  update(id: number, payload: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, payload, {
      headers: this.getHeaders()
    });
  }

  // ✅ Delete a policy type
  delete(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }
}
