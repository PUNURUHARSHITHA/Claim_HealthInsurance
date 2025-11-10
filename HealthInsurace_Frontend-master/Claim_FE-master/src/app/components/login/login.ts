import { Component, ChangeDetectorRef } from '@angular/core';

import { CommonModule } from '@angular/common';

import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { RouterModule, Router } from '@angular/router';

import { HttpClientModule } from '@angular/common/http';

import { NavbarComponent } from '../navbar/navbar';

import { AuthService } from '../../services/auth.service';
 
@Component({

  selector: 'app-login',

  standalone: true,

  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, RouterModule, NavbarComponent],

  templateUrl: './login.html',

  styleUrls: ['./login.css']

})

export class LoginComponent {

  loginForm: FormGroup;

  message = '';

  loading = false;
 
  constructor(

    private fb: FormBuilder,

    private authService: AuthService,

    private router: Router,

    private cd: ChangeDetectorRef // ✅ Injected here

  ) {

    this.loginForm = this.fb.group({

      username: ['', Validators.required],

      password: ['', Validators.required]

    });

  }
 
  onSubmit(): void {

    if (this.loginForm.invalid) return;
 
    this.loading = true;

    this.message = '';

    this.cd.detectChanges(); // ✅ Trigger UI update for loading spinner
 
    const { username, password } = this.loginForm.value;
 
    this.authService.login({ username, password }).subscribe({

      next: (res: { accessToken: string; refreshToken: string }) => {

        this.authService.storeTokens(res.accessToken, res.refreshToken);

        this.message = '✅ Successfully logged in!';

        this.loading = false;

        this.cd.detectChanges(); // ✅ Show success message immediately
 
        const role = this.authService.getUserRole()?.toLowerCase();
 
        if (role === 'admin') {

          this.router.navigate(['/admin/admin-dashboard']);

        } else if (role === 'policyholder') {

          this.router.navigate(['/policyholder/policyholder-dashboard']);

        } else if (role === 'manager') {

          this.router.navigate(['/manager']);

        } else {

          this.message = '❌ Unknown role. Cannot redirect.';

          this.cd.detectChanges(); // ✅ Show unknown role message

        }

      },

      error: (err: any) => {

        this.message = err.error?.message || '❌ Login failed. Please check your credentials.';

        this.loading = false;

        this.cd.detectChanges(); // ✅ Show error message instantly

      }

    });

  }

}

 