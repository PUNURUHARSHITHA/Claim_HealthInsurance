import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Approval1Service } from '../../services/approval-1.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-approval-1',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './approval-1.html',
  styleUrls: ['./approval-1.css']
})
export class Approval1 {
  approvalForm: FormGroup;
  message: string | null = null;
  error: string | null = null;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private approvalService: Approval1Service,
    private auth: AuthService
  ) {
    this.approvalForm = this.fb.group({
      payoutId: ['', [Validators.required, Validators.pattern(/^\d+$/)]]
    });
  }

  approve(): void {
    this.message = null;
    this.error = null;

    const payoutId = Number(this.approvalForm.value.payoutId);
    if (!payoutId || payoutId < 1) {
      this.error = 'Please enter a valid numeric Payout ID.';
      return;
    }

    this.loading = true;

    this.approvalService.approveLevel1(payoutId).subscribe({
      next: (res: any) => {
        this.message = res.message || `Payout ${payoutId} approved successfully.`;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = this.extractErrorMessage(err);
        this.loading = false;
      }
    });
  }

  private extractErrorMessage(err: any): string {
    if (err.status === 403) return '❌ Only Pratheek is allowed to approve Level 1 payouts.';
    if (err.status === 404) return `❌ ${err.error}`;
    if (err.status === 400) return `❌ ${err.error}`;
    return '❌ Something went wrong. Please try again.';
  }
}