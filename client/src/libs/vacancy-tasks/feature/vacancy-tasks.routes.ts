import { Routes } from '@angular/router';
import { Component } from '@angular/core';

@Component({ standalone: true, template: `<div class="card"><h2>Задачи</h2><p>Страница в разработке</p></div>` })
class VacancyTasksPageComponent {}

export const VACANCY_TASKS_ROUTES: Routes = [{ path: '', component: VacancyTasksPageComponent }];
