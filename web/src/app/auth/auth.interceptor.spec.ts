import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    localStorage.clear();
  });

  it('adds the token to the request', () => {
    localStorage.setItem('accessToken', 'my-token');

    http.get('/api/applications').subscribe();

    const request = httpTesting.expectOne('/api/applications');
    expect(request.request.headers.get('Authorization')).toBe('Bearer my-token');
    request.flush([]);
  });

  it('does not add a header when there is no token', () => {
    http.get('/api/applications').subscribe();

    const request = httpTesting.expectOne('/api/applications');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush([]);
  });

  it('signs out and opens the login page when the API answers 401', () => {
    localStorage.setItem('accessToken', 'expired-token');

    http.get('/api/users/me').subscribe();
    httpTesting.expectOne('/api/users/me').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(localStorage.getItem('accessToken')).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('lets the login page handle its own 401 (wrong credentials)', () => {
    let status = 0;

    http.post('/api/auth/login', {}).subscribe({
      error: (error: HttpErrorResponse) => {
        status = error.status;
      },
    });
    httpTesting.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(status).toBe(401);
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
