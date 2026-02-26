import { Routes } from '@angular/router';
import { Component } from '@angular/core';

@Component({ standalone: true, template: `<div class="card"><h2>Кандидаты</h2><p>Страница в разработке</p></div>` })
class CandidatesPageComponent {}

export const CANDIDATES_ROUTES: Routes = [{ path: '', component: CandidatesPageComponent }];
