import { MenuItem } from '@cheetah/shared/core';

export function filterByRoles(items: MenuItem[], roles: string[]): MenuItem[] {
  return items
    .filter(item =>
      !item.requiredRoles?.length ||
      item.requiredRoles.some(r => roles.includes(r))
    )
    .map(item => ({
      ...item,
      children: filterByRoles(item.children ?? [], roles),
    }))
    .sort((a, b) => a.order - b.order);
}

// Конвертирует MenuItem в формат для Sakai AppMenu
// Sakai ожидает: { label, icon, routerLink, items?: [...] }
export function toSakaiMenuItem(item: MenuItem): Record<string, unknown> {
  return {
    label:      item.label,
    icon:       item.icon,
    routerLink: item.routerLink,
    url:        item.url,
    target:     item.target,
    disabled:   item.disabled,
    separator:  item.separator,
    items:      item.children?.length
      ? item.children.map(toSakaiMenuItem)
      : undefined,
  };
}
