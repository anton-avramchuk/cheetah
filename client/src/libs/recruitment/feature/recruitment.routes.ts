import { Routes } from '@angular/router';
import { Component } from '@angular/core';

@Component({ standalone: true, template: `<div class="card"><h2>Вакансии</h2><p>Страница в разработке</p></div>` })
class VacanciesPageComponent {}

export const RECRUITMENT_ROUTES: Routes = [
    { path: '', component: VacanciesPageComponent },
];
