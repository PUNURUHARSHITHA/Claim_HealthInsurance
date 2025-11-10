import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { ClaimTrackingService, ClaimActionLogDto } from '../../services/claim-tracking.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-claim-tracking',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './claim-tracking.html',
  styleUrls: ['./claim-tracking.css']
})
export class ClaimTracking implements OnInit {
  claimId: number = 0;
  logs: ClaimActionLogDto[] = [];
  errorMessage: string = '';
  loading: boolean = false;
  hasAccess: boolean = false;

  private auth = inject(AuthService);

  constructor(private trackingService: ClaimTrackingService) {}

  ngOnInit(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.hasAccess = role === 'admin' || role === 'policyholder';
  }

  loadLogs(): void {
    this.errorMessage = '';
    this.logs = [];

    if (!this.hasAccess) {
      this.errorMessage = 'Access denied. You do not have permission to view claim logs.';
      return;
    }

    if (!this.claimId || this.claimId <= 0) {
      this.errorMessage = 'Please enter a valid Claim ID.';
      return;
    }

    this.loading = true;

    this.trackingService.getActionLogs(this.claimId).subscribe({
      next: (data) => {
        this.logs = data;
        this.loading = false;
        if (data.length === 0) {
          this.errorMessage = 'No logs found for this claim.';
        }
      },
      error: (err) => {
        console.error('Failed to load logs:', err);
        this.errorMessage = 'Error fetching logs.';
        this.loading = false;
      }
    });
  }
}
