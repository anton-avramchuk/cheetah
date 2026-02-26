import { Component, computed, inject } from '@angular/core';
import { TieredMenuModule } from 'primeng/tieredmenu';
import { AvatarModule } from 'primeng/avatar';
import type { MenuItem as PrimeMenuItem } from 'primeng/api';
import { AuthStore } from '@cheetah/auth/data-access';
import { MenuService } from '@cheetah/navigation/data-access';
import { StandardMenus } from '@cheetah/shared/core';

@Component({
  selector: 'crm-user-info-widget',
  standalone: true,
  imports: [TieredMenuModule, AvatarModule],
  template: `
    @if (auth.isAuthenticated()) {
      <div class="crm-user-info" (click)="menu.toggle($event)">
        <p-avatar
          [label]="auth.avatarLetter()"
          styleClass="crm-user-avatar"
          shape="circle"
        />
        <span class="crm-user-name">{{ auth.displayName() }}</span>
        <i class="pi pi-angle-down crm-user-chevron"></i>
      </div>
      <p-tieredMenu #menu [model]="primeMenuItems()" [popup]="true" />
    }
  `,
})
export class UserInfoWidgetComponent {
  readonly auth    = inject(AuthStore);
  readonly menuSvc = inject(MenuService);

  readonly primeMenuItems = computed<PrimeMenuItem[]>(() => {
    const items = this.menuSvc.menus()[StandardMenus.USER] ?? [];
    return items.map(item => ({
      label:      item.label,
      icon:       item.icon,
      routerLink: item.routerLink,
      url:        item.id === 'logout' ? undefined : item.url,
      target:     item.target,
      disabled:   item.disabled,
      command:    item.id === 'logout' ? () => this.auth.logout() : undefined,
    }));
  });
}
