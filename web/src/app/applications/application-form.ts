import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApplicationStatus, JobApplicationService } from './job-application.service';

const nextStatuses: Record<ApplicationStatus, ApplicationStatus[]> = {
  Applied: ['Interviewing', 'Rejected'],
  Interviewing: ['Offer', 'Rejected'],
  Offer: ['Accepted', 'Rejected'],
  Accepted: [],
  Rejected: [],
};

/**
 * Page to create a new job application or edit an existing one (when the URL has an id).
 */
@Component({
  selector: 'app-application-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './application-form.html',
})
export class ApplicationForm implements OnInit {
  private readonly jobApplicationService = inject(JobApplicationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = this.route.snapshot.paramMap.get('id');
  protected readonly today = new Date().toISOString().slice(0, 10);
  protected readonly statusOptions = signal<ApplicationStatus[]>([]);
  protected readonly errorMessage = signal('');

  protected readonly form = new FormGroup({
    company: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    position: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    appliedOn: new FormControl(this.today, { nonNullable: true, validators: [Validators.required] }),
    status: new FormControl<ApplicationStatus>('Applied', { nonNullable: true }),
  });

  ngOnInit(): void {
    if (this.id === null) {
      return;
    }

    this.jobApplicationService.getById(this.id).subscribe({
      next: (application) => {
        this.form.setValue({
          company: application.company,
          position: application.position,
          appliedOn: application.appliedOn,
          status: application.status,
        });
        this.statusOptions.set([application.status, ...nextStatuses[application.status]]);
      },
      error: () => {
        this.router.navigate(['/applications']);
      },
    });
  }

  protected submit(): void {
    const { company, position, appliedOn, status } = this.form.getRawValue();

    if (this.id === null) {
      this.jobApplicationService.create({ company, position, appliedOn }).subscribe({
        next: () => {
          this.router.navigate(['/applications']);
        },
        error: (error: HttpErrorResponse) => {
          this.showError(error);
        },
      });
      return;
    }

    this.jobApplicationService.update(this.id, { company, position, appliedOn, status }).subscribe({
      next: () => {
        this.router.navigate(['/applications']);
      },
      error: (error: HttpErrorResponse) => {
        this.showError(error);
      },
    });
  }

  private showError(error: HttpErrorResponse): void {
    this.errorMessage.set(error.error?.detail ?? 'The application could not be saved.');
  }
}
