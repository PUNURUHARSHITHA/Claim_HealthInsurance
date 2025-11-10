import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { AgentService } from '../../services/agent.service';
 
@Component({
  selector: 'app-agent',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, HttpClientModule],
  templateUrl: './agents.html',
  styleUrls: ['./agents.css']
})
export class AgentComponent implements OnInit {
  agents: any[] = [];
  form!: FormGroup;
  message = '';
  error = '';
  isAdmin = false;
  editing = false;
  createMode = false;
 
  constructor(private service: AgentService, private fb: FormBuilder) {}
 
  ngOnInit(): void {
    this.buildForm();
    this.detectRole();
    this.loadAll();
  }
 
  private buildForm(): void {
    this.form = this.fb.group({
      AgentID: [0],
      Name: ['', [Validators.required, Validators.maxLength(100)]],
      ContactNumber: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
      Email: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@gmail\.com$/)]]
    });
  }
 
  private detectRole(): void {
    const token = localStorage.getItem('token');
    if (!token) { this.isAdmin = false; return; }
 
    try {
      const parts = token.split('.');
      if (parts.length < 2) { this.isAdmin = false; return; }
      const payloadJson = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
      const payload = JSON.parse(payloadJson);
 
      const rolesSet = new Set<string>();
      const pushRoleValue = (val: any) => {
        if (!val) return;
        if (Array.isArray(val)) val.forEach((v: any) => rolesSet.add(String(v).toLowerCase()));
        else if (typeof val === 'string') String(val).split(/[,\s]+/).forEach((r: string) => rolesSet.add(r.toLowerCase()));
        else rolesSet.add(String(val).toLowerCase());
      };
 
      pushRoleValue(payload.role || payload.roles || payload.rolesClaim || payload.roleClaim);
      Object.keys(payload).forEach(k => {
        const kl = k.toLowerCase();
        if (kl.includes('role')) pushRoleValue(payload[k]);
        if (k.toLowerCase().endsWith('/role') || k.toLowerCase().endsWith('/roles')) pushRoleValue(payload[k]);
      });
 
      this.isAdmin = rolesSet.has('admin');
    } catch (e) {
      this.isAdmin = false;
    }
  }
 
  loadAll(): void {
    this.service.getAll().subscribe({
      next: (res) => { this.agents = res || []; },
      error: (err) => { this.error = err?.error || 'Failed to load agents.'; }
    });
  }
 
  startCreate(): void {
    this.createMode = true;
    this.editing = false;
    this.form.reset({ AgentID: 0 });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
 
  edit(agent: any): void {
    if (!this.isAdmin) return;
    this.createMode = false;
    this.editing = true;
    this.form.patchValue({
      AgentID: agent.agentID ?? agent.AgentID ?? 0,
      Name: agent.name ?? agent.Name,
      ContactNumber: agent.contactNumber ?? agent.ContactNumber,
      Email: agent.email ?? agent.Email
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
 
  cancelEdit(): void {
    this.editing = false;
    this.createMode = false;
    this.form.reset({ AgentID: 0 });
  }
 
  save(): void {
    this.message = '';
    this.error = '';
    if (this.form.invalid) { this.error = 'Please fix validation errors.'; return; }
 
    const payload = {
      AgentID: Number(this.form.value.AgentID),
      Name: this.form.value.Name,
      ContactNumber: this.form.value.ContactNumber,
      Email: this.form.value.Email
    };
 
    if (this.editing && payload.AgentID && payload.AgentID > 0) {
      this.service.update(payload.AgentID, payload).subscribe({
        next: () => { this.message = 'Agent updated.'; this.error = ''; this.editing = false; this.loadAll(); this.form.reset({ AgentID: 0 }); },
        error: (err) => { this.error = err?.error || (err?.message ?? 'Failed to update.'); }
      });
    } else {
      this.service.create(payload).subscribe({
        next: () => { this.message = 'Agent created.'; this.error = ''; this.createMode = false; this.loadAll(); this.form.reset({ AgentID: 0 }); },
        error: (err) => { this.error = err?.error || (err?.message ?? 'Failed to create.'); }
      });
    }
  }
 
  delete(id: number): void {
    if (!this.isAdmin) return;
    if (!confirm('Are you sure you want to delete this agent?')) return;
    this.service.delete(id).subscribe({
      next: () => { this.message = 'Deleted successfully.'; this.loadAll(); },
      error: (err) => { this.error = err?.error || 'Failed to delete.'; }
    });
  }
}
 
 
 