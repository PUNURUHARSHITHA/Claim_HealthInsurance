import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { HospitalService } from '../../services/hospital.service';
 
@Component({
  selector: 'app-hospital',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, HttpClientModule],
  templateUrl: './hospitals.html',
  styleUrls: ['./hospitals.css']
})
export class Hospitals implements OnInit {
  hospitals: any[] = [];
  form!: FormGroup;
  message = '';
  error = '';
  isAdmin = false;
  editing = false;
  createMode = false;
 
  constructor(
    private service: HospitalService,
    private fb: FormBuilder,
    private cd: ChangeDetectorRef
  ) {}
 
  ngOnInit(): void {
    this.buildForm();
    this.detectRole();
    this.loadAll();
  }
 
  private buildForm(): void {
    this.form = this.fb.group({
      HospitalID: [0],
      HospitalName: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[A-Za-z\s]+$/)]],
      Location: ['', [Validators.required, Validators.maxLength(100)]],
      IsNetworkHospital: [true]
    });
  }
 
  private detectRole(): void {
    const token = localStorage.getItem('token');
    if (!token) { this.isAdmin = false; return; }
 
    try {
      const parts = token.split('.');
      if (parts.length < 2) { this.isAdmin = false; return; }
      const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
      const roles: string[] = [];
      if (payload.role) roles.push(...(Array.isArray(payload.role) ? payload.role : String(payload.role).split(/[,\s]+/)));
      if (payload.roles) roles.push(...(Array.isArray(payload.roles) ? payload.roles : String(payload.roles).split(/[,\s]+/)));
      Object.keys(payload).forEach(k => {
        if (k.toLowerCase().includes('role')) {
          const v = payload[k];
          roles.push(...(Array.isArray(v) ? v : String(v).split(/[,\s]+/)));
        }
      });
      this.isAdmin = roles.map(r => String(r).toLowerCase()).includes('admin');
    } catch {
      this.isAdmin = false;
    }
  }
 
  loadAll(): void {
    this.service.getAll().subscribe({
      next: (res) => {
        this.hospitals = res || [];
        this.cd.detectChanges(); // ✅ Ensure table updates immediately
      },
      error: (err) => {
        this.error = err?.error || 'Failed to load hospitals.';
        this.cd.detectChanges(); // ✅ Ensure error message shows
      }
    });
  }
 
  startCreate(): void {
    this.createMode = true;
    this.editing = false;
    this.form.reset({ HospitalID: 0, IsNetworkHospital: true });
    this.cd.detectChanges(); // ✅ Fixes double-click issue
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
 
  edit(h: any): void {
    if (!this.isAdmin) return;
    this.createMode = false;
    this.editing = true;
    this.form.patchValue({
      HospitalID: h.hospitalID ?? h.HospitalID ?? 0,
      HospitalName: h.hospitalName ?? h.HospitalName,
      Location: h.location ?? h.Location,
      IsNetworkHospital: h.isNetworkHospital ?? h.IsNetworkHospital ?? true
    });
    this.cd.detectChanges(); // ✅ Ensures form shows instantly
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
 
  cancelEdit(): void {
    this.editing = false;
    this.createMode = false;
    this.form.reset({ HospitalID: 0, IsNetworkHospital: true });
    this.cd.detectChanges(); // ✅ Reflect cancel immediately
  }
 
  save(): void {
    this.message = '';
    this.error = '';
    if (this.form.invalid) {
      this.error = 'Please fix validation errors.';
      this.cd.detectChanges(); // ✅ Show validation error instantly
      return;
    }
 
    const payload = {
      HospitalID: Number(this.form.value.HospitalID),
      HospitalName: this.form.value.HospitalName,
      Location: this.form.value.Location,
      IsNetworkHospital: !!this.form.value.IsNetworkHospital
    };
 
    const request = this.editing && payload.HospitalID > 0
      ? this.service.update(payload.HospitalID, payload)
      : this.service.create(payload);
 
    request.subscribe({
      next: () => {
        this.message = this.editing ? 'Hospital updated.' : 'Hospital created.';
        this.error = '';
        this.editing = false;
        this.createMode = false;
        this.loadAll();
        this.form.reset({ HospitalID: 0, IsNetworkHospital: true });
        this.cd.detectChanges(); // ✅ Refresh UI after save
      },
      error: (err) => {
        this.error = err?.error || err?.message || 'Operation failed.';
        this.cd.detectChanges(); // ✅ Show error instantly
      }
    });
  }
 
  delete(id: number): void {
    if (!this.isAdmin) return;
    if (!confirm('Are you sure you want to delete this hospital?')) return;
    this.service.delete(id).subscribe({
      next: () => {
        this.message = 'Deleted successfully.';
        this.loadAll();
        this.cd.detectChanges(); // ✅ Refresh after delete
      },
      error: (err) => {
        this.error = err?.error || 'Failed to delete.';
        this.cd.detectChanges(); // ✅ Show error instantly
      }
    });
  }
 
  trackById(index: number, item: any): number {
    return item.hospitalID ?? item.HospitalID;
  }
}
 
 