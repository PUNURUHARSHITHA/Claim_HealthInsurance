import { Component, OnInit } from '@angular/core';
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
export class HospitalComponent implements OnInit {
  hospitals: any[] = [];
  form!: FormGroup;
  message = '';
  error = '';
  isAdmin = false;
  editing = false;
  createMode = false;

  constructor(private service: HospitalService, private fb: FormBuilder) {}

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
      Object.keys(payload).forEach(k => { if (k.toLowerCase().includes('role')) { const v = payload[k]; roles.push(...(Array.isArray(v)? v : String(v).split(/[,\s]+/))); } });
      this.isAdmin = roles.map(r => String(r).toLowerCase()).includes('admin');
    } catch (e) {
      this.isAdmin = false;
    }
  }

  loadAll(): void {
    this.service.getAll().subscribe({
      next: (res) => { this.hospitals = res || []; },
      error: (err) => { this.error = err?.error || 'Failed to load hospitals.'; }
    });
  }

  startCreate(): void {
    this.createMode = true;
    this.editing = false;
    this.form.reset({ HospitalID: 0, IsNetworkHospital: true });
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
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editing = false;
    this.createMode = false;
    this.form.reset({ HospitalID: 0, IsNetworkHospital: true });
  }

  save(): void {
    this.message = '';
    this.error = '';
    if (this.form.invalid) { this.error = 'Please fix validation errors.'; return; }

    const payload = {
      HospitalID: Number(this.form.value.HospitalID),
      HospitalName: this.form.value.HospitalName,
      Location: this.form.value.Location,
      IsNetworkHospital: !!this.form.value.IsNetworkHospital
    };

    if (this.editing && payload.HospitalID && payload.HospitalID > 0) {
      this.service.update(payload.HospitalID, payload).subscribe({
        next: () => { this.message = 'Hospital updated.'; this.error = ''; this.editing = false; this.loadAll(); this.form.reset({ HospitalID: 0, IsNetworkHospital: true }); },
        error: (err) => { this.error = err?.error || (err?.message ?? 'Failed to update.'); }
      });
    } else {
      this.service.create(payload).subscribe({
        next: () => { this.message = 'Hospital created.'; this.error = ''; this.createMode = false; this.loadAll(); this.form.reset({ HospitalID: 0, IsNetworkHospital: true }); },
        error: (err) => { this.error = err?.error || (err?.message ?? 'Failed to create.'); }
      });
    }
  }

  delete(id: number): void {
    if (!this.isAdmin) return;
    if (!confirm('Are you sure you want to delete this hospital?')) return;
    this.service.delete(id).subscribe({
      next: () => { this.message = 'Deleted successfully.'; this.loadAll(); },
      error: (err) => { this.error = err?.error || 'Failed to delete.'; }
    });
  }
}
