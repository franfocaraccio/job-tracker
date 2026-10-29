import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Lets the user into protected pages only when signed in; otherwise sends them to the login page.
 */
export const authGuard: CanActivateFn = () => {
  if (inject(AuthService).isLoggedIn()) {
    return true;
  }

  return inject(Router).createUrlTree(['/login']);
};
