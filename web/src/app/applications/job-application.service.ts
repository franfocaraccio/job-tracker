import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export type ApplicationStatus = 'Applied' | 'Interviewing' | 'Offer' | 'Accepted' | 'Rejected';

export interface JobApplication {
  id: string;
  company: string;
  position: string;
  status: ApplicationStatus;
  appliedOn: string;
}

export interface CreateJobApplicationRequest {
  company: string;
  position: string;
  appliedOn: string;
}

export interface UpdateJobApplicationRequest {
  company: string;
  position: string;
  appliedOn: string;
  status: ApplicationStatus;
}

/**
 * Calls the job applications API of the signed-in user.
 */
@Injectable({ providedIn: 'root' })
export class JobApplicationService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(`${environment.apiUrl}/applications`);
  }

  getById(id: string): Observable<JobApplication> {
    return this.http.get<JobApplication>(`${environment.apiUrl}/applications/${id}`);
  }

  create(request: CreateJobApplicationRequest): Observable<JobApplication> {
    return this.http.post<JobApplication>(`${environment.apiUrl}/applications`, request);
  }

  update(id: string, request: UpdateJobApplicationRequest): Observable<JobApplication> {
    return this.http.put<JobApplication>(`${environment.apiUrl}/applications/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/applications/${id}`);
  }
}
