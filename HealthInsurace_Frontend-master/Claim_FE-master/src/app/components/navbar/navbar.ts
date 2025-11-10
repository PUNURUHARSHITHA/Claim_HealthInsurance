import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule } from '@angular/router';
 
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class NavbarComponent implements OnInit {
  isClosed = false;
  isLightMode = false;
  policyholderRoute = '/policyholder-form';
 
  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}
 
  ngOnInit(): void {
    this.updatePolicyholderRoute();
  }
 
  toggleSidebar(): void {
    this.isClosed = !this.isClosed;
  }
 
  toggleTheme(): void {
    this.isLightMode = !this.isLightMode;
    if (isPlatformBrowser(this.platformId)) {
      document.body.classList.toggle('light-mode', this.isLightMode);
    }
  }
 
  private updatePolicyholderRoute(): void {
    if (!isPlatformBrowser(this.platformId)) {
      this.policyholderRoute = '/policyholder-form';
      return;
    }
 
    const token = localStorage.getItem('token');
    if (!token) {
      this.policyholderRoute = '/policyholder-form';
      return;
    }
 
    try {
      const parts = token.split('.');
      if (parts.length < 2) {
        this.policyholderRoute = '/policyholder-form';
        return;
      }
 
      const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
      const roles: string[] = [];
 
      if (payload.role) {
        roles.push(...(Array.isArray(payload.role) ? payload.role : String(payload.role).split(/[,\s]+/)));
      }
 
      if (payload.roles) {
        roles.push(...(Array.isArray(payload.roles) ? payload.roles : String(payload.roles).split(/[,\s]+/)));
      }
 
      Object.keys(payload).forEach(k => {
        const kl = k.toLowerCase();
        if (kl.includes('role')) {
          const v = payload[k];
          roles.push(...(Array.isArray(v) ? v : String(v).split(/[,\s]+/)));
        }
      });
 
      const normalized = roles.map(r => String(r).toLowerCase());
      this.policyholderRoute = normalized.includes('admin') ? '/policyholders' : '/policyholder-form';
    } catch {
      this.policyholderRoute = '/policyholder-form';
    }
  }
}