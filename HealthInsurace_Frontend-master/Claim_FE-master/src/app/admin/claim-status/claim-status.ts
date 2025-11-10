import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClaimStatusService } from '../../services/claim-status.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-claim-status',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './claim-status.html',
  styleUrls: ['./claim-status.css']
})
export class ClaimStatus implements OnInit {
  claimID: number | null = null;
  message: string = '';
  error: string = '';
  isAdmin: boolean = false;

  private auth = inject(AuthService);
  private service = inject(ClaimStatusService);
  private cdRef = inject(ChangeDetectorRef);

  ngOnInit(): void {
    this.isAdmin = this.auth.getUserRole()?.toLowerCase() === 'admin';
    this.cdRef.detectChanges(); // Ensure UI reflects role immediately
  }

  approve(): void {
    if (!this.claimID || this.claimID <= 0) {
      this.error = 'Please enter a valid Claim ID.';
      this.message = '';
      this.cdRef.detectChanges();
      return;
    }

    this.message = '';
    this.error = '';
    this.cdRef.detectChanges();

    this.service.approveClaim(this.claimID).subscribe({
      next: (res) => {
        const lower = res.toLowerCase();
        if (lower.includes('rejected')) {
          this.error = res;
          this.message = '';
        } else {
          this.message = res;
          this.error = '';
        }
        this.cdRef.detectChanges();
      },
      error: (err) => {
        this.error = err.message || 'Approval failed.';
        this.message = '';
        this.cdRef.detectChanges();
      }
    });
  }
}
