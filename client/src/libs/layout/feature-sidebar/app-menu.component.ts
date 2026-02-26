// Адаптировано из sakai-ng/src/app/layout/component/app.menu.ts
// Изменено: model берётся из MenuService вместо захардкоженного массива
import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AppMenuitem } from './app-menuitem.component';
import { MenuService } from '@cheetah/navigation/data-access';
import { StandardMenus } from '@cheetah/shared/core';
import { toSakaiMenuItem } from '@cheetah/navigation/util';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `
        <ul class="layout-menu">
            <ng-container *ngFor="let item of model(); let i = index">
                <li app-menuitem *ngIf="!item['separator']" [item]="item" [index]="i" [root]="true"></li>
                <li *ngIf="item['separator']" class="menu-separator"></li>
            </ng-container>
        </ul>
    `
})
export class AppMenu {
    private readonly menuSvc = inject(MenuService);

    // Computed signal: пересчитывается автоматически при смене ролей через AuthStore
    readonly model = computed(() => {
        const items = this.menuSvc.menus()[StandardMenus.MAIN] ?? [];
        return items.map(toSakaiMenuItem);
    });
}
