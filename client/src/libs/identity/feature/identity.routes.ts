import { Routes } from '@angular/router';
import { Component } from '@angular/core';

@Component({ standalone: true, template: `<div class="card"><h2>Пользователи</h2><p>Страница в разработке</p></div>` })
class IdentityPageComponent {}

export const IDENTITY_ROUTES: Routes = [{ path: '', component: IdentityPageComponent }];
