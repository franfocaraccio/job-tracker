import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from '../api';

export type ApplicationStatus = 'Applied' | 'Interviewing' | 'Offer' | 'Accepted' | 'Rejected';

export interface JobApplication {
  id: string;
  company: string;
  position: string;
  status: ApplicationStatus;
  appliedOn: string;
}

/**
 * Calls the job applications API of the signed-in user.
 */
@Injectable({ providedIn: 'root' })
export class JobApplicationService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(`${apiUrl}/applications`);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${apiUrl}/applications/${id}`);
  }
}
