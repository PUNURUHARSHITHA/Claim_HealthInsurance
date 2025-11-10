import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ManagerDashboardService, PayoutDto } from '../../services/manager-dashboard.service';
import { AuthService } from '../../services/auth.service';
 
@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manager-dashboard.html',
  styleUrls: ['./manager-dashboard.css']
})
export class ManagerDashboardComponent implements OnInit {
  private dashboardService = inject(ManagerDashboardService);
  private auth = inject(AuthService);
 
  payouts: PayoutDto[] = [];
  error = '';
  role: string = '';
  username: string = '';
 
  ngOnInit(): void {
    this.role = this.auth.getUserRole() ?? 'Unknown';
    this.username = this.auth.getUserName() ?? 'Unknown';
 
    if (this.role !== 'Manager' && this.role !== 'Admin') {
      this.error = 'Access denied. Only Managers and Admins can view this dashboard.';
      return;
    }
 
    this.dashboardService.getAllPayouts().subscribe({
      next: (res: PayoutDto[]) => this.payouts = res,
      error: () => this.error = 'Failed to load payout data.'
    });
  }
}
 