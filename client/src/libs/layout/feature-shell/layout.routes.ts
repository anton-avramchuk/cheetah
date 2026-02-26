import { Routes } from '@angular/router';
import { AppLayout } from './app-layout.component';
import { authGuard, roleGuard } from '@cheetah/auth/data-access';

export const LAYOUT_ROUTES: Routes = [
    {
        path: '',
        component: AppLayout,
        canActivate: [authGuard],
        children: [
            { path: '', redirectTo: 'vacancies', pathMatch: 'full' },
            {
                path: 'vacancies',
                loadChildren: () => import('@cheetah/recruitment/feature').then(m => m.RECRUITMENT_ROUTES),
            },
            {
                path: 'candidates',
                loadChildren: () => import('@cheetah/candidates/feature').then(m => m.CANDIDATES_ROUTES),
            },
            {
                path: 'tasks',
                loadChildren: () => import('@cheetah/vacancy-tasks/feature').then(m => m.VACANCY_TASKS_ROUTES),
            },
            {
                path: 'admin',
                canActivate: [roleGuard(['Admin'])],
                loadChildren: () => import('@cheetah/identity/feature').then(m => m.IDENTITY_ROUTES),
            },
        ],
    },
];
