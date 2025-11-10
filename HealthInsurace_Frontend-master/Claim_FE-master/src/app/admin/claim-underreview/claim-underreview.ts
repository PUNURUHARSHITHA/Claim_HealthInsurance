import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClaimUnderReviewService } from '../../services/claim-underreview.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-claim-underreview',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './claim-underreview.html',
  styleUrls: ['./claim-underreview.css']
})
export class ClaimUnderReviewComponent implements OnInit {
  claimID: number | null = null;
  message = '';
  error = '';
  isAdmin = false;

  submittedClaims: { claimID: number; treatmentDetails: string; status: string }[] = [];
  showSubmitted = false;

  private auth = inject(AuthService);
  private service = inject(ClaimUnderReviewService);
  private cdRef = inject(ChangeDetectorRef);

  ngOnInit(): void {
    this.isAdmin = this.auth.getUserRole()?.toLowerCase() === 'admin';
    this.cdRef.detectChanges();
  }

  submit(): void {
    this.message = '';
    this.error = '';
    this.cdRef.detectChanges();

    if (!this.claimID || this.claimID <= 0) {
      this.error = 'Please enter a valid Claim ID.';
      this.cdRef.detectChanges();
      return;
    }

    // ✅ Prevent duplicate UnderReview submission
    const alreadyUnderReview = this.submittedClaims.some(
      c => c.claimID === this.claimID && c.status?.toLowerCase() === 'underreview'
    );
    if (alreadyUnderReview) {
      this.error = 'Claim has already been moved to UnderReview.';
      this.cdRef.detectChanges();
      return;
    }

    this.service.updateClaimToUnderReview(this.claimID).subscribe({
      next: (res: string) => {
        const lower = res.toLowerCase();

        // ✅ Prioritize specific matches first
        if (lower.includes('already in underreview')) {
          this.error = 'Claim has already been moved to UnderReview.';
        } else if (lower.includes('already approved')) {
          this.error = 'Claim is already approved.';
        } else if (lower.includes('already rejected')) {
          this.error = 'Claim is already rejected.';
        } else if (lower.includes('not found')) {
          this.error = 'Claim ID not found.';
        } else if (lower.includes('invalid transition')) {
          this.error = 'Claim cannot be moved to UnderReview. Invalid status.';
        } else if (lower.includes('underreview')) {
          this.message = '✅ Claim successfully moved to UnderReview.';
        } else {
          this.message = res;
        }

        this.cdRef.detectChanges();
      },
      error: (err: any) => {
        const msg = err.message?.toLowerCase() || '';

        // ✅ Same prioritization in error block
        if (msg.includes('already in underreview')) {
          this.error = 'Claim has already been moved to UnderReview.';
        } else if (msg.includes('already approved')) {
          this.error = 'Claim is already approved.';
        } else if (msg.includes('already rejected')) {
          this.error = 'Claim is already rejected.';
        } else if (msg.includes('not found')) {
          this.error = 'Claim ID not found.';
        } else if (msg.includes('invalid transition')) {
          this.error = 'Claim cannot be moved to UnderReview. Invalid status.';
        } else {
          this.error = err.message || 'An error occurred.';
        }

        this.cdRef.detectChanges();
      }
    });
  }

  toggleSubmittedClaims(): void {
    this.message = '';
    this.error = '';
    this.showSubmitted = false;
    this.submittedClaims = [];
    this.cdRef.detectChanges();

    this.service.getAllClaims().subscribe({
      next: (claims) => {
        this.submittedClaims = claims
          .filter(c => {
            const status = c.status?.toLowerCase();
            return status === 'submitted' || status === 'underreview';
          })
          .map(c => ({
            claimID: c.claimID,
            treatmentDetails: c.treatmentDetails,
            status: c.status
          }));

        this.showSubmitted = true;
        if (this.submittedClaims.length === 0) {
          this.error = 'No submitted claims found.';
        }

        this.cdRef.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load submitted claims.';
        this.cdRef.detectChanges();
      }
    });
  }
}
