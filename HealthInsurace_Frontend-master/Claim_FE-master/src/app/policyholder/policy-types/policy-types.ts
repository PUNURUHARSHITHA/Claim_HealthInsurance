import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { PolicyTypeService } from '../../services/policy-type.service';
import { AuthService } from '../../services/auth.service';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-policy-type',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, HttpClientModule],
  templateUrl: './policy-types.html',
  styleUrls: ['./policy-types.css']
})
export class PolicyTypeComponent implements OnInit {
  policyTypes: any[] = [];
  form!: FormGroup;
  message = '';
  error = '';
  isAdmin = false;
  isPolicyholder = false;
  editing = false;
  createMode = false;

  constructor(
    private service: PolicyTypeService,
    private fb: FormBuilder,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.detectRole();

    // ✅ Load data for both Admin and Policyholder
    if (this.isAdmin || this.isPolicyholder) {
      this.loadAll();
      this.cdr.detectChanges();
    }
  }

  private detectRole(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.isAdmin = role === 'admin';
    this.isPolicyholder = role === 'policyholder';
  }

  private buildForm(): void {
    this.form = this.fb.group({
      PolicyTypeID: [0],
      TypeName: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^[A-Za-z\s]+$/)]],
      Description: ['', [Validators.required, Validators.maxLength(300)]],
      CoverageLimit: [100000, [Validators.required, Validators.min(0.01)]],
      PolicyDurationYears: [1, [Validators.required, Validators.min(1), Validators.max(100)]]
    });
  }

  loadAll(): void {
    this.service.getAll().subscribe({
      next: (res) => { this.policyTypes = res || []; },
      error: (err) => { this.error = err?.error || 'Failed to load policy types.'; }
    });
  }

  startCreate(): void {
    this.createMode = true;
    this.editing = false;
    this.form.reset({ PolicyTypeID: 0, CoverageLimit: 100000, PolicyDurationYears: 1 });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  edit(type: any): void {
    this.createMode = false;
    this.editing = true;
    this.form.patchValue({
      PolicyTypeID: type.policyTypeID ?? type.PolicyTypeID ?? 0,
      TypeName: type.typeName ?? type.TypeName,
      Description: type.description ?? type.Description,
      CoverageLimit: type.coverageLimit ?? type.CoverageLimit,
      PolicyDurationYears: type.policyDurationYears ?? type.PolicyDurationYears
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editing = false;
    this.createMode = false;
    this.form.reset({ PolicyTypeID: 0, CoverageLimit: 100000, PolicyDurationYears: 1 });
  }

  save(): void {
    this.message = '';
    this.error = '';

    if (this.form.invalid) {
      this.error = 'Please fix validation errors.';
      return;
    }

    const payload = {
      PolicyTypeID: Number(this.form.value.PolicyTypeID),
      TypeName: this.form.value.TypeName,
      Description: this.form.value.Description,
      CoverageLimit: parseFloat(this.form.value.CoverageLimit),
      PolicyDurationYears: Number(this.form.value.PolicyDurationYears)
    };

    const request = this.editing && payload.PolicyTypeID > 0
      ? this.service.update(payload.PolicyTypeID, payload)
      : this.service.create(payload);

    request.subscribe({
      next: () => {
        this.message = this.editing ? 'Policy type updated.' : 'Policy type created.';
        this.editing = false;
        this.createMode = false;
        this.loadAll();
        this.form.reset({ PolicyTypeID: 0, CoverageLimit: 100000, PolicyDurationYears: 1 });
      },
      error: (err) => {
        this.error = err?.error || (err?.message ?? 'Operation failed.');
      }
    });
  }

  delete(id: number): void {
    if (!confirm('Are you sure you want to delete this policy type?')) return;
    this.service.delete(id).subscribe({
      next: () => {
        this.message = 'Deleted successfully.';
        this.loadAll();
      },
      error: (err) => {
        this.error = err?.error || 'Failed to delete.';
      }
    });
  }
}
