# Cheetah CRM — Angular Migration: Детальная инструкция по реализации

> Этот документ — пошаговый cookbook для реализации Angular фронтенда.
> Читать сверху вниз, выполнять команды в порядке следования.

---

## Содержание

1. [Предварительные требования](#1-предварительные-требования)
2. [Создание Nx workspace](#2-создание-nx-workspace)
3. [Конфигурация workspace](#3-конфигурация-workspace)
4. [libs/shared/core — базовые контракты](#4-libssharedcore)
5. [libs/auth — аутентификация](#5-libsauth)
6. [libs/navigation — меню система](#6-libsnavigation)
7. [libs/layout — Sakai-NG based layout](#7-libslayout)
8. [OpenAPI Codegen pipeline](#8-openapi-codegen-pipeline)
9. [Паттерн feature модуля (на примере recruitment)](#9-паттерн-feature-модуля)
10. [apps/crm-shell — сборка всего вместе](#10-appscrm-shell)
11. [Стилизация PrimeNG](#11-стилизация-primeng)
12. [Тестирование](#12-тестирование)
13. [Чеклист реализации](#13-чеклист-реализации)

---

## 1. Предварительные требования

```bash
node --version   # >= 20.x
npm --version    # >= 10.x

npm i -g nx@latest
npm i -g @angular/cli@latest
npm i -g @openapitools/openapi-generator-cli
```

---

## 2. Создание Nx workspace

```bash
# Создать в корне проекта рядом с /src
cd E:/Projects/cheetah

npx create-nx-workspace@latest angular \
  --preset=angular-standalone \
  --appName=crm-shell \
  --style=scss \
  --routing=true \
  --unitTestRunner=jest \
  --e2eTestRunner=playwright \
  --bundler=esbuild \
  --ssr=false \
  --nxCloud=skip

cd angular
```

### Установить зависимости

```bash
# PrimeNG + иконки
npm i primeng @primeng/themes primeicons

# NgRx Signals Store
npm i @ngrx/signals @ngrx/effects @ngrx/store @ngrx/store-devtools

# HTTP + Auth
npm i jwt-decode

# OpenAPI codegen
npm i -D @openapitools/openapi-generator-cli

# Nx плагины
npm i -D @nx/angular @nx/js @nx/playwright
```

---

## 3. Конфигурация workspace

### `nx.json`

```json
{
  "$schema": "./node_modules/nx/schemas/nx-schema.json",
  "defaultBase": "main",
  "namedInputs": {
    "default": ["{projectRoot}/**/*", "sharedGlobals"],
    "production": ["default", "!{projectRoot}/**/?(*.)+(spec|test).[jt]s?(x)"]
  },
  "targetDefaults": {
    "build":    { "dependsOn": ["^build"], "inputs": ["production", "^production"] },
    "test":     { "inputs": ["default", "^production"] },
    "generate": { "cache": false }
  }
}
```

### `tsconfig.base.json` — пути для всех libs

```json
{
  "compilerOptions": {
    "paths": {
      "@cheetah/shared/core":             ["libs/shared/core/src/index.ts"],
      "@cheetah/shared/ui":               ["libs/shared/ui/src/index.ts"],
      "@cheetah/shared/util-auth":        ["libs/shared/util-auth/src/index.ts"],
      "@cheetah/auth/data-access":        ["libs/auth/data-access/src/index.ts"],
      "@cheetah/auth/feature-login":      ["libs/auth/feature-login/src/index.ts"],
      "@cheetah/auth/interceptors":       ["libs/auth/interceptors/src/index.ts"],
      "@cheetah/navigation/data-access":  ["libs/navigation/data-access/src/index.ts"],
      "@cheetah/navigation/util":         ["libs/navigation/util/src/index.ts"],
      "@cheetah/layout/feature-shell":    ["libs/layout/feature-shell/src/index.ts"],
      "@cheetah/layout/feature-header":   ["libs/layout/feature-header/src/index.ts"],
      "@cheetah/layout/feature-sidebar":  ["libs/layout/feature-sidebar/src/index.ts"],
      "@cheetah/layout/data-access":      ["libs/layout/data-access/src/index.ts"],
      "@cheetah/recruitment/feature":     ["libs/recruitment/feature/src/index.ts"],
      "@cheetah/recruitment/data-access": ["libs/recruitment/data-access/src/index.ts"],
      "@cheetah/recruitment/api-client":  ["libs/recruitment/api-client/src/index.ts"],
      "@cheetah/identity/feature":        ["libs/identity/feature/src/index.ts"],
      "@cheetah/identity/data-access":    ["libs/identity/data-access/src/index.ts"],
      "@cheetah/identity/api-client":     ["libs/identity/api-client/src/index.ts"],
      "@cheetah/candidates/feature":      ["libs/candidates/feature/src/index.ts"],
      "@cheetah/candidates/data-access":  ["libs/candidates/data-access/src/index.ts"],
      "@cheetah/candidates/api-client":   ["libs/candidates/api-client/src/index.ts"],
      "@cheetah/vacancy-tasks/feature":   ["libs/vacancy-tasks/feature/src/index.ts"],
      "@cheetah/vacancy-tasks/data-access":["libs/vacancy-tasks/data-access/src/index.ts"],
      "@cheetah/vacancy-tasks/api-client":["libs/vacancy-tasks/api-client/src/index.ts"]
    }
  }
}
```

### Создать все lib директории командами Nx

```bash
# shared
nx g @nx/angular:library shared-core         --directory=libs/shared/core          --standalone --no-module --tags="type:util,scope:shared"
nx g @nx/angular:library shared-ui           --directory=libs/shared/ui            --standalone --no-module --tags="type:ui,scope:shared"
nx g @nx/angular:library shared-util-auth    --directory=libs/shared/util-auth     --standalone --no-module --tags="type:util,scope:shared"

# auth
nx g @nx/angular:library auth-data-access    --directory=libs/auth/data-access     --standalone --no-module --tags="type:data-access,scope:auth"
nx g @nx/angular:library auth-feature-login  --directory=libs/auth/feature-login   --standalone --no-module --tags="type:feature,scope:auth"
nx g @nx/angular:library auth-interceptors   --directory=libs/auth/interceptors    --standalone --no-module --tags="type:util,scope:auth"

# navigation
nx g @nx/angular:library nav-util            --directory=libs/navigation/util       --standalone --no-module --tags="type:util,scope:shared"
nx g @nx/angular:library nav-data-access     --directory=libs/navigation/data-access --standalone --no-module --tags="type:data-access,scope:shared"

# layout
nx g @nx/angular:library layout-data-access  --directory=libs/layout/data-access   --standalone --no-module --tags="type:data-access,scope:shared"
nx g @nx/angular:library layout-header       --directory=libs/layout/feature-header --standalone --no-module --tags="type:feature,scope:shared"
nx g @nx/angular:library layout-sidebar      --directory=libs/layout/feature-sidebar --standalone --no-module --tags="type:feature,scope:shared"
nx g @nx/angular:library layout-shell        --directory=libs/layout/feature-shell  --standalone --no-module --tags="type:feature,scope:shared"

# recruitment
nx g @nx/js:library recruitment-api-client   --directory=libs/recruitment/api-client --tags="type:data-access,scope:recruitment"
nx g @nx/angular:library recruitment-da      --directory=libs/recruitment/data-access --standalone --no-module --tags="type:data-access,scope:recruitment"
nx g @nx/angular:library recruitment-feature --directory=libs/recruitment/feature    --standalone --no-module --tags="type:feature,scope:recruitment"

# identity (аналогично)
nx g @nx/js:library identity-api-client      --directory=libs/identity/api-client   --tags="type:data-access,scope:identity"
nx g @nx/angular:library identity-da         --directory=libs/identity/data-access  --standalone --no-module --tags="type:data-access,scope:identity"
nx g @nx/angular:library identity-feature    --directory=libs/identity/feature      --standalone --no-module --tags="type:feature,scope:identity"

# candidates
nx g @nx/js:library candidates-api-client    --directory=libs/candidates/api-client --tags="type:data-access,scope:candidates"
nx g @nx/angular:library candidates-da       --directory=libs/candidates/data-access --standalone --no-module --tags="type:data-access,scope:candidates"
nx g @nx/angular:library candidates-feature  --directory=libs/candidates/feature    --standalone --no-module --tags="type:feature,scope:candidates"

# vacancy-tasks
nx g @nx/js:library vt-api-client            --directory=libs/vacancy-tasks/api-client --tags="type:data-access,scope:vacancy-tasks"
nx g @nx/angular:library vt-da               --directory=libs/vacancy-tasks/data-access --standalone --no-module --tags="type:data-access,scope:vacancy-tasks"
nx g @nx/angular:library vt-feature          --directory=libs/vacancy-tasks/feature    --standalone --no-module --tags="type:feature,scope:vacancy-tasks"
```

### ESLint boundary rules — `eslint.config.mjs`

```js
// В секцию rules добавить:
"@nx/enforce-module-boundaries": ["error", {
  "enforceBuildableLibDependency": true,
  "depConstraints": [
    { "sourceTag": "type:app",         "onlyDependOnLibsWithTags": ["type:feature","type:data-access","type:ui","type:util"] },
    { "sourceTag": "type:feature",     "onlyDependOnLibsWithTags": ["type:data-access","type:ui","type:util"] },
    { "sourceTag": "type:data-access", "onlyDependOnLibsWithTags": ["type:util","scope:shared"] },
    { "sourceTag": "type:ui",          "onlyDependOnLibsWithTags": ["type:util","scope:shared"] },
    { "sourceTag": "type:util",        "onlyDependOnLibsWithTags": [] }
  ]
}]
```

---

## 4. `libs/shared/core`

Нет зависимостей от Angular, только TypeScript. Это основа всей системы.

### `libs/shared/core/src/lib/tokens.ts`

```typescript
import { InjectionToken } from '@angular/core';
import type { MenuContributor } from './menu-contributor';
import type { HeaderWidget } from './header-widget';
import type { AppConfig } from './app-config';

export const MENU_CONTRIBUTOR = new InjectionToken<MenuContributor[]>('MENU_CONTRIBUTOR');
export const HEADER_WIDGET    = new InjectionToken<HeaderWidget[]>('HEADER_WIDGET');
export const APP_CONFIG       = new InjectionToken<AppConfig>('APP_CONFIG');
export const BASE_PATH        = new InjectionToken<string>('BASE_PATH');
```

### `libs/shared/core/src/lib/menu-contributor.ts`

```typescript
export const StandardMenus = {
  MAIN:     'main',
  USER:     'user',
  ADMIN:    'admin',
  SETTINGS: 'settings',
} as const;

export type StandardMenu = typeof StandardMenus[keyof typeof StandardMenus];

export interface MenuItem {
  id: string;
  label: string;
  icon?: string;          // 'pi pi-briefcase' формат PrimeNG
  routerLink?: string[];
  url?: string;
  target?: string;
  requiredRoles?: string[];
  order: number;
  disabled?: boolean;
  children?: MenuItem[];
}

export interface MenuBuilder {
  addItem(item: Omit<MenuItem, 'children'> & { children?: MenuItem[] }): MenuBuilder;
}

export interface MenuConfigurationContext {
  getOrCreate(menuName: string): MenuBuilder;
}

export interface MenuContributor {
  order?: number;   // порядок выполнения, меньше = раньше
  configureMenu(context: MenuConfigurationContext): void;
}
```

### `libs/shared/core/src/lib/header-widget.ts`

```typescript
import { Type } from '@angular/core';

export interface HeaderWidget {
  component: Type<unknown>;
  inputs?: Record<string, unknown>;
  order: number;
}
```

### `libs/shared/core/src/lib/app-config.ts`

```typescript
export interface AppConfig {
  appName: string;
  logoUrl?: string;
  homeUrl: string;
  apiUrl: string;
}
```

### `libs/shared/core/src/lib/standard-menus.ts`

```typescript
export const StandardMenus = {
  MAIN:     'main',
  USER:     'user',
  ADMIN:    'admin',
  SETTINGS: 'settings',
} as const;
```

### `libs/shared/core/src/index.ts` (публичное API)

```typescript
export * from './lib/tokens';
export * from './lib/menu-contributor';
export * from './lib/header-widget';
export * from './lib/app-config';
export * from './lib/standard-menus';
```

---

## 5. `libs/auth`

### `libs/shared/util-auth/src/lib/jwt.utils.ts`

```typescript
export interface JwtPayload {
  sub: string;
  email?: string;
  unique_name?: string;
  role?: string | string[];
  exp: number;
  [key: string]: unknown;
}

export interface ParsedUser {
  id: string;
  displayName: string;
  email: string;
  roles: string[];
}

export function parseJwt(token: string): ParsedUser {
  const payload = JSON.parse(atob(token.split('.')[1])) as JwtPayload;
  const roles = Array.isArray(payload['role'])
    ? payload['role']
    : payload['role'] ? [payload['role']] : [];

  return {
    id:          payload['sub'] ?? '',
    displayName: payload['unique_name'] ?? payload['email'] ?? 'User',
    email:       payload['email'] ?? '',
    roles,
  };
}

export function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1])) as JwtPayload;
    return Date.now() >= payload['exp'] * 1000;
  } catch {
    return true;
  }
}
```

### `libs/auth/data-access/src/lib/token-storage.service.ts`

```typescript
import { Injectable } from '@angular/core';

const TOKEN_KEY = 'crm_auth_token';

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  get(): string | null           { return localStorage.getItem(TOKEN_KEY); }
  set(token: string): void       { localStorage.setItem(TOKEN_KEY, token); }
  remove(): void                 { localStorage.removeItem(TOKEN_KEY); }
}
```

### `libs/auth/data-access/src/lib/auth.store.ts`

```typescript
import { inject }        from '@angular/core';
import { Router }        from '@angular/router';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { computed }      from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TokenStorage }  from './token-storage.service';
import { parseJwt, isTokenExpired, ParsedUser } from '@cheetah/shared/util-auth';

// Тип для API клиента логина — будет заменён на сгенерированный
export interface LoginRequest  { username: string; password: string; }
export interface LoginResponse { accessToken: string; }

interface AuthState {
  token:   string | null;
  user:    ParsedUser | null;
  loading: boolean;
  error:   string | null;
}

export const AuthStore = signalStore(
  { providedIn: 'root' },

  withState<AuthState>({
    token:   null,
    user:    null,
    loading: false,
    error:   null,
  }),

  withComputed(({ token, user }) => ({
    isAuthenticated: computed(() => !!token()),
    roles:           computed(() => user()?.roles ?? []),
    displayName:     computed(() => user()?.displayName ?? ''),
    avatarLetter:    computed(() => {
      const name = user()?.displayName ?? '';
      return name.length > 0 ? name[0].toUpperCase() : '?';
    }),
  })),

  withMethods((store) => {
    const tokenStorage = inject(TokenStorage);
    const router       = inject(Router);

    return {
      // Вызывается после успешного ответа с токеном от API
      handleLoginSuccess(token: string): void {
        tokenStorage.set(token);
        const user = parseJwt(token);
        patchState(store, { token, user, loading: false, error: null });
        router.navigate(['/']);
      },

      setLoading(loading: boolean): void {
        patchState(store, { loading });
      },

      setError(error: string): void {
        patchState(store, { error, loading: false });
      },

      async logout(): Promise<void> {
        tokenStorage.remove();
        patchState(store, { token: null, user: null, error: null });
        await router.navigate(['/login']);
      },

      // Восстановление сессии при старте приложения
      restore(): void {
        const token = tokenStorage.get();
        if (token && !isTokenExpired(token)) {
          const user = parseJwt(token);
          patchState(store, { token, user });
        }
      },
    };
  })
);
```

### `libs/auth/interceptors/src/lib/jwt.interceptor.ts`

```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { inject }            from '@angular/core';
import { AuthStore }         from '@cheetah/auth/data-access';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthStore).token();
  if (!token) return next(req);
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
```

### `libs/auth/interceptors/src/lib/unauthorized.interceptor.ts`

```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { inject }            from '@angular/core';
import { tap }               from 'rxjs/operators';
import { AuthStore }         from '@cheetah/auth/data-access';

export const unauthorizedInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  // Не перехватываем ошибки самого логин-эндпоинта
  if (req.url.includes('/auth/login')) return next(req);

  return next(req).pipe(
    tap({ error: (err) => { if (err.status === 401) authStore.logout(); } })
  );
};
```

### `libs/auth/feature-login/src/lib/login.component.ts`

```typescript
import { Component, inject } from '@angular/core';
import { CommonModule }      from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CardModule }        from 'primeng/card';
import { InputTextModule }   from 'primeng/inputtext';
import { ButtonModule }      from 'primeng/button';
import { MessageModule }     from 'primeng/message';
import { AuthStore }         from '@cheetah/auth/data-access';
// ВАЖНО: после codegen заменить на сгенерированный AuthApiClient
import { AuthApiClient }     from './auth-api-client.stub';

@Component({
  selector: 'crm-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CardModule, InputTextModule, ButtonModule, MessageModule],
  template: `
    <div class="crm-center-layout">
      <p-card header="Вход в систему" styleClass="crm-login-card">
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="field">
            <label for="username">Логин</label>
            <input pInputText id="username" formControlName="username" />
          </div>
          <div class="field">
            <label for="password">Пароль</label>
            <input pInputText type="password" id="password" formControlName="password" />
          </div>
          @if (auth.error()) {
            <p-message severity="error" [text]="auth.error()!" />
          }
          <p-button
            type="submit"
            label="Войти"
            [loading]="auth.loading()"
            [disabled]="form.invalid"
            styleClass="w-full mt-3"
          />
        </form>
      </p-card>
    </div>
  `
})
export class LoginComponent {
  auth    = inject(AuthStore);
  api     = inject(AuthApiClient);
  fb      = inject(FormBuilder);

  form = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  async submit(): Promise<void> {
    if (this.form.invalid) return;
    const { username, password } = this.form.value;
    this.auth.setLoading(true);
    try {
      // AuthApiClient.login возвращает Observable<LoginResponse>
      const response = await firstValueFrom(
        this.api.login({ username: username!, password: password! })
      );
      this.auth.handleLoginSuccess(response.accessToken);
    } catch {
      this.auth.setError('Неверный логин или пароль');
    }
  }
}
```

### `libs/auth/feature-login/src/lib/user-info-widget.component.ts`

```typescript
import { Component, computed, inject } from '@angular/core';
import { CommonModule }  from '@angular/common';
import { TieredMenuModule } from 'primeng/tieredmenu';
import { ButtonModule }  from 'primeng/button';
import { AvatarModule }  from 'primeng/avatar';
import { AuthStore }     from '@cheetah/auth/data-access';
import { MenuService }   from '@cheetah/navigation/data-access';
import { StandardMenus } from '@cheetah/shared/core';
import type { MenuItem as PrimeMenuItem } from 'primeng/api';

@Component({
  selector: 'crm-user-info-widget',
  standalone: true,
  imports: [CommonModule, TieredMenuModule, ButtonModule, AvatarModule],
  template: `
    @if (auth.isAuthenticated()) {
      <div class="crm-user-info" (click)="menu.toggle($event)">
        <p-avatar
          [label]="auth.avatarLetter()"
          styleClass="crm-user-avatar"
          shape="circle"
        />
        <span class="crm-user-name">{{ auth.displayName() }}</span>
        <i class="pi pi-chevron-down crm-user-chevron"></i>
      </div>
      <p-tieredMenu #menu [model]="primeMenuItems()" [popup]="true" />
    }
  `
})
export class UserInfoWidgetComponent {
  auth       = inject(AuthStore);
  menuSvc    = inject(MenuService);

  primeMenuItems = computed<PrimeMenuItem[]>(() => {
    const items = this.menuSvc.menus()[StandardMenus.USER] ?? [];
    return items.map(item => ({
      label:        item.label,
      icon:         item.icon,
      routerLink:   item.routerLink,
      url:          item.url,
      target:       item.target,
      disabled:     item.disabled,
      command:      item.id === 'logout' ? () => this.auth.logout() : undefined,
    }));
  });
}
```

### `libs/auth/data-access/src/lib/user-menu.contributor.ts`

```typescript
import { MenuContributor, MenuConfigurationContext, StandardMenus } from '@cheetah/shared/core';

export class UserMenuContributor implements MenuContributor {
  order = 1000; // последним, чтобы logout был внизу

  configureMenu(ctx: MenuConfigurationContext): void {
    ctx.getOrCreate(StandardMenus.USER)
       .addItem({
         id:    'logout',
         label: 'Выйти',
         icon:  'pi pi-sign-out',
         order: 100,
       });
  }
}
```

**Регистрация в providers shell:**
```typescript
{ provide: MENU_CONTRIBUTOR, useClass: UserMenuContributor, multi: true },
{ provide: HEADER_WIDGET, useValue: { component: UserInfoWidgetComponent, order: 100 }, multi: true },
```

### Guards

```typescript
// libs/auth/data-access/src/lib/auth.guard.ts
import { inject }       from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore }    from './auth.store';

export const authGuard: CanActivateFn = () => {
  const auth   = inject(AuthStore);
  const router = inject(Router);
  return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

// libs/auth/data-access/src/lib/role.guard.ts
export const roleGuard = (requiredRoles: string[]): CanActivateFn => () => {
  const auth   = inject(AuthStore);
  const router = inject(Router);
  const roles  = auth.roles();
  return requiredRoles.some(r => roles.includes(r))
    ? true
    : router.createUrlTree(['/']);
};
```

---

## 6. `libs/navigation`

### `libs/navigation/util/src/lib/menu-configuration-context.ts`

```typescript
import { MenuItem, MenuBuilder, MenuConfigurationContext } from '@cheetah/shared/core';

class MenuBuilderImpl implements MenuBuilder {
  private items: MenuItem[] = [];

  addItem(item: MenuItem): MenuBuilder {
    this.items.push(item);
    return this;
  }

  getItems(): MenuItem[] {
    return this.items;
  }
}

export class MenuConfigurationContextImpl implements MenuConfigurationContext {
  private menus = new Map<string, MenuBuilderImpl>();

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
```

### `libs/navigation/util/src/lib/menu-filter.util.ts`

```typescript
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

// Конвертер для PrimeNG PanelMenu / TieredMenu
export function toPrimeMenuItem(item: MenuItem): Record<string, unknown> {
  return {
    label:      item.label,
    icon:       item.icon,
    routerLink: item.routerLink,
    url:        item.url,
    target:     item.target,
    disabled:   item.disabled,
    items:      item.children?.map(toPrimeMenuItem),
  };
}
```

### `libs/navigation/data-access/src/lib/menu.service.ts`

```typescript
import { Injectable, inject, computed, Signal } from '@angular/core';
import { MENU_CONTRIBUTOR, MenuItem }           from '@cheetah/shared/core';
import { MenuConfigurationContextImpl }         from '@cheetah/navigation/util';
import { filterByRoles }                        from '@cheetah/navigation/util';
import { AuthStore }                            from '@cheetah/auth/data-access';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly contributors = inject(MENU_CONTRIBUTOR, { optional: true }) ?? [];
  private readonly authStore    = inject(AuthStore);

  // Computed signal: автоматически пересчитывается когда меняются роли
  readonly menus: Signal<Record<string, MenuItem[]>> = computed(() => {
    const roles = this.authStore.roles(); // отслеживаем этот signal
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
```

---

## 7. `libs/layout` — на основе Sakai-NG

> **Главное правило**: layout максимально копируется из `C:\Projects\sakai-ng\src\app\layout\`.
> Sakai — это официальный бесплатный шаблон от PrimeTek для PrimeNG.
> Мы не изобретаем колесо — берём их компоненты, сервис, SCSS и адаптируем.

### Откуда копировать файлы

```
C:\Projects\sakai-ng\src\app\layout\
├── component/
│   ├── app.layout.ts          → libs/layout/feature-shell/src/lib/app-layout.component.ts
│   ├── app.topbar.ts          → libs/layout/feature-header/src/lib/app-topbar.component.ts
│   ├── app.sidebar.ts         → libs/layout/feature-sidebar/src/lib/app-sidebar.component.ts
│   ├── app.menu.ts            → libs/layout/feature-sidebar/src/lib/app-menu.component.ts
│   ├── app.menuitem.ts        → libs/layout/feature-sidebar/src/lib/app-menuitem.component.ts
│   ├── app.footer.ts          → libs/layout/feature-shell/src/lib/app-footer.component.ts
│   ├── app.configurator.ts    → libs/layout/feature-shell/src/lib/app-configurator.component.ts
│   └── app.floatingconfigurator.ts → libs/layout/feature-shell/src/lib/app-floating-configurator.component.ts
└── service/
    └── layout.service.ts      → libs/layout/data-access/src/lib/layout.service.ts

C:\Projects\sakai-ng\src\assets\layout\
    layout.scss + все _*.scss  → libs/layout/feature-shell/src/styles/
```

### Шаг 1 — Скопировать все файлы как есть

```bash
# Компоненты
cp "C:/Projects/sakai-ng/src/app/layout/component/app.layout.ts"               libs/layout/feature-shell/src/lib/app-layout.component.ts
cp "C:/Projects/sakai-ng/src/app/layout/component/app.topbar.ts"               libs/layout/feature-header/src/lib/app-topbar.component.ts
cp "C:/Projects/sakai-ng/src/app/layout/component/app.sidebar.ts"              libs/layout/feature-sidebar/src/lib/app-sidebar.component.ts
cp "C:/Projects/sakai-ng/src/app/layout/component/app.menu.ts"                 libs/layout/feature-sidebar/src/lib/app-menu.component.ts
cp "C:/Projects/sakai-ng/src/app/layout/component/app.menuitem.ts"             libs/layout/feature-sidebar/src/lib/app-menuitem.component.ts
cp "C:/Projects/sakai-ng/src/app/layout/component/app.footer.ts"               libs/layout/feature-shell/src/lib/app-footer.component.ts
cp "C:/Projects/sakai-ng/src/app/layout/component/app.configurator.ts"         libs/layout/feature-shell/src/lib/app-configurator.component.ts
cp "C:/Projects/sakai-ng/src/app/layout/component/app.floatingconfigurator.ts" libs/layout/feature-shell/src/lib/app-floating-configurator.component.ts

# Сервис
cp "C:/Projects/sakai-ng/src/app/layout/service/layout.service.ts"             libs/layout/data-access/src/lib/layout.service.ts

# SCSS
cp -r "C:/Projects/sakai-ng/src/assets/layout/"  libs/layout/feature-shell/src/styles/
```

### Шаг 2 — Понять архитектуру LayoutService (Sakai)

LayoutService из Sakai используется вместо нашего `LayoutStore`. Он уже написан на Angular Signals.

```typescript
// libs/layout/data-access/src/lib/layout.service.ts (СКОПИРОВАНО ИЗ SAKAI — не изменять!)

interface LayoutConfig {
  preset:    string;   // 'Aura' | 'Lara' | 'Nora'
  primary:   string;   // 'emerald' | 'blue' | etc.
  surface:   string | null;
  darkTheme: boolean;
  menuMode:  string;   // 'static' | 'overlay'
}

interface LayoutState {
  staticMenuDesktopInactive: boolean;  // Свёрнуто на десктопе
  overlayMenuActive:         boolean;  // Оверлей открыт
  staticMenuMobileActive:    boolean;  // Мобильное меню открыто
  menuHoverActive:           boolean;
}

@Injectable({ providedIn: 'root' })
export class LayoutService {
  layoutConfig = signal<LayoutConfig>({
    preset: 'Aura', primary: 'emerald', surface: null,
    darkTheme: false, menuMode: 'static'
  });

  layoutState = signal<LayoutState>({
    staticMenuDesktopInactive: false,
    overlayMenuActive: false,
    staticMenuMobileActive: false,
    menuHoverActive: false,
  });

  // Вычисляемые сигналы
  isSidebarActive = computed(() =>
    this.layoutState().overlayMenuActive || this.layoutState().staticMenuMobileActive
  );
  isDarkTheme  = computed(() => this.layoutConfig().darkTheme);
  isOverlay    = computed(() => this.layoutConfig().menuMode === 'overlay');
  isDesktop    = () => window.innerWidth > 991;

  // Observable-стримы для реактивных событий
  menuSource$  = new Subject<MenuChangeEvent>();
  resetSource$ = new Subject<boolean>();
  overlayOpen$ = new Subject<void>();

  onMenuToggle(): void {
    if (this.isOverlay()) {
      this.layoutState.update(s => ({ ...s, overlayMenuActive: !s.overlayMenuActive }));
      this.overlayOpen$.next();
    } else if (this.isDesktop()) {
      this.layoutState.update(s => ({ ...s, staticMenuDesktopInactive: !s.staticMenuDesktopInactive }));
    } else {
      this.layoutState.update(s => ({ ...s, staticMenuMobileActive: !s.staticMenuMobileActive }));
    }
  }
}
```

**Почему не NgRx для layout?** Sakai использует чистые Angular Signals — это оптимально для layout-стейта, который не нужно персистить и логировать через redux DevTools. NgRx оставляем только для доменных данных (auth, vacancies, etc).

### Шаг 3 — AppLayout (главный контейнер)

Копируем `app.layout.ts` из Sakai и добавляем **только один** `canActivate: [authGuard]` в роутинге — сам компонент не трогаем.

```typescript
// app-layout.component.ts — СКОПИРОВАНО ИЗ SAKAI БЕЗ ИЗМЕНЕНИЙ
// Ключевая логика контейнера:
//
// 1. CSS классы на корневом div:
//    'layout-overlay'          — если menuMode === 'overlay'
//    'layout-static'           — если menuMode === 'static'
//    'layout-static-inactive'  — если staticMenuDesktopInactive
//    'layout-overlay-active'   — если overlayMenuActive
//    'layout-mobile-active'    — если staticMenuMobileActive
//
// 2. Закрытие меню при клике вне его:
//    @HostListener('document:click') → проверяет isOutsideClick
//
// 3. Закрытие при навигации:
//    router.events.pipe(filter(NavigationEnd)) → resetSources

@Component({
  selector: 'app-layout',
  template: `
    <div class="layout-wrapper" [ngClass]="containerClass">
      <app-topbar />
      <app-sidebar />
      <div class="layout-main-container">
        <div class="layout-main">
          <router-outlet />
        </div>
        <app-footer />
      </div>
      <app-configurator />
      <div class="layout-mask" (click)="hideMenu()"></div>
    </div>
  `
})
export class AppLayout { /* ... скопировано из Sakai */ }
```

### Шаг 4 — AppTopbar: добавляем HEADER_WIDGET

Это единственное **значительное изменение** относительно оригинального Sakai topbar.
В `.layout-topbar-actions` (правая часть) добавляем рендеринг виджетов через `ngComponentOutlet`.

```typescript
// libs/layout/feature-header/src/lib/app-topbar.component.ts
// ОСНОВА: скопировано из sakai-ng/src/app/layout/component/app.topbar.ts
// ИЗМЕНЕНИЕ: добавлен inject HEADER_WIDGET + ngComponentOutlet в layout-topbar-actions

import { Component, computed, inject } from '@angular/core';
import { CommonModule }  from '@angular/common';
import { RouterLink }    from '@angular/router';
import { ButtonModule }  from 'primeng/button';
import { StyleClassModule } from 'primeng/styleclass';
import { LayoutService } from '@cheetah/layout/data-access';
import { AppConfigurator } from './app-configurator.component';
import { HEADER_WIDGET }   from '@cheetah/shared/core';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, StyleClassModule, AppConfigurator],
  template: `
    <div class="layout-topbar">
      <div class="layout-topbar-logo-container">
        <!-- Кнопка гамбургера — БЕЗ ИЗМЕНЕНИЙ из Sakai -->
        <button
          class="layout-menu-button layout-topbar-action"
          (click)="layoutService.onMenuToggle()"
        >
          <i class="pi pi-bars"></i>
        </button>

        <!-- Логотип — меняем на APP_CONFIG -->
        <a [routerLink]="config.homeUrl" class="layout-topbar-logo">
          @if (config.logoUrl) {
            <img [src]="config.logoUrl" [alt]="config.appName" height="32" />
          } @else {
            <i class="pi pi-bolt" style="font-size: 2rem; color: var(--primary-color)"></i>
          }
          <span class="font-semibold text-xl">{{ config.appName }}</span>
        </a>
      </div>

      <!-- Правая часть: виджеты из HEADER_WIDGET токена -->
      <div class="layout-topbar-actions">
        @for (widget of sortedWidgets(); track widget.component) {
          <!--
            ngComponentOutlet — рендерит компонент инлайн, без обёрток.
            Сюда попадёт UserInfoWidgetComponent (auth) и любые будущие виджеты.
            Порядок определяется widget.order.
          -->
          <ng-container *ngComponentOutlet="widget.component; inputs: widget.inputs" />
        }
      </div>
    </div>
  `
})
export class AppTopbar {
  layoutService = inject(LayoutService);
  config        = inject(APP_CONFIG);

  private readonly widgets = inject(HEADER_WIDGET, { optional: true }) ?? [];
  sortedWidgets = computed(() => [...this.widgets].sort((a, b) => a.order - b.order));
}
```

### Шаг 5 — AppMenu: подключаем MenuService

Оригинальный `app.menu.ts` из Sakai имеет **захардкоженный** массив `model`. Заменяем его на сигнал из нашего `MenuService`.

```typescript
// libs/layout/feature-sidebar/src/lib/app-menu.component.ts
// ОСНОВА: скопировано из sakai-ng app.menu.ts
// ИЗМЕНЕНИЕ: model берётся из MenuService вместо захардкоженного массива

import { Component, computed, inject } from '@angular/core';
import { CommonModule }  from '@angular/common';
import { AppMenuitem }   from './app-menuitem.component';
import { MenuService }   from '@cheetah/navigation/data-access';
import { StandardMenus } from '@cheetah/shared/core';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule, AppMenuitem],
  template: `
    <ul class="layout-menu">
      @for (item of model(); track item.id; let i = $index) {
        @if (!item['separator']) {
          <li
            app-menuitem
            [item]="item"
            [index]="i"
            [root]="true"
          ></li>
        } @else {
          <li class="menu-separator"></li>
        }
      }
    </ul>
  `
})
export class AppMenu {
  private readonly menuSvc = inject(MenuService);

  // computed signal: автоматически реагирует на изменение ролей
  // MenuService.menus() пересчитывается при смене auth.roles()
  model = computed(() => this.toSakaiFormat(this.menuSvc.menus()[StandardMenus.MAIN] ?? []));

  private toSakaiFormat(items: MenuItem[]): SakaiMenuItem[] {
    // Sakai ожидает структуру { label, items: [...] } для корневых групп
    // Наш MenuItem плоский, поэтому оборачиваем в одну группу если нет детей
    return items.map(item => ({
      label:      item.label,
      icon:       item.icon,       // 'pi pi-briefcase'
      routerLink: item.routerLink,
      items:      item.children?.length ? this.toSakaiFormat(item.children) : undefined,
      separator:  false,
    }));
  }
}
```

**Формат MenuItem для Sakai** (как передавать из MenuContributor):

```typescript
// Плоский список (без вложенности) → один уровень меню
ctx.getOrCreate(StandardMenus.MAIN)
   .addItem({ id: 'vacancies', label: 'Вакансии', icon: 'pi pi-fw pi-briefcase', routerLink: ['/vacancies'], order: 1 });

// С детьми → раскрывающийся подраздел
ctx.getOrCreate(StandardMenus.MAIN)
   .addItem({
     id: 'recruitment', label: 'Рекрутинг', icon: 'pi pi-fw pi-users', order: 10,
     children: [
       { id: 'vacancies',  label: 'Вакансии',  icon: 'pi pi-fw pi-briefcase', routerLink: ['/vacancies'],  order: 1 },
       { id: 'candidates', label: 'Кандидаты', icon: 'pi pi-fw pi-user-plus', routerLink: ['/candidates'], order: 2 },
     ]
   });
```

### Шаг 6 — AppMenuitem (скопировать без изменений)

`app.menuitem.ts` — рекурсивный компонент с анимацией expand/collapse. **Копировать как есть**, он работает с любым форматом данных.

Ключевые моменты из Sakai которые нужно понимать:
- Использует `key` вида `"0-1-2"` для отслеживания состояния раскрытия
- Следит за роутом через `router.events` чтобы подсветить активный пункт
- Анимация: `400ms cubic-bezier(0.86, 0, 0.07, 1)` height 0↔*
- Атрибутный селектор `[app-menuitem]` — это фича Angular, не баг

### Шаг 7 — SCSS: копируем из Sakai полностью

```bash
# Копируем все SCSS файлы Sakai
cp -r "C:/Projects/sakai-ng/src/assets/layout/" angular/libs/layout/feature-shell/src/styles/
```

Структура после копирования:
```
libs/layout/feature-shell/src/styles/
├── layout.scss                 # Точка входа — @use все остальные
├── variables/
│   ├── _common.scss            # CSS custom properties (var(--p-*))
│   ├── _light.scss             # Переменные для светлой темы
│   └── _dark.scss              # Переменные для .app-dark класса
├── _mixins.scss                # @mixin focused(), focused-inset()
├── _preloading.scss            # Loader анимация
├── _core.scss                  # html, body, .layout-wrapper
├── _main.scss                  # .layout-main-container, .layout-main
├── _topbar.scss                # .layout-topbar, height: 4rem, z-index: 997
├── _menu.scss                  # .layout-sidebar (20rem), .layout-menu, анимации
├── _footer.scss                # .layout-footer
├── _responsive.scss            # breakpoint 992px, overlay/static поведение
├── _utils.scss                 # .card class, p-toast offset
└── _typography.scss            # Lato font, heading sizes
```

**В `apps/crm-shell/src/styles.scss` добавить:**
```scss
// Sakai layout styles — ДОЛЖЕН БЫТЬ ПЕРВЫМ
@use 'libs/layout/feature-shell/src/styles/layout' as *;

// Primeicons
@import "primeicons/primeicons.css";

// Наши кастомные overrides — ТОЛЬКО ПОСЛЕ SAKAI
// (переопределяем только то, что нужно изменить)
.layout-topbar-actions {
  gap: 0.5rem;
}
```

**Ключевые CSS классы Sakai которые надо знать:**

| Класс | Что делает |
|---|---|
| `.layout-wrapper.layout-static` | Статичное боковое меню |
| `.layout-wrapper.layout-overlay` | Оверлейное меню |
| `.layout-static-inactive` | Меню свёрнуто на десктопе |
| `.layout-overlay-active` | Оверлей открыт |
| `.layout-mobile-active` | Мобильное меню открыто |
| `.layout-sidebar` | Сайдбар, ширина 20rem, top: 6rem |
| `.layout-topbar` | Шапка, высота 4rem, fixed, z-index 997 |
| `.layout-main-container` | Основная область, padding-top: 6rem |
| `.layout-main-container` (static) | margin-left: 22rem |
| `.app-dark` на `<html>` | Тёмная тема |
| `.active-route` | Активный пункт меню |
| `.active-menuitem` | Раскрытый подменю |

### Шаг 8 — Dark mode

Sakai управляет тёмной темой через класс `app-dark` на `<html>`. Это уже реализовано в `LayoutService.toggleDarkMode()`.

```typescript
// В AppTopbar можно добавить кнопку переключения — берём из app.floatingconfigurator.ts
onDarkModeToggle(): void {
  this.layoutService.layoutConfig.update(c => ({ ...c, darkTheme: !c.darkTheme }));
}
```

PrimeNG настраивается в `app.config.ts`:
```typescript
providePrimeNG({
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.app-dark'  // Совпадает с Sakai
    }
  }
})
```

### Шаг 9 — Роуты: layout как parent route

```typescript
// libs/layout/feature-shell/src/lib/layout.routes.ts
import { Routes }  from '@angular/router';
import { AppLayout } from './app-layout.component';  // Sakai компонент
import { authGuard } from '@cheetah/auth/data-access';
import { roleGuard } from '@cheetah/auth/data-access';

export const LAYOUT_ROUTES: Routes = [
  {
    path: '',
    component: AppLayout,          // Sakai AppLayout как оболочка
    canActivate: [authGuard],      // Наш guard — единственное добавление
    children: [
      { path: '', redirectTo: 'vacancies', pathMatch: 'full' },
      {
        path: 'vacancies',
        loadChildren: () => import('@cheetah/recruitment/feature').then(m => m.RECRUITMENT_ROUTES),
      },
      {
        path: 'candidates',
        loadChildren: () => import('@cheetah/candidates/feature').then(m => m.CANDIDATES_ROUTES),
      },
      {
        path: 'tasks',
        loadChildren: () => import('@cheetah/vacancy-tasks/feature').then(m => m.VACANCY_TASKS_ROUTES),
      },
      {
        path: 'admin',
        canActivate: [roleGuard(['Admin'])],
        loadChildren: () => import('@cheetah/identity/feature').then(m => m.IDENTITY_ROUTES),
      },
    ],
  },
];
```

### Шаг 10 — Что НЕ трогать в Sakai

Следующие компоненты копировать **дословно, без изменений**:

| Файл | Причина |
|---|---|
| `app.sidebar.ts` | Простой враппер — менять нечего |
| `app.menuitem.ts` | Сложная логика анимаций и active state — проверено |
| `app.footer.ts` | Статичный контент |
| `app.configurator.ts` | Готовый theme switcher — это бонус бесплатно |
| `app.floatingconfigurator.ts` | Кнопка вызова конфигуратора + dark mode toggle |
| `layout.service.ts` | Вся логика состояния меню — не трогать |
| Все `_*.scss` файлы | Вся адаптивность и анимации — не переписывать |

### Шаг 11 — Итоговая схема связей

```
HEADER_WIDGET (multi token)
  └── UserInfoWidgetComponent (auth)
        ↓ inject
  AppTopbar.sortedWidgets (computed)
        ↓ *ngComponentOutlet (инлайн, без обёртки)
  .layout-topbar-actions

MENU_CONTRIBUTOR (multi token)
  ├── RecruitmentMenuContributor
  ├── IdentityMenuContributor
  └── UserMenuContributor
        ↓ inject all
  MenuService.menus (computed signal → реагирует на auth.roles())
        ↓ inject
  AppMenu.model (computed signal → toSakaiFormat)
        ↓ @for
  AppMenuitem (recursive, Sakai, без изменений)

AuthStore.roles() ──signal──→ MenuService.menus() ──signal──→ AppMenu.model()
                                (автоматический пересчёт при смене ролей)

LayoutService (Sakai) — управляет collapsed/overlay/mobile state
        ↑
  AppTopbar (onMenuToggle)
  AppLayout (containerClass, click outside)
```

---

## 8. OpenAPI Codegen pipeline

### `tools/generate-api-clients.mjs`

```js
#!/usr/bin/env node
import { execSync } from 'node:child_process';
import { existsSync, mkdirSync } from 'node:fs';

const services = [
  {
    name:   'identity',
    spec:   process.env['IDENTITY_API_URL']    ?? 'http://localhost:5200/openapi/v1.json',
    output: 'libs/identity/api-client/src/lib/generated',
  },
  {
    name:   'recruitment',
    spec:   process.env['RECRUITMENT_API_URL'] ?? 'http://localhost:5201/openapi/v1.json',
    output: 'libs/recruitment/api-client/src/lib/generated',
  },
  {
    name:   'candidates',
    spec:   process.env['CANDIDATES_API_URL']  ?? 'http://localhost:5202/openapi/v1.json',
    output: 'libs/candidates/api-client/src/lib/generated',
  },
  {
    name:   'vacancy-tasks',
    spec:   process.env['VT_API_URL']          ?? 'http://localhost:5203/openapi/v1.json',
    output: 'libs/vacancy-tasks/api-client/src/lib/generated',
  },
];

// Фильтр по --service=NAME если нужна только одна
const filter = process.argv.find(a => a.startsWith('--service='))?.split('=')[1];
const targets = filter ? services.filter(s => s.name === filter) : services;

for (const svc of targets) {
  console.log(`\n▶ Generating ${svc.name} client from ${svc.spec}`);
  mkdirSync(svc.output, { recursive: true });

  execSync(
    `npx openapi-generator-cli generate \
      --input-spec "${svc.spec}" \
      --generator-name typescript-angular \
      --output "${svc.output}" \
      --additional-properties=\
providedIn=root,\
withInterfaces=true,\
useSingleRequestParameter=true,\
enumPropertyNaming=original,\
removeOperationIdPrefix=true,\
ngVersion=18`,
    { stdio: 'inherit' }
  );

  console.log(`✓ ${svc.name} done`);
}
```

### `openapitools.json` (в корне angular/)

```json
{
  "$schema": "node_modules/@openapitools/openapi-generator-cli/config.schema.json",
  "spaces": 2,
  "generator-cli": {
    "version": "7.4.0"
  }
}
```

### Nx targets в `project.json` каждого api-client

```json
{
  "targets": {
    "generate": {
      "executor": "nx:run-commands",
      "options": {
        "command": "node tools/generate-api-clients.mjs --service recruitment",
        "cwd": "{workspaceRoot}"
      }
    }
  }
}
```

### Использование сгенерированных клиентов

```typescript
// В data-access store напрямую inject сервис из generated папки:
import { VacanciesService } from '@cheetah/recruitment/api-client';

// В providers app.config.ts:
{ provide: BASE_PATH, useValue: environment.apiUrl }
// BASE_PATH из сгенерированного кода подхватывается автоматически
```

### Ручной запуск

```bash
# Регенерировать все клиенты (бэкенд должен быть запущен)
nx run-many --target=generate --all

# Только один сервис
node tools/generate-api-clients.mjs --service recruitment
```

### Если бэкенд недоступен — генерация из файла

```bash
# Скачать spec заранее
curl http://localhost:5201/openapi/v1.json > specs/recruitment.json

# Поменять spec в generate-api-clients.mjs на путь к файлу
spec: './specs/recruitment.json'
```

---

## 9. Паттерн feature модуля

Показано на примере Recruitment. Все остальные модули (candidates, identity, vacancy-tasks) реализуются по тому же шаблону.

### `libs/recruitment/data-access/src/lib/vacancies.store.ts`

```typescript
import { inject }         from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { computed }       from '@angular/core';
import { firstValueFrom } from 'rxjs';
// Сгенерированный клиент
import { VacanciesService, VacancyViewModel, CreateVacancyRequest } from '@cheetah/recruitment/api-client';

interface VacanciesState {
  items:    VacancyViewModel[];
  selected: VacancyViewModel | null;
  loading:  boolean;
  error:    string | null;
}

export const VacanciesStore = signalStore(
  { providedIn: 'root' },

  withState<VacanciesState>({
    items:    [],
    selected: null,
    loading:  false,
    error:    null,
  }),

  withComputed(({ items }) => ({
    total: computed(() => items().length),
  })),

  withMethods((store) => {
    const api = inject(VacanciesService);

    return {
      async loadAll(): Promise<void> {
        patchState(store, { loading: true, error: null });
        try {
          const items = await firstValueFrom(api.vacanciesGet());
          patchState(store, { items, loading: false });
        } catch (e) {
          patchState(store, { error: 'Failed to load vacancies', loading: false });
        }
      },

      async create(request: CreateVacancyRequest): Promise<void> {
        await firstValueFrom(api.vacanciesPost({ createVacancyRequest: request }));
        await this.loadAll();
      },

      async delete(id: string): Promise<void> {
        await firstValueFrom(api.vacanciesIdDelete({ id }));
        patchState(store, s => ({ items: s.items.filter(i => i.id !== id) }));
      },

      select(vacancy: VacancyViewModel | null): void {
        patchState(store, { selected: vacancy });
      },
    };
  })
);
```

### `libs/recruitment/feature/src/lib/routes.ts`

```typescript
import { Routes } from '@angular/router';

export const RECRUITMENT_ROUTES: Routes = [
  { path: '',     loadComponent: () => import('./vacancies-board/vacancies-board.component').then(m => m.VacanciesBoardComponent) },
  { path: 'list', loadComponent: () => import('./vacancies-list/vacancies-list.component').then(m => m.VacanciesListComponent) },
  { path: ':id',  loadComponent: () => import('./vacancy-detail/vacancy-detail.component').then(m => m.VacancyDetailComponent) },
];
```

### `libs/recruitment/feature/src/lib/vacancies-board/vacancies-board.component.ts`

```typescript
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule }    from '@angular/common';
import { CardModule }      from 'primeng/card';
import { DataViewModule }  from 'primeng/dataview';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { VacanciesStore }  from '@cheetah/recruitment/data-access';

@Component({
  selector: 'crm-vacancies-board',
  standalone: true,
  imports: [CommonModule, CardModule, DataViewModule, ProgressSpinnerModule],
  template: `
    @if (store.loading()) {
      <p-progressSpinner />
    } @else {
      <p-dataView [value]="store.items()">
        <ng-template pTemplate="list" let-vacancy>
          <p-card [header]="vacancy.title">
            <p>{{ vacancy.description }}</p>
          </p-card>
        </ng-template>
      </p-dataView>
    }
  `
})
export class VacanciesBoardComponent implements OnInit {
  store = inject(VacanciesStore);
  ngOnInit() { this.store.loadAll(); }
}
```

### `libs/recruitment/feature/src/lib/recruitment-menu.contributor.ts`

```typescript
import { MenuContributor, MenuConfigurationContext, StandardMenus } from '@cheetah/shared/core';

export class RecruitmentMenuContributor implements MenuContributor {
  order = 10;

  configureMenu(ctx: MenuConfigurationContext): void {
    ctx.getOrCreate(StandardMenus.MAIN)
       .addItem({ id: 'vacancies',  label: 'Вакансии',   icon: 'pi pi-briefcase', routerLink: ['/vacancies'],  order: 1 })
       .addItem({ id: 'candidates', label: 'Кандидаты',  icon: 'pi pi-users',     routerLink: ['/candidates'], order: 2 })
       .addItem({ id: 'tasks',      label: 'Задачи',     icon: 'pi pi-check-square', routerLink: ['/tasks'],   order: 3 });
  }
}
```

**Регистрация в app.config.ts:**
```typescript
{ provide: MENU_CONTRIBUTOR, useClass: RecruitmentMenuContributor, multi: true }
```

---

## 10. `apps/crm-shell`

### `apps/crm-shell/src/app/app.config.ts`

```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding }  from '@angular/router';
import { provideAnimationsAsync }                    from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors }        from '@angular/common/http';
import { providePrimeNG }                            from 'primeng/config';
import Aura                                          from '@primeng/themes/aura';

import { jwtInterceptor }         from '@cheetah/auth/interceptors';
import { unauthorizedInterceptor } from '@cheetah/auth/interceptors';
import { MENU_CONTRIBUTOR, HEADER_WIDGET, APP_CONFIG, BASE_PATH } from '@cheetah/shared/core';
import { UserMenuContributor }    from '@cheetah/auth/data-access';
import { UserInfoWidgetComponent } from '@cheetah/auth/feature-login';
import { RecruitmentMenuContributor } from '@cheetah/recruitment/feature';
import { IdentityMenuContributor }    from '@cheetah/identity/feature';

import { environment } from '../environments/environment';
import { APP_ROUTES }  from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(APP_ROUTES, withComponentInputBinding()),
    provideAnimationsAsync(),
    provideHttpClient(
      withInterceptors([jwtInterceptor, unauthorizedInterceptor])
    ),

    // PrimeNG
    providePrimeNG({
      theme: { preset: Aura, options: { darkModeSelector: '.dark-mode' } },
      ripple: true,
    }),

    // App config
    {
      provide: APP_CONFIG,
      useValue: {
        appName: 'Cheetah CRM',
        homeUrl: '/',
        apiUrl:  environment.apiUrl,
      },
    },

    // OpenAPI BASE_PATH
    { provide: BASE_PATH, useValue: environment.apiUrl },

    // Header widgets (multi)
    { provide: HEADER_WIDGET, useValue: { component: UserInfoWidgetComponent, order: 100 }, multi: true },

    // Menu contributors (multi) — порядок определяет порядок пунктов
    { provide: MENU_CONTRIBUTOR, useClass: RecruitmentMenuContributor, multi: true },
    { provide: MENU_CONTRIBUTOR, useClass: IdentityMenuContributor,    multi: true },
    { provide: MENU_CONTRIBUTOR, useClass: UserMenuContributor,        multi: true },
  ]
};
```

### `apps/crm-shell/src/app/app.routes.ts`

```typescript
import { Routes }          from '@angular/router';
import { LAYOUT_ROUTES }   from '@cheetah/layout/feature-shell';

