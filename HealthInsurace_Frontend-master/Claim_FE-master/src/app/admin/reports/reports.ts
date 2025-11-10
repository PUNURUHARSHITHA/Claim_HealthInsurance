import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { Chart } from 'chart.js/auto';
import { AuthService } from '../../services/auth.service';

interface ClaimsReportDto {
  reportID: number;
  claimID: number;
  policyholderID: number;
  type: string;
  generatedDate: string;
  insights: string;
}

interface GenerateReportRequestDto {
  claimId: number;
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './reports.html',
  styleUrls: ['./reports.css']
})
export class Reports implements OnInit {
  private auth = inject(AuthService);
  private http = inject(HttpClient);
  private cdRef = inject(ChangeDetectorRef);

  isAdmin = false;

  reports: ClaimsReportDto[] = [];
  pagedReports: ClaimsReportDto[] = [];
  pageSize = 5;
  currentPage = 1;
  totalPages = 0;
  claimId = 0;
  selectedType = '';
  availableTypes: string[] = [];
  fromDate = '';
  toDate = '';
  chart: Chart | null = null;

  ngOnInit(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.isAdmin = role === 'admin';
    this.cdRef.detectChanges();

    if (this.isAdmin) {
      this.loadReports();
    }
  }

  loadReports(): void {
    this.http.get<ClaimsReportDto[]>('http://localhost:5030/api/claimsreport').subscribe({
      next: (data) => {
        this.reports = data;
        this.extractTypes(data);
        this.totalPages = Math.ceil(this.reports.length / this.pageSize);
        this.setPage(1);
        this.renderChart();
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load reports:', err);
        this.cdRef.detectChanges();
      }
    });
  }

  extractTypes(data: ClaimsReportDto[]): void {
    const types = new Set<string>();
    data.forEach(r => types.add(r.type));
    this.availableTypes = Array.from(types);
    this.cdRef.detectChanges();
  }

  setPage(page: number): void {
    this.currentPage = page;
    const start = (page - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.pagedReports = this.reports.slice(start, end);
    this.cdRef.detectChanges();
  }

  generateReport(): void {
    if (this.claimId <= 0) return;
    const dto: GenerateReportRequestDto = { claimId: this.claimId };

    this.http.post('http://localhost:5030/api/claimsreport/Auto-generateReport', dto, { responseType: 'text' }).subscribe({
      next: () => {
        this.loadReports();
        this.claimId = 0;
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Report generation failed:', err);
        this.cdRef.detectChanges();
      }
    });
  }

  filterByType(): void {
    if (!this.selectedType) {
      this.loadReports();
      return;
    }

    this.http.get<ClaimsReportDto[]>(`http://localhost:5030/api/claimsreport/type/${this.selectedType}`).subscribe({
      next: (data) => {
        this.reports = data;
        this.totalPages = Math.ceil(this.reports.length / this.pageSize);
        this.setPage(1);
        this.renderChart();
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Failed to filter by type:', err);
        this.cdRef.detectChanges();
      }
    });
  }

  filterByDate(): void {
    if (!this.fromDate || !this.toDate) return;

    this.http.get<ClaimsReportDto[]>(`http://localhost:5030/api/claimsreport/range?from=${this.fromDate}&to=${this.toDate}`).subscribe({
      next: (data) => {
        this.reports = data;
        this.totalPages = Math.ceil(this.reports.length / this.pageSize);
        this.setPage(1);
        this.renderChart();
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Failed to filter by date:', err);
        this.cdRef.detectChanges();
      }
    });
  }

  downloadReport(reportId: number): void {
    this.http.get(`http://localhost:5030/api/claimsreport/download/${reportId}`, {
      responseType: 'blob'
    }).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `ClaimReport_${reportId}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
        alert(`✅ Report ${reportId} downloaded successfully.`);
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Download failed:', err);
        alert(`❌ Failed to download report ${reportId}.`);
        this.cdRef.detectChanges();
      }
    });
  }

  renderChart(): void {
    const typeCounts: { [key: string]: number } = {};
    this.reports.forEach(r => {
      typeCounts[r.type] = (typeCounts[r.type] || 0) + 1;
    });

    const labels = Object.keys(typeCounts);
    const data = Object.values(typeCounts);

    if (this.chart) this.chart.destroy();

    this.chart = new Chart('reportChart', {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Report Count',
          data,
          backgroundColor: '#0078d4',
          borderRadius: 4,
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            display: true,
            position: 'bottom',
            labels: {
              color: '#333',
              font: {
                size: 12,
                weight: 'bold'
              }
            }
          },
          title: {
            display: true,
            text: 'Claims Reports by Type',
            font: {
              size: 16
            },
            color: '#0078d4'
          }
        },
        scales: {
          x: {
            ticks: {
              color: '#555',
              font: {
                size: 12
              }
            },
            grid: {
              display: false
            }
          },
          y: {
            beginAtZero: true,
            ticks: {
              stepSize: 1,
              color: '#555',
              font: {
                size: 12
              }
            },
            grid: {
              color: '#eee'
            }
          }
        }
      }
    });

    this.cdRef.detectChanges();
  }
}
