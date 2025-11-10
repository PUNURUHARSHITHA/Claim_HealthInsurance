import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { PolicyholderListService, PolicyholderDto } from '../../services/policyholder-list.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-policyholder-list',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './policyholder-list.html',
  styleUrls: ['./policyholder-list.css']
})
export class PolicyholderList implements OnInit {
  policyholders: PolicyholderDto[] = [];
  errorMessage: string = '';
  loading: boolean = false;
  isAdmin: boolean = false;

  private auth = inject(AuthService);
  private cdRef = inject(ChangeDetectorRef);

  constructor(private service: PolicyholderListService) {}

  ngOnInit(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.isAdmin = role === 'admin';
    this.cdRef.detectChanges();

    if (this.isAdmin) {
      this.fetchPolicyholders();
    } else {
      this.errorMessage = 'Access denied. Only Admins can view policyholder data.';
      this.cdRef.detectChanges();
    }
  }

  fetchPolicyholders(): void {
    this.loading = true;
    this.cdRef.detectChanges();

    this.service.getAllPolicyholders().subscribe({
      next: (data) => {
        this.policyholders = data;
        this.loading = false;
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching policyholders:', err);
        this.errorMessage = 'Failed to load policyholder data.';
        this.loading = false;
        this.cdRef.detectChanges();
      }
    });
  }
}
