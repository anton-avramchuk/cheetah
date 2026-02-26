import { MenuContributor, MenuConfigurationContext, StandardMenus } from '@cheetah/shared/core';

export class UserMenuContributor implements MenuContributor {
  order = 1000;

  configureMenu(ctx: MenuConfigurationContext): void {
    ctx.getOrCreate(StandardMenus.USER)
       .addItem({
         id:       'logout',
         label:    'Выйти',
         icon:     'pi pi-sign-out',
         order:    100,
         url:      '/logout',
       });
  }
}
