import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withInMemoryScrolling, withComponentInputBinding } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { appRoutes } from './app.routes';
import { jwtInterceptor, unauthorizedInterceptor } from '@cheetah/auth/interceptors';
import { MENU_CONTRIBUTOR, HEADER_WIDGET, APP_CONFIG, BASE_PATH } from '@cheetah/shared/core';
import { UserMenuContributor } from '@cheetah/auth/data-access';
import { UserInfoWidgetComponent } from '@cheetah/auth/feature-login';
import { RecruitmentMenuContributor } from '@cheetah/recruitment/feature';
import { IdentityMenuContributor } from '@cheetah/identity/feature';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      appRoutes,
      withComponentInputBinding(),
      withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' })
    ),
    provideAnimationsAsync(),
    provideHttpClient(
      withFetch(),
      withInterceptors([jwtInterceptor, unauthorizedInterceptor])
    ),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: { darkModeSelector: '.app-dark' }
      },
      ripple: true,
    }),

    // App config
    {
      provide: APP_CONFIG,
      useValue: {
        appName:  'Cheetah CRM',
        homeUrl:  '/',
        apiUrl:   'http://localhost:5555',
      },
    },

    // OpenAPI base path
    { provide: BASE_PATH, useValue: 'http://localhost:5555' },

    // Header widgets (multi) — UserInfoWidget из auth
    { provide: HEADER_WIDGET, useValue: { component: UserInfoWidgetComponent, order: 100 }, multi: true },

    // Menu contributors (multi)
    { provide: MENU_CONTRIBUTOR, useClass: RecruitmentMenuContributor, multi: true },
    { provide: MENU_CONTRIBUTOR, useClass: IdentityMenuContributor,    multi: true },
    { provide: MENU_CONTRIBUTOR, useClass: UserMenuContributor,        multi: true },
  ],
};
