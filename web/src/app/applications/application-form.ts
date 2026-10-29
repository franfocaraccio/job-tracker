import { formatDate } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApplicationStatus, JobApplicationService } from './job-application.service';

const nextStatuses: Record<ApplicationStatus, ApplicationStatus[]> = {
  Applied: ['Interviewing', 'Rejected'],
  Interviewing: ['Offer', 'Rejected'],
  Offer: ['Accepted', 'Rejected'],
  Accepted: [],
  Rejected: [],
};

/**
 * Modal to create a new job application or edit an existing one (when it receives an id).
 */
@Component({
  selector: 'app-application-form',
  imports: [ReactiveFormsModule],
  templateUrl: './application-form.html',
})
export class ApplicationForm implements OnInit {
  private readonly jobApplicationService = inject(JobApplicationService);

  readonly applicationId = input<string | null>(null);
  readonly saved = output();
  readonly cancelled = output();

  protected readonly today = formatDate(new Date(), 'yyyy-MM-dd', 'en-US');
  protected readonly statusOptions = signal<ApplicationStatus[]>([]);
  protected readonly errorMessage = signal('');

  protected readonly form = new FormGroup({
    company: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    position: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    appliedOn: new FormControl(this.today, { nonNullable: true, validators: [Validators.required] }),
    status: new FormControl<ApplicationStatus>('Applied', { nonNullable: true }),
  });

  ngOnInit(): void {
    const id = this.applicationId();
    if (id === null) {
      return;
    }

    this.jobApplicationService.getById(id).subscribe({
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
        this.errorMessage.set('The application could not be loaded.');
      },
    });
  }

  protected submit(): void {
    const id = this.applicationId();
    const { company, position, appliedOn, status } = this.form.getRawValue();

    if (id === null) {
      this.jobApplicationService.create({ company, position, appliedOn }).subscribe({
        next: () => {
          this.saved.emit();
        },
        error: (error: HttpErrorResponse) => {
          this.showError(error);
        },
      });
      return;
    }

    this.jobApplicationService.update(id, { company, position, appliedOn, status }).subscribe({
      next: () => {
        this.saved.emit();
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
