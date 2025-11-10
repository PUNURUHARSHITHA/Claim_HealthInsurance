import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClientModule, HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ClaimService } from '../../services/claim.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-claim',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule],
  templateUrl: './claims.html',
  styleUrls: ['./claims.css']
})
export class ClaimsComponent implements OnInit {
  claimForm: FormGroup;
  feedbackMessage = '';
  isSubmitting = false;

  hospitals: any[] = [];
  treatments: any[] = [];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private claimService: ClaimService,
    private auth: AuthService,
    private router: Router
  ) {
    this.claimForm = this.fb.group({
      policyholderID: [null, [Validators.required, Validators.min(1)]],
      hospitalID: [null, Validators.required],
      treatmentID: [null, Validators.required],
      treatmentDetails: ['', Validators.required],
      documents: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    const headers = this.auth.getAuthHeaders();

    this.http.get<any[]>('http://localhost:5030/api/hospital', { headers }).subscribe({
      next: res => this.hospitals = res || [],
      error: err => console.error('Failed to load hospitals', err)
    });

    this.http.get<any[]>('http://localhost:5030/api/treatment', { headers }).subscribe({
      next: res => this.treatments = res || [],
      error: err => console.error('Failed to load treatments', err)
    });

    this.claimForm.get('treatmentID')?.valueChanges.subscribe(id => {
      const selected = this.treatments.find(t => t.treatmentID === +id);
      this.claimForm.get('treatmentDetails')?.setValue(selected?.treatmentName || '');
    });
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const files: File[] = Array.from(input.files);
    const allowedExtensions = ['.pdf', '.jpg', '.jpeg', '.png'];
    const fileNames = files.map(f => f.name.toLowerCase());
    const uniqueNames = new Set(fileNames);

    if (files.length < 1 || files.length > 2) {
      this.feedbackMessage = 'Upload 1 or 2 documents only.';
      return;
    }

    if (uniqueNames.size !== fileNames.length) {
      this.feedbackMessage = 'Uploaded documents must have different filenames.';
      return;
    }

    for (const file of files) {
      const ext = '.' + file.name.split('.').pop()?.toLowerCase();
      if (!allowedExtensions.includes(ext)) {
        this.feedbackMessage = `File ${file.name} has unsupported format.`;
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        this.feedbackMessage = `File ${file.name} exceeds 5 MB limit.`;
        return;
      }

      const baseName = file.name.replace(/\.[^/.]+$/, '');
      if (/scan|copy/i.test(baseName)) {
        this.feedbackMessage = `File ${file.name} appears to be a fraud document.`;
        return;
      }
    }

    this.claimForm.get('documents')?.setValue(files);
    this.feedbackMessage = '';
  }

  submitClaim(): void {
    if (this.claimForm.invalid || this.feedbackMessage) return;

    this.isSubmitting = true;
    const formData = new FormData();
    const formValue = this.claimForm.value;

    Object.entries(formValue).forEach(([key, value]) => {
      if (key === 'documents' && Array.isArray(value)) {
        value.forEach(file => formData.append('Documents', file));
      } else {
        formData.append(key, String(value));
      }
    });

    this.claimService.submitClaim(formData).subscribe({
      next: (res: any) => {
        this.feedbackMessage = res || 'Claim submitted successfully.';
        this.claimForm.reset();
        this.isSubmitting = false;
        this.router.navigate(['/home']);
      },
      error: (err: HttpErrorResponse) => {
        this.feedbackMessage = err.error || 'Submission failed.';
        this.isSubmitting = false;
      }
    });
  }
}