export const APP_ROUTES: Routes = [
  { path: 'login',  loadComponent: () => import('@cheetah/auth/feature-login').then(m => m.LoginComponent) },
  { path: 'logout', loadComponent: () => import('@cheetah/auth/feature-login').then(m => m.LogoutComponent) },
  ...LAYOUT_ROUTES,
  { path: '**', redirectTo: '' },
];
```

### `apps/crm-shell/src/app/app.component.ts`

```typescript
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet }    from '@angular/router';
import { AuthStore }       from '@cheetah/auth/data-access';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`
})
export class AppComponent implements OnInit {
  authStore = inject(AuthStore);

  ngOnInit(): void {
    // Восстановить сессию из localStorage при старте
    this.authStore.restore();
  }
}
```

### `apps/crm-shell/src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl:     'http://localhost:5555',
};
```

### `apps/crm-shell/src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl:     '/api',  // nginx proxy в production
};
```

---

## 11. Стилизация PrimeNG

> Основная стилизация layout — это SCSS из Sakai (раздел 7, шаги 7-8).
> Здесь — только то, что добавляется **поверх** Sakai для наших нужд.

### `apps/crm-shell/src/styles.scss`

```scss
// 1. Sakai layout styles — ПЕРВЫМ, до всего остального
@use 'libs/layout/feature-shell/src/styles/layout' as *;

