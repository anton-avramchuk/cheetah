import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs/operators';
import { AuthStore } from '@cheetah/auth/data-access';

export const unauthorizedInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  if (req.url.includes('/auth/login')) return next(req);
  return next(req).pipe(
    tap({ error: (err) => { if (err.status === 401) authStore.logout(); } })
  );
};
