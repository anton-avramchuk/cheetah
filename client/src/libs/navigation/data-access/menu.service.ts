import { Injectable, inject, computed, Signal } from '@angular/core';
import { MENU_CONTRIBUTOR, MenuItem } from '@cheetah/shared/core';
import { MenuConfigurationContextImpl, filterByRoles } from '@cheetah/navigation/util';
import { AuthStore } from '@cheetah/auth/data-access';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly contributors = inject(MENU_CONTRIBUTOR, { optional: true }) ?? [];
  private readonly authStore    = inject(AuthStore);

  // Computed signal: автоматически пересчитывается при смене ролей
  readonly menus: Signal<Record<string, MenuItem[]>> = computed(() => {
    const roles = this.authStore.roles();
    return this.buildMenus(roles);
  });

  private buildMenus(roles: string[]): Record<string, MenuItem[]> {
    const ctx = new MenuConfigurationContextImpl();

    [...this.contributors]
      .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
      .forEach(c => c.configureMenu(ctx));

    const rawMenus = ctx.getMenus();
    return Object.fromEntries(
      Object.entries(rawMenus).map(([name, items]) => [
        name,
        filterByRoles(items, roles),
      ])
    );
  }
}
