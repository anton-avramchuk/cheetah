import { computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { parseJwt, isTokenExpired, ParsedUser } from '@cheetah/shared/util-auth';
import { TokenStorage } from './token-storage.service';

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
