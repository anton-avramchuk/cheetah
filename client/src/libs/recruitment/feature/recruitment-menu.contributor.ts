import { MenuContributor, MenuConfigurationContext, StandardMenus } from '@cheetah/shared/core';

export class RecruitmentMenuContributor implements MenuContributor {
    order = 10;

    configureMenu(ctx: MenuConfigurationContext): void {
        ctx.getOrCreate(StandardMenus.MAIN)
            .addItem({ id: 'vacancies',  label: 'Вакансии',  icon: 'pi pi-fw pi-briefcase', routerLink: ['/vacancies'],  order: 1 })
            .addItem({ id: 'candidates', label: 'Кандидаты', icon: 'pi pi-fw pi-users',     routerLink: ['/candidates'], order: 2 })
            .addItem({ id: 'tasks',      label: 'Задачи',    icon: 'pi pi-fw pi-check-square', routerLink: ['/tasks'],   order: 3 });
    }
}
