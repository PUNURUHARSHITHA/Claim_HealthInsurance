import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
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
  isAdmin: boolean = false;

  private auth = inject(AuthService);
  private cdRef = inject(ChangeDetectorRef);

  constructor(private trackingService: ClaimTrackingService) {}

  ngOnInit(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.isAdmin = role === 'admin';
    this.cdRef.detectChanges(); // Ensure UI reflects role immediately
  }

  loadLogs(): void {
    this.errorMessage = '';
    this.logs = [];
    this.loading = true;
    this.cdRef.detectChanges();

    if (!this.claimId || this.claimId <= 0) {
      this.errorMessage = 'Please enter a valid Claim ID.';
      this.loading = false;
      this.cdRef.detectChanges();
      return;
    }

    this.trackingService.getActionLogs(this.claimId).subscribe({
      next: (data) => {
        this.logs = data;
        this.loading = false;
        if (data.length === 0) {
          this.errorMessage = 'No logs found for this claim.';
        }
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load logs:', err);
        this.errorMessage = 'Error fetching logs.';
        this.loading = false;
        this.cdRef.detectChanges();
      }
    });
  }
}
