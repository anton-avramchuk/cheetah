import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { AuthStore } from '@cheetah/auth/data-access';
import { BASE_PATH } from '@cheetah/shared/core';

interface LoginResponse {
  accessToken: string;
}

@Component({
  selector: 'crm-login',
  standalone: true,
  imports: [ReactiveFormsModule, CardModule, InputTextModule, ButtonModule, MessageModule],
  template: `
    <div class="crm-center-layout">
      <p-card header="Вход в систему" styleClass="crm-login-card">
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="field mb-4">
            <label for="username" class="block font-medium mb-2">Логин</label>
            <input
              pInputText
              id="username"
              formControlName="username"
              class="w-full"
              placeholder="Введите логин"
            />
          </div>
          <div class="field mb-4">
            <label for="password" class="block font-medium mb-2">Пароль</label>
            <input
              pInputText
              type="password"
              id="password"
              formControlName="password"
              class="w-full"
              placeholder="Введите пароль"
            />
          </div>
          @if (auth.error()) {
            <p-message severity="error" [text]="auth.error()!" styleClass="mb-4 w-full" />
          }
          <p-button
            type="submit"
            label="Войти"
            [loading]="auth.loading()"
            [disabled]="form.invalid"
            styleClass="w-full"
          />
        </form>
      </p-card>
    </div>
  `,
})
export class LoginComponent {
  readonly auth    = inject(AuthStore);
  readonly http    = inject(HttpClient);
  readonly basePath = inject(BASE_PATH);
  readonly fb      = inject(FormBuilder);

  readonly form = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  async submit(): Promise<void> {
    if (this.form.invalid) return;
    const { username, password } = this.form.value;
    this.auth.setLoading(true);
    try {
      const response = await firstValueFrom(
        this.http.post<LoginResponse>(`${this.basePath}/api/auth/login`, { username, password })
      );
      this.auth.handleLoginSuccess(response.accessToken);
    } catch {
      this.auth.setError('Неверный логин или пароль');
    }
  }
}
