// Адаптировано из sakai-ng/src/app/layout/component/app.topbar.ts
// Изменено: добавлены HEADER_WIDGET + ngComponentOutlet + APP_CONFIG
import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StyleClassModule } from 'primeng/styleclass';
import { ButtonModule } from 'primeng/button';
import { LayoutService } from '@cheetah/layout/data-access';
import { HEADER_WIDGET, APP_CONFIG } from '@cheetah/shared/core';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [CommonModule, RouterModule, StyleClassModule, ButtonModule],
    template: `
        <div class="layout-topbar">
            <div class="layout-topbar-logo-container">
                <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
                    <i class="pi pi-bars"></i>
                </button>
                <a class="layout-topbar-logo" [routerLink]="config.homeUrl">
                    @if (config.logoUrl) {
                        <img [src]="config.logoUrl" [alt]="config.appName" style="height: 2.5rem;" />
                    } @else {
                        <i class="pi pi-bolt" style="font-size: 2rem; color: var(--primary-color)"></i>
                    }
                    <span class="layout-topbar-app-name">{{ config.appName }}</span>
                </a>
            </div>

            <div class="layout-topbar-actions">
                <!-- Кнопка смены темы -->
                <button class="layout-topbar-action" (click)="toggleDarkMode()" title="Сменить тему">
                    <i class="pi" [class.pi-moon]="!layoutService.isDarkTheme()" [class.pi-sun]="layoutService.isDarkTheme()"></i>
                </button>

                <!--
                    Виджеты из HEADER_WIDGET токена.
                    ngComponentOutlet рендерит компоненты инлайн без обёртки —
                    это ключевое отличие от DynamicComponent и решение проблемы event routing.
                -->
                @for (widget of sortedWidgets(); track widget.component) {
                    <ng-container *ngComponentOutlet="widget.component; inputs: widget.inputs" />
                }
            </div>
        </div>
    `
})
export class AppTopbar {
    readonly layoutService = inject(LayoutService);
    readonly config        = inject(APP_CONFIG);

    private readonly widgets = inject(HEADER_WIDGET, { optional: true }) ?? [];
    readonly sortedWidgets   = computed(() =>
        [...this.widgets].sort((a, b) => a.order - b.order)
    );

    toggleDarkMode(): void {
        this.layoutService.layoutConfig.update(state => ({ ...state, darkTheme: !state.darkTheme }));
    }
}