// 2. PrimeIcons
@import "primeicons/primeicons.css";

// 3. Lato font (как в Sakai)
@import url('https://fonts.googleapis.com/css2?family=Lato:wght@300;400;700&display=swap');

// 4. Наши кастомные добавления (только то, чего нет в Sakai)

// Login/logout страницы — центрированный layout
.crm-center-layout {
  display: flex;
  min-height: 100vh;
  align-items: center;
  justify-content: center;
  background-color: var(--surface-ground);
}
.crm-login-card { width: 400px; }

// User info widget (в topbar-actions)
.crm-user-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.625rem;
  border-radius: var(--content-border-radius);
  cursor: pointer;
  user-select: none;
  font-size: 0.875rem;
  transition: background-color var(--element-transition-duration);

  &:hover { background-color: var(--surface-hover); }
}
.crm-user-name    { font-weight: 500; white-space: nowrap; }
.crm-user-chevron { font-size: 0.75rem; opacity: 0.7; }

// Разделитель в topbar между виджетами
.layout-topbar-actions {
  gap: 0.5rem;
}
```

**Обращай внимание**: в Sakai используются PrimeNG CSS-переменные (`var(--surface-ground)`, `var(--surface-hover)`, `var(--content-border-radius)`) — использовать их вместо хардкоженных значений. Они автоматически переключаются при смене темы.

---

## 12. Тестирование

### NgRx SignalStore тесты

```typescript
// vacancies.store.spec.ts
import { TestBed }          from '@angular/core/testing';
import { VacanciesStore }   from './vacancies.store';
import { VacanciesService } from '@cheetah/recruitment/api-client';
import { of }               from 'rxjs';

