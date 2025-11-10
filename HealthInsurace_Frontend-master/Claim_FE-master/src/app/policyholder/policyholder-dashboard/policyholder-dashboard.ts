import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PolicyholderDashboardService, PolicyholderDto } from '../../services/policyholder-dashboard.service';
import { AuthService } from '../../services/auth.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-policyholder-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],

  templateUrl: './policyholder-dashboard.html',
  styleUrls: ['./policyholder-dashboard.css']
})
export class PolicyholderDashboardComponent implements OnInit {
  policyholderID: number | null = null;
  isPolicyholder: boolean = false;
  errorMessage: string = '';

  private auth = inject(AuthService);
  private service = inject(PolicyholderDashboardService);

  ngOnInit(): void {
    const role = this.auth.getUserRole()?.toLowerCase();
    this.isPolicyholder = role === 'policyholder';

    if (this.isPolicyholder) {
      this.service.getLoggedInPolicyholder().subscribe({
        next: (data) => this.policyholderID = data.policyholderID,
        error: () => this.errorMessage = 'Unable to fetch your Policyholder ID.'
      });
    }
  }
}
