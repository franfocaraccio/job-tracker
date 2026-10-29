import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ApplicationForm } from './application-form';
import { ApplicationStatus, JobApplicationService } from './job-application.service';

describe('ApplicationForm', () => {
  async function renderForm(id: string | null, status: ApplicationStatus): Promise<HTMLElement> {
    TestBed.configureTestingModule({
      imports: [ApplicationForm],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(id === null ? {} : { id }) } } },
        {
          provide: JobApplicationService,
          useValue: {
            getById: () => of({ id: '1', company: 'Ballast Lane', position: '.NET Developer', status, appliedOn: '2026-10-20' }),
          },
        },
      ],
    });

    const fixture = TestBed.createComponent(ApplicationForm);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function statusOptions(page: HTMLElement): string[] {
    return Array.from(page.querySelectorAll('#status option')).map((option) => option.textContent!.trim());
  }

  it('offers only the current status and the valid next ones', async () => {
    const page = await renderForm('1', 'Applied');

    expect(statusOptions(page)).toEqual(['Applied', 'Interviewing', 'Rejected']);
  });

  it('offers only the current status when the process is closed', async () => {
    const page = await renderForm('1', 'Rejected');

    expect(statusOptions(page)).toEqual(['Rejected']);
  });

  it('does not show the status when creating a new application', async () => {
    const page = await renderForm(null, 'Applied');

    expect(page.querySelector('#status')).toBeNull();
  });
});
