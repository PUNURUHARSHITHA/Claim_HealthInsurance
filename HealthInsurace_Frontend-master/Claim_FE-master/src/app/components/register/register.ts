import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../navbar/navbar';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, NavbarComponent],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  message = '';
  loading = false;

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(4)]],
      password: ['', [
        Validators.required,
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,}$/)
      ]]
    });
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.loading = true;
      this.message = '';

      this.http.post<any>('http://localhost:5030/api/auth/register', this.registerForm.value)
        .subscribe({
          next: res => {
            console.log('Register response:', res); // ✅ Debug log
            this.message = '✅ Successfully registered!';
            this.registerForm.reset();
            this.loading = false;

            setTimeout(() => this.message = '', 3000);
          },
          error: err => {
            console.error('Register error:', err); // ✅ Debug log
            this.message = err.error?.message || '❌ Registration failed. Try a different username.';
            this.loading = false;

            setTimeout(() => this.message = '', 3000);
          }
        });
    }
  }
}
