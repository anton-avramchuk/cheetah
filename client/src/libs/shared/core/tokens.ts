import { InjectionToken, Type } from '@angular/core';

export interface HeaderWidget {
  component: Type<unknown>;
  inputs?: Record<string, unknown>;
  order: number;
}

export interface AppConfig {
  appName: string;
  logoUrl?: string;
  homeUrl: string;
  apiUrl: string;
}

export const HEADER_WIDGET = new InjectionToken<HeaderWidget[]>('HEADER_WIDGET');
export const APP_CONFIG    = new InjectionToken<AppConfig>('APP_CONFIG');
export const BASE_PATH     = new InjectionToken<string>('BASE_PATH');
