import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
 
export interface ClaimsReportDto {
  reportID: number;
  claimID: number;
  policyholderID: number;
  type: string;
  generatedDate: string;
  insights: string;
}
 
export interface GenerateReportRequestDto {
  claimId: number;
}
 
@Injectable({ providedIn: 'root' })
export class ReportsService {
  private baseUrl = 'http://localhost:5030/api/claimsreport';
 
  constructor(private http: HttpClient) {}
 
  // ✅ Get all reports
  getAllReports(): Observable<ClaimsReportDto[]> {
    return this.http.get<ClaimsReportDto[]>(this.baseUrl);
  }
 
  // ✅ Generate report for a claim
  generateReport(dto: GenerateReportRequestDto): Observable<string> {
    return this.http.post(this.baseUrl + '/Auto-generateReport', dto, {
      responseType: 'text'
    });
  }
 
  // ✅ Filter by type
  getReportsByType(type: string): Observable<ClaimsReportDto[]> {
    return this.http.get<ClaimsReportDto[]>(`${this.baseUrl}/type/${type}`);
  }
 
  // ✅ Filter by date range
  getReportsByDateRange(from: string, to: string): Observable<ClaimsReportDto[]> {
    return this.http.get<ClaimsReportDto[]>(`${this.baseUrl}/range?from=${from}&to=${to}`);
  }
 
  // ✅ Get report by ID
  getReportById(reportId: number): Observable<ClaimsReportDto> {
    return this.http.get<ClaimsReportDto>(`${this.baseUrl}/${reportId}`);
  }
 
  // ✅ Download report as PDF
  downloadReportById(reportId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/download/${reportId}`, {
      responseType: 'blob'
    });
  }
}