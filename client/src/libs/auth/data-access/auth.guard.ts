import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const authGuard: CanActivateFn = () => {
  const auth   = inject(AuthStore);
  const router = inject(Router);
  return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

export const roleGuard = (requiredRoles: string[]): CanActivateFn => () => {
  const auth   = inject(AuthStore);
  const router = inject(Router);
  const roles  = auth.roles();
  return requiredRoles.some(r => roles.includes(r))
    ? true
    : router.createUrlTree(['/']);
};
