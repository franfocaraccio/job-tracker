import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { ApplicationStatus, JobApplication, JobApplicationService } from './job-application.service';

/**
 * Main page for the signed-in user: lists their job applications and lets them delete one.
 */
@Component({
  selector: 'app-application-list',
  templateUrl: './application-list.html',
})
export class ApplicationList implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly jobApplicationService = inject(JobApplicationService);
  private readonly router = inject(Router);

  protected readonly userName = signal('');
  protected readonly applications = signal<JobApplication[]>([]);
  protected readonly errorMessage = signal('');

  ngOnInit(): void {
    this.authService.getCurrentUser().subscribe((user) => {
      this.userName.set(user.name);
    });

    this.loadApplications();
  }

  protected delete(application: JobApplication): void {
    if (!confirm(`Delete the application to ${application.company}?`)) {
      return;
    }

    this.jobApplicationService.delete(application.id).subscribe({
      next: () => {
        this.loadApplications();
      },
      error: () => {
        this.errorMessage.set('The application could not be deleted.');
      },
    });
  }

  protected statusClass(status: ApplicationStatus): string {
    if (status === 'Interviewing') {
      return 'bg-primary-subtle text-primary-emphasis border-primary-subtle';
    }

    if (status === 'Offer') {
      return 'bg-warning-subtle text-warning-emphasis border-warning-subtle';
    }

    if (status === 'Accepted') {
      return 'bg-success-subtle text-success-emphasis border-success-subtle';
    }

    if (status === 'Rejected') {
      return 'bg-danger-subtle text-danger-emphasis border-danger-subtle';
    }

    return 'bg-secondary-subtle text-secondary-emphasis border-secondary-subtle';
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private loadApplications(): void {
    this.jobApplicationService.getAll().subscribe({
      next: (applications) => {
        this.applications.set(applications);
      },
      error: () => {
        this.errorMessage.set('The applications could not be loaded.');
      },
    });
  }
}
