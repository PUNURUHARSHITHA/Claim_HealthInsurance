import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EligibilityCheckService, EligibilityResult } from '../../services/eligibility-check.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-eligibility-check',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './eligibility-check.html',
  styleUrls: ['./eligibility-check.css']
})
export class EligibilityCheck implements OnInit {
  claimID: number | null = null;
  result: EligibilityResult | null = null;
  error: string = '';
  isAdmin: boolean = false;

  private auth = inject(AuthService);
  private service = inject(EligibilityCheckService);
  private cdRef = inject(ChangeDetectorRef);

  ngOnInit(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.isAdmin = role === 'admin';
    this.cdRef.detectChanges(); // Ensure role-based UI updates
  }

  runCheck(): void {
    if (!this.claimID || this.claimID <= 0) {
      this.error = 'Please enter a valid Claim ID.';
      this.result = null;
      this.cdRef.detectChanges();
      return;
    }

    this.error = '';
    this.result = null;
    this.cdRef.detectChanges();

    this.service.runEligibilityCheck(this.claimID).subscribe({
      next: (res) => {
        this.result = res;
        this.cdRef.detectChanges();
      },
      error: (err) => {
        this.error = err.message;
        this.cdRef.detectChanges();
      }
    });
  }
}
