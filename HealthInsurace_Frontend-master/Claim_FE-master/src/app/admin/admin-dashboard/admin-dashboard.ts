import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDashboardService } from '../../services/admin-dashboard.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.css']
})
export class AdminDashboardComponent implements OnInit {
  private dashboardService = inject(AdminDashboardService);
  private auth = inject(AuthService);
  private cdRef = inject(ChangeDetectorRef);

  statusCounts: { [key: string]: number } = {};
  statusKeys: string[] = [];
  selectedStatus: string = '';
  filteredClaims: any[] = [];
  error = '';
  loading = false;

  ngOnInit(): void {
    const role = this.auth.getUserRole();
    if (role !== 'Admin') {
      this.error = 'Access denied. Admins only.';
      this.cdRef.detectChanges();
      return;
    }

    this.loadStatusCounts();
  }

  loadStatusCounts(): void {
    this.dashboardService.getStatusCounts().subscribe({
      next: res => {
        this.statusCounts = res;
        this.statusKeys = Object.keys(res);
        this.cdRef.detectChanges();
      },
      error: err => {
        this.error = 'Failed to load status counts.';
        this.cdRef.detectChanges();
      }
    });
  }

  filterByStatus(status: string): void {
    this.selectedStatus = status;
    this.filteredClaims = [];
    this.loading = true;
    this.cdRef.detectChanges();

    this.dashboardService.getClaimsByStatus(status).subscribe({
      next: res => {
        this.filteredClaims = res;
        this.loading = false;
        this.cdRef.detectChanges();
      },
      error: err => {
        this.error = `Failed to load ${status} claims.`;
        this.loading = false;
        this.cdRef.detectChanges();
      }
    });
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return '🕒';
      case 'approved': return '✅';
      case 'rejected': return '❌';
      case 'underreview': return '🔍';
      default: return '📄';
    }
  }
}
