import { InjectionToken } from '@angular/core';

export const StandardMenus = {
  MAIN:     'main',
  USER:     'user',
  ADMIN:    'admin',
  SETTINGS: 'settings',
} as const;

export type StandardMenuName = typeof StandardMenus[keyof typeof StandardMenus];

export interface MenuItem {
  id: string;
  label: string;
  icon?: string;
  routerLink?: string[];
  url?: string;
  target?: string;
  requiredRoles?: string[];
  order: number;
  disabled?: boolean;
  separator?: boolean;
  children?: MenuItem[];
}

export interface MenuBuilder {
  addItem(item: MenuItem): MenuBuilder;
}

export interface MenuConfigurationContext {
  getOrCreate(menuName: string): MenuBuilder;
}

export interface MenuContributor {
  order?: number;
  configureMenu(context: MenuConfigurationContext): void;
}

export const MENU_CONTRIBUTOR = new InjectionToken<MenuContributor[]>('MENU_CONTRIBUTOR');
