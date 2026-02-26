// Скопировано из sakai-ng/src/app/layout/component/app.layout.ts
// Изменено: импорты обновлены на наши пути
import { Component, Renderer2, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AppTopbar } from '@cheetah/layout/feature-header';
import { AppSidebar } from '@cheetah/layout/feature-sidebar';
import { AppFooter } from './app-footer.component';
import { LayoutService } from '@cheetah/layout/data-access';

@Component({
    selector: 'app-layout',
    standalone: true,
    imports: [CommonModule, AppTopbar, AppSidebar, RouterModule, AppFooter],
    template: `
        <div class="layout-wrapper" [ngClass]="containerClass">
            <app-topbar></app-topbar>
            <app-sidebar></app-sidebar>
            <div class="layout-main-container">
                <div class="layout-main">
                    <router-outlet></router-outlet>
                </div>
                <app-footer></app-footer>
            </div>
            <div class="layout-mask animate-fadein"></div>
        </div>
    `
})
export class AppLayout {
    private overlayMenuOpenSubscription!: Subscription;
    private menuOutsideClickListener: (() => void) | null = null;

    @ViewChild(AppSidebar) appSidebar!: AppSidebar;
    @ViewChild(AppTopbar)  appTopBar!: AppTopbar;

    constructor(
        public layoutService: LayoutService,
        public renderer: Renderer2,
        public router: Router
    ) {
        this.overlayMenuOpenSubscription = this.layoutService.overlayOpen$.subscribe(() => {
            if (!this.menuOutsideClickListener) {
                this.menuOutsideClickListener = this.renderer.listen('document', 'click', (event: MouseEvent) => {
                    if (this.isOutsideClicked(event)) this.hideMenu();
                });
            }
            if (this.layoutService.layoutState().staticMenuMobileActive) this.blockBodyScroll();
        });

        this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
            this.hideMenu();
        });
    }

    isOutsideClicked(event: MouseEvent): boolean {
        const sidebarEl  = document.querySelector('.layout-sidebar');
        const topbarEl   = document.querySelector('.layout-menu-button');
        const target     = event.target as Node;
        return !(
            sidebarEl?.isSameNode(target) || sidebarEl?.contains(target) ||
            topbarEl?.isSameNode(target)  || topbarEl?.contains(target)
        );
    }

    hideMenu(): void {
        this.layoutService.layoutState.update(prev => ({
            ...prev, overlayMenuActive: false, staticMenuMobileActive: false, menuHoverActive: false
        }));
        if (this.menuOutsideClickListener) {
            this.menuOutsideClickListener();
            this.menuOutsideClickListener = null;
        }
        this.unblockBodyScroll();
    }

    blockBodyScroll(): void   { document.body.classList.add('blocked-scroll'); }
    unblockBodyScroll(): void { document.body.classList.remove('blocked-scroll'); }

    get containerClass(): Record<string, boolean> {
        return {
            'layout-overlay':         this.layoutService.layoutConfig().menuMode === 'overlay',
            'layout-static':          this.layoutService.layoutConfig().menuMode === 'static',
            'layout-static-inactive': !!this.layoutService.layoutState().staticMenuDesktopInactive && this.layoutService.layoutConfig().menuMode === 'static',
            'layout-overlay-active':  !!this.layoutService.layoutState().overlayMenuActive,
            'layout-mobile-active':   !!this.layoutService.layoutState().staticMenuMobileActive,
        };
    }

    ngOnDestroy(): void {
        this.overlayMenuOpenSubscription?.unsubscribe();
        if (this.menuOutsideClickListener) this.menuOutsideClickListener();
    }
}
