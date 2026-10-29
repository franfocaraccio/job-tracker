import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Main page for the signed-in user.
 */
@Component({
  selector: 'app-application-list',
  templateUrl: './application-list.html',
})
export class ApplicationList implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly userName = signal('');

  ngOnInit(): void {
    this.authService.getCurrentUser().subscribe((user) => {
      this.userName.set(user.name);
    });
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
