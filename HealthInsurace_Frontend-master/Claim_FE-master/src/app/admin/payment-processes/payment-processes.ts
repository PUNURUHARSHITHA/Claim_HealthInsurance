import { Component, inject, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PaymentProcessesService } from '../../services/payment-processes.service';
import { AuthService } from '../../services/auth.service';
 
@Component({
  selector: 'app-payment-processes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './payment-processes.html',
  styleUrls: ['./payment-processes.css']
})
export class PaymentProcessesComponent implements OnInit {
  private fb = inject(FormBuilder);
  private service = inject(PaymentProcessesService);
  private auth = inject(AuthService);
  private cdRef = inject(ChangeDetectorRef);
 
  payoutForm = this.fb.group({
    claimID: [null, [Validators.required, Validators.min(1)]],
    payoutID: [null, [Validators.required, Validators.min(1)]]
  });
 
  result = '';
  error = '';
  role = this.auth.getUserRole();
 
  allPayouts: any[] = [];
  showPayouts = false;
  level2Approved = false;
 
  ngOnInit(): void {
    // Reset approval flag when payoutID changes
    this.payoutForm.get('payoutID')?.valueChanges.subscribe(() => {
      this.level2Approved = false;
    });
  }
 
  calculate(): void {
    this.resetMessages();
    this.level2Approved = false;
    this.cdRef.detectChanges();
 
    const claimID = this.payoutForm.get('claimID')?.value;
    if (typeof claimID === 'number') {
      this.service.calculatePayout(claimID).subscribe({
        next: res => {
          if (res && typeof res === 'object' && 'amount' in res) {
            this.result = `✅ Payout calculated: ₹${res.amount}`;
          } else {
            this.result = `✅ ${JSON.stringify(res)}`;
          }
          this.cdRef.detectChanges();
        },
        error: err => {
          this.error = err.error?.message || '❌ Calculation failed.';
          this.cdRef.detectChanges();
        }
      });
    } else {
      this.error = '❌ Claim ID must be a valid number.';
      this.cdRef.detectChanges();
    }
  }
 
  approveLevel2(): void {
    this.resetMessages();
    this.cdRef.detectChanges();
 
    const payoutID = this.payoutForm.get('payoutID')?.value;
    if (typeof payoutID === 'number') {
      this.service.approveLevel2(payoutID).subscribe({
        next: () => {
          this.level2Approved = true;
          this.result = `✅ Level 2 approved successfully by Admin`;
          this.cdRef.detectChanges();
        },
        error: err => {
          this.level2Approved = false;
          this.error = err.error?.message || '❌ Approval failed.';
          this.cdRef.detectChanges();
        }
      });
    } else {
      this.level2Approved = false;
      this.error = '❌ Payout ID must be a valid number.';
      this.cdRef.detectChanges();
    }
  }
 
  transfer(): void {
    this.resetMessages();
    this.cdRef.detectChanges();
 
    if (!this.level2Approved) {
      this.error = '❌ Transfer not allowed. Please approve Level 2 first.';
      this.cdRef.detectChanges();
      return;
    }
 
    const payoutID = this.payoutForm.get('payoutID')?.value;
    if (typeof payoutID === 'number') {
      this.service.markAsTransferred(payoutID).subscribe({
        next: res => {
          this.result = typeof res === 'string'
            ? `✅ ${res}`
            : `✅ ${res.message || JSON.stringify(res)}`;
          this.cdRef.detectChanges();
        },
        error: err => {
          this.error = err.error?.message || '❌ Transfer failed.';
          this.cdRef.detectChanges();
        }
      });
    } else {
      this.error = '❌ Payout ID must be a valid number.';
      this.cdRef.detectChanges();
    }
  }
 
  loadAllPayouts(): void {
    this.resetMessages();
    this.level2Approved = false;
    this.cdRef.detectChanges();
 
    this.service.getAllPayouts().subscribe({
      next: res => {
        this.allPayouts = res || [];
        this.showPayouts = true;
        this.cdRef.detectChanges();
      },
      error: err => {
        this.error = err.error?.message || '❌ Failed to load payouts.';
        this.cdRef.detectChanges();
      }
    });
  }
 
  resetMessages(): void {
    this.result = '';
    this.error = '';
    this.showPayouts = false;
    this.cdRef.detectChanges();
  }
}