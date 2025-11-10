import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from '../navbar/navbar';

@Component({
  selector: 'app-logout',
  standalone: true,
  imports: [CommonModule, RouterModule, HttpClientModule, NavbarComponent],
  templateUrl: './logout.html',
  styleUrls: ['./logout.css']
})
export class LogoutComponent {
  message = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  onLogout(): void {
  this.message = '';
  this.error = '';

  const role = this.auth.getUserRole();
  const username = this.auth.getUserName();

  this.auth.logoutFromServer().subscribe({
    next: () => {
      this.auth.clearTokens();
      this.message = `✅ Logged out successfully.`;
      setTimeout(() => this.router.navigate(['/home']), 2000); // ✅ Redirect to Home
    },
    error: (err) => {
      this.auth.clearTokens();
      this.error = err?.error || '❌ Logout failed. Token may be invalid or already revoked.';
      setTimeout(() => this.router.navigate(['/home']), 3000); // ✅ Redirect to Home even on error
    }
  });
}

}
