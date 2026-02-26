import { Routes } from '@angular/router';
import { LAYOUT_ROUTES } from '@cheetah/layout/feature-shell';

export const appRoutes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('@cheetah/auth/feature-login').then(m => m.LoginComponent),
  },
  {
    path: 'logout',
    loadComponent: () => import('@cheetah/auth/feature-login').then(m => m.LogoutComponent),
  },
  ...LAYOUT_ROUTES,
  { path: '**', redirectTo: '' },
];
