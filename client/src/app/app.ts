import { Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthStore } from '@cheetah/auth/data-access';

@Component({
  imports: [RouterModule],
  selector: 'app-root',
  template: `<router-outlet />`,
})
export class App implements OnInit {
  private readonly authStore = inject(AuthStore);

  ngOnInit(): void {
    // Восстановить JWT-сессию из localStorage при старте приложения
    this.authStore.restore();
  }
}
