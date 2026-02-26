// Скопировано из sakai-ng/src/app/layout/component/app.footer.ts
import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-footer',
    template: `
        <div class="layout-footer">
            Cheetah CRM &nbsp;|&nbsp;
            <span class="text-muted">powered by</span>
            <a href="https://primeng.org" target="_blank" rel="noopener noreferrer" class="text-primary font-bold hover:underline"> PrimeNG</a>
        </div>
    `
})
export class AppFooter {}
