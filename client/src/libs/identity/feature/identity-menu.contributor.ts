import { MenuContributor, MenuConfigurationContext, StandardMenus } from '@cheetah/shared/core';

export class IdentityMenuContributor implements MenuContributor {
    order = 100;

    configureMenu(ctx: MenuConfigurationContext): void {
        ctx.getOrCreate(StandardMenus.MAIN).addItem({
            id: 'admin', label: 'Пользователи', icon: 'pi pi-fw pi-shield',
            routerLink: ['/admin'], requiredRoles: ['Admin'], order: 100,
        });
    }
}
