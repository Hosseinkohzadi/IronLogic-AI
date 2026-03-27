import { Component } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { SidebarComponent } from './layout/sidebar/sidebar.component';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent],
  templateUrl: './app.html',
})
export class AppComponent {
  // به صورت پیش‌فرض سایدبار مخفی است (برای لندینگ و لاگین)
  showSidebar = false;

  constructor(private router: Router) {
    // گوش دادن به تغییرات مسیر در مرورگر
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      const noSidebarRoutes = ['/', '/login', '/register', '/forgot'];
      const currentUrl = event.urlAfterRedirects.split('?')[0];
      this.showSidebar = !noSidebarRoutes.includes(currentUrl);
    });
  }
}
