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
export class App {
  // این متغیر خطای TS2339 را برطرف می‌کند
  showSidebar = false;

  constructor(private router: Router) {
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      const noSidebarRoutes = ['/', '/login', '/register', '/forgot'];
      const currentUrl = event.urlAfterRedirects.split('?')[0];
      this.showSidebar = !noSidebarRoutes.includes(currentUrl);
    });
  }
}
