import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { PolicyholderService } from '../../services/policyholder.service';
 
@Component({
  selector: 'app-policyholder',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, RouterModule],
  templateUrl: './policyholder-form.html',
  styleUrls: ['./policyholder-form.css']
})
export class PolicyholderComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private auth = inject(AuthService);
  private service = inject(PolicyholderService);
 
  agents: any[] = [];
  policyTypes: any[] = [];
  selectedPolicyType: any = null;
 
  productTypes = [
    'Individual Health', 'Family Floater', 'Senior Citizen', 'Maternity Cover',
    'Critical Illness', 'Top-Up Plan', 'Group Health', 'Personal Accident'
  ];
  regions = ['North', 'South', 'East', 'West'];
 
  error = '';
  submitting = false;
 
  form = this.fb.group({
    policyTypeID: [null, Validators.required],
    agentID: [null, Validators.required],
    region: ['', Validators.required],
    productType: ['', Validators.required],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
    bankReferenceNumber: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{4}[0-9]{8}$/)]],
    policyStartDate: ['', Validators.required]
  });
 
  ngOnInit(): void {
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
 
    this.loadDropdowns();
  }
 
  loadDropdowns(): void {
    this.service.getAgents().subscribe({
      next: res => this.agents = res || [],
      error: err => console.error('Failed to load agents', err)
    });
 
    this.service.getPolicyTypes().subscribe({
      next: res => this.policyTypes = res || [],
      error: err => console.error('Failed to load policy types', err)
    });
  }
 
  onPolicyTypeChange(): void {
    const selectedId = this.form.get('policyTypeID')?.value;
    this.selectedPolicyType = this.policyTypes.find(p => p.policyTypeID === selectedId);
  }
 
  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = '❌ Please fill all required fields correctly.';
      return;
    }
 
    this.submitting = true;
 
    const {
      policyTypeID,
      agentID,
      region,
      productType,
      phoneNumber,
      bankReferenceNumber,
      policyStartDate
    } = this.form.getRawValue();
 
    const dto = {
      name: this.auth.getUserName()?.replace(/[^a-zA-Z\s]/g, '').trim() || '',
      policyTypeID: Number(policyTypeID),
      agentID: Number(agentID),
      region: region?.trim(),
      productType: productType?.trim(),
      phoneNumber: phoneNumber?.trim(),
      bankReferenceNumber: bankReferenceNumber?.trim(),
      policyStartDate: policyStartDate ? new Date(policyStartDate).toISOString() : '',
      coverageDetails: ''
    };
 
    this.service.create(dto).subscribe({
      next: () => {
        this.router.navigate(['/policyholder/claims']);

      },
      error: err => {
        const backendErrors = err.error;
        console.error('Backend validation errors:', backendErrors);
        if (Array.isArray(backendErrors)) {
          this.error = backendErrors
            .map(e => `${e.field ?? 'Unknown'}: ${e.error ?? 'Invalid'}`)
            .join(', ');
        } else if (typeof backendErrors === 'object') {
          this.error = Object.entries(backendErrors)
            .map(([key, val]) => `${key}: ${Array.isArray(val) ? val.join(', ') : val}`)
            .join(', ');
        } else {
          this.error = backendErrors?.message || '❌ Failed to submit form.';
        }
      },
      complete: () => {
        this.submitting = false;
      }
    });
  }
}
 
 