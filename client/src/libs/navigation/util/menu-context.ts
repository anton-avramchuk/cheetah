import { MenuItem, MenuBuilder, MenuConfigurationContext } from '@cheetah/shared/core';

class MenuBuilderImpl implements MenuBuilder {
  private readonly items: MenuItem[] = [];

  addItem(item: MenuItem): MenuBuilder {
    this.items.push(item);
    return this;
  }

  getItems(): MenuItem[] {
    return this.items;
  }
}

export class MenuConfigurationContextImpl implements MenuConfigurationContext {
  private readonly menus = new Map<string, MenuBuilderImpl>();

  getOrCreate(name: string): MenuBuilder {
    if (!this.menus.has(name)) {
      this.menus.set(name, new MenuBuilderImpl());
    }
    return this.menus.get(name)!;
  }

  getMenus(): Record<string, MenuItem[]> {
    return Object.fromEntries(
      Array.from(this.menus.entries()).map(([k, v]) => [k, v.getItems()])
    );
  }
}
