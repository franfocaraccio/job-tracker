import { Routes } from '@angular/router';
import { ApplicationForm } from './applications/application-form';
import { ApplicationList } from './applications/application-list';
import { authGuard } from './auth/auth.guard';
import { Login } from './auth/login';
import { Register } from './auth/register';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'applications', component: ApplicationList, canActivate: [authGuard] },
  { path: 'applications/new', component: ApplicationForm, canActivate: [authGuard] },
  { path: 'applications/:id/edit', component: ApplicationForm, canActivate: [authGuard] },
  { path: '', redirectTo: 'applications', pathMatch: 'full' },
];
