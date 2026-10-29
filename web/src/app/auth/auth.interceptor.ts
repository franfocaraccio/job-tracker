import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { EMPTY, catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Adds the JWT of the signed-in user to every request sent to the API.
 * When the API rejects the token (expired or invalid), it signs the user out and opens the login page.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  let authorizedRequest = request;
  const token = authService.getToken();
  if (token !== null) {
    authorizedRequest = request.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(authorizedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      const isLoginRequest = request.url.endsWith('/auth/login');
      if (error.status === 401 && !isLoginRequest) {
        authService.logout();
        router.navigate(['/login']);
        return EMPTY;
      }

      return throwError(() => error);
    }),
  );
};
