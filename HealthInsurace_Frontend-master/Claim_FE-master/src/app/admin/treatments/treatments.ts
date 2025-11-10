import { Component, OnInit ,ChangeDetectorRef} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule,FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { TreatmentService } from '../../services/treatment.service';
import { AuthService } from '../../services/auth.service';


@Component({
  selector: 'app-treatment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,FormsModule, RouterModule, HttpClientModule],
  templateUrl: './treatments.html',
  styleUrls: ['./treatments.css']
})
export class Treatments implements OnInit {
  treatments: any[] = [];
  form!: FormGroup;
  message = '';
  error = '';
  isAdmin = false;
  isPolicyholder = false;
  editing = false;
  createMode = false;

  constructor(
    private service: TreatmentService,
    private fb: FormBuilder,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.detectRole();
    
    if (this.isAdmin || this.isPolicyholder) this.loadAll();
  }

  private detectRole(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.isAdmin = role === 'admin';
    this.isPolicyholder = role === 'policyholder';
  }

  private buildForm(): void {
    this.form = this.fb.group({
      TreatmentID: [0],
      TreatmentName: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[A-Za-z\s]+$/)]],
      IsCovered: [true, Validators.required],
      WaitingPeriodMonths: [0, [Validators.required, Validators.min(0), Validators.max(24)]]
    });
  }

  loadAll(): void {
    this.service.getAll().subscribe({
      next: (res) => {this.treatments = res || [];
      this.cdr.detectChanges();
      },
      error: (err) => this.error = err?.error || 'Failed to load treatments.'
    });
  }

  startCreate(): void {
    this.createMode = true;
    this.editing = false;
    this.form.reset({ TreatmentID: 0, IsCovered: true, WaitingPeriodMonths: 0 });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  edit(t: any): void {
    this.createMode = false;
    this.editing = true;
    this.form.patchValue({
      TreatmentID: t.treatmentID ?? t.TreatmentID,
      TreatmentName: t.treatmentName ?? t.TreatmentName,
      IsCovered: t.isCovered ?? t.IsCovered,
      WaitingPeriodMonths: t.waitingPeriodMonths ?? t.WaitingPeriodMonths
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editing = false;
    this.createMode = false;
    this.form.reset({ TreatmentID: 0, IsCovered: true, WaitingPeriodMonths: 0 });
  }

  save(): void {
    this.message = '';
    this.error = '';
    if (this.form.invalid) {
      this.error = 'Please fix validation errors.';
      return;
    }

    const payload = {
      TreatmentID: Number(this.form.value.TreatmentID),
      TreatmentName: this.form.value.TreatmentName,
      IsCovered: this.form.value.IsCovered,
      WaitingPeriodMonths: Number(this.form.value.WaitingPeriodMonths)
    };

    const request = this.editing && payload.TreatmentID > 0
      ? this.service.update(payload.TreatmentID, payload)
      : this.service.create(payload);

    request.subscribe({
      next: () => {
        this.message = this.editing ? 'Treatment updated.' : 'Treatment created.';
        this.editing = false;
        this.createMode = false;
        this.loadAll();
        this.form.reset({ TreatmentID: 0, IsCovered: true, WaitingPeriodMonths: 0 });
      },
      error: (err) => this.error = err?.error || 'Operation failed.'
    });
  }

  delete(id: number): void {
    if (!confirm('Are you sure you want to delete this treatment?')) return;
    this.service.delete(id).subscribe({
      next: () => {
        this.message = 'Deleted successfully.';
        this.loadAll();
      },
      error: (err) => this.error = err?.error || 'Failed to delete.'
    });
  }
}
