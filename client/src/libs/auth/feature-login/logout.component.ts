import { Component, inject, OnInit } from '@angular/core';
import { AuthStore } from '@cheetah/auth/data-access';

@Component({
  selector: 'crm-logout',
  standalone: true,
  template: `<div class="crm-center-layout"><p>Выход...</p></div>`,
})
export class LogoutComponent implements OnInit {
  readonly auth = inject(AuthStore);
  ngOnInit(): void { this.auth.logout(); }
}