describe('VacanciesStore', () => {
  let store: InstanceType<typeof VacanciesStore>;
  let apiSpy: jest.Mocked<VacanciesService>;

  beforeEach(() => {
    apiSpy = { vacanciesGet: jest.fn().mockReturnValue(of([])) } as any;

    TestBed.configureTestingModule({
      providers: [
        VacanciesStore,
        { provide: VacanciesService, useValue: apiSpy },
      ]
    });
    store = TestBed.inject(VacanciesStore);
  });

  it('should load vacancies', async () => {
    apiSpy.vacanciesGet.mockReturnValue(of([{ id: '1', title: 'Dev' }] as any));
    await store.loadAll();
    expect(store.items().length).toBe(1);
    expect(store.loading()).toBe(false);
  });
});
```

### MenuService тест

```typescript
// menu.service.spec.ts
describe('MenuService', () => {
  it('should filter items by roles', () => {
    const item: MenuItem = { id: '1', label: 'Admin', order: 1, requiredRoles: ['Admin'] };
    expect(filterByRoles([item], ['Admin'])).toHaveLength(1);
    expect(filterByRoles([item], ['User'])).toHaveLength(0);
  });
});
```

### E2E (Playwright)

```typescript
// apps/crm-shell-e2e/src/login.spec.ts
test('should login and redirect to dashboard', async ({ page }) => {
  await page.goto('/login');
  await page.fill('#username', 'admin');
  await page.fill('#password', 'password');
  await page.click('button[type="submit"]');
  await expect(page).toHaveURL('/vacancies');
  await expect(page.locator('.crm-user-name')).toBeVisible();
});
```

---

## 13. Чеклист реализации

### Фаза 1 — Foundation
- [ ] `nx create-workspace` выполнен
- [ ] Все npm зависимости установлены
- [ ] Все libs созданы через `nx g`
- [ ] `tsconfig.base.json` пути настроены
- [ ] ESLint boundary rules настроены

### Фаза 2 — Core + Auth
- [ ] `libs/shared/core` — токены, интерфейсы, StandardMenus
- [ ] `libs/shared/util-auth` — parseJwt, isTokenExpired
- [ ] `libs/auth/data-access` — AuthStore, TokenStorage, guards
- [ ] `libs/auth/interceptors` — jwtInterceptor, unauthorizedInterceptor
- [ ] `libs/auth/feature-login` — LoginComponent, UserInfoWidgetComponent, UserMenuContributor

### Фаза 3 — Navigation + Layout (Sakai-based)
- [ ] `libs/navigation/util` — MenuConfigurationContextImpl, filterByRoles, toSakaiMenuItem
- [ ] `libs/navigation/data-access` — MenuService (computed signal)
- [ ] Скопировать `layout.service.ts` из Sakai → `libs/layout/data-access/`
- [ ] Скопировать `app.sidebar.ts`, `app.menu.ts`, `app.menuitem.ts` из Sakai → `libs/layout/feature-sidebar/` (без изменений)
- [ ] Скопировать `app.footer.ts`, `app.layout.ts`, `app.configurator.ts`, `app.floatingconfigurator.ts` из Sakai → `libs/layout/feature-shell/` (без изменений)
- [ ] Адаптировать `app.topbar.ts` из Sakai: добавить `HEADER_WIDGET` + `ngComponentOutlet`
- [ ] Адаптировать `app.menu.ts` из Sakai: заменить хардкоженный model на `MenuService.menus()`
- [ ] Скопировать все `_*.scss` из Sakai → `libs/layout/feature-shell/src/styles/`
- [ ] `libs/layout/feature-shell/src/lib/layout.routes.ts` — AppLayout + authGuard + lazy children

### Фаза 4 — OpenAPI Codegen
- [ ] `tools/generate-api-clients.mjs` создан
- [ ] `openapitools.json` настроен
- [ ] Бэкенд запущен (docker-compose up)
- [ ] `node tools/generate-api-clients.mjs` выполнен для всех сервисов
- [ ] Сгенерированные клиенты в `libs/*/api-client/src/lib/generated`
- [ ] Nx targets `generate` в каждом `project.json` прописаны

### Фаза 5 — Feature модули
- [ ] Recruitment: api-client → data-access (VacanciesStore) → feature (маршруты, компоненты) → MenuContributor
- [ ] Candidates: аналогично
- [ ] VacancyTasks: аналогично
- [ ] Identity: аналогично + roleGuard на роуте `/admin`

### Фаза 6 — Shell сборка
- [ ] `app.config.ts` — все providers зарегистрированы
- [ ] `app.routes.ts` — все маршруты подключены
- [ ] `AppComponent.ngOnInit` — `authStore.restore()` вызывается
- [ ] `styles.scss` — все CSS variables и overrides прописаны
- [ ] `environment.ts` — apiUrl настроен для dev и prod

### Фаза 7 — QA
- [ ] Юнит тесты для каждого store
- [ ] Юнит тесты для MenuService
- [ ] E2E: login flow
- [ ] E2E: sidebar menu rendering после логина
- [ ] E2E: role-based route guard
- [ ] `nx affected --target=build` — нет ошибок
- [ ] Bundle size проверен (`nx build crm-shell --stats-json`)

---

## Важные замечания

### `ngComponentOutlet` vs `DynamicComponent`
В Angular `*ngComponentOutlet="widget.component"` рендерит компонент **инлайн** в текущем дереве без обёртки. Это аналог `builder.OpenComponent` в Blazor и именно то, что решило проблему с event routing при использовании `DynamicComponent`. Никогда не использовать обёртки для виджетов.

### Порядок interceptors
В `withInterceptors([jwtInterceptor, unauthorizedInterceptor])` interceptors выполняются **в порядке перечисления** на запрос и **в обратном порядке** на ответ. JWT добавляется первым (запрос), 401 обрабатывается последним (ответ). Этот порядок правильный.

### `multi: true` для токенов
`MENU_CONTRIBUTOR` и `HEADER_WIDGET` всегда регистрируются с `multi: true`. При `inject(MENU_CONTRIBUTOR)` Angular вернёт массив всех зарегистрированных значений. Если `multi: true` забыть — предыдущая регистрация будет перезаписана.

### Signal store и реактивность
`MenuService.menus` — это `computed()` сигнал, зависящий от `AuthStore.roles()`. Когда пользователь логинится или меняются роли — меню **автоматически** пересчитывается без явного вызова `Invalidate()`. CrmSidebarComponent использует этот сигнал напрямую, поэтому он тоже обновляется автоматически.

### Codegen — имена методов
Генератор `typescript-angular` создаёт методы по паттерну `{tag}{OperationId}`. Если метод называется `vacanciesGet()` а не `getAll()` — это нормально, так генератор именует методы. Проверять сгенерированный файл после первого запуска и адаптировать вызовы в store-ах.

### BASE_PATH и CORS
`BASE_PATH` должен указывать на прокси (порт 5555 в dev), который форвардит запросы к отдельным API сервисам. Не указывать напрямую на отдельные сервисы — CORS настроен только через прокси.
