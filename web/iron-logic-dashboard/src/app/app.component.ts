import { Component, OnInit, inject } from '@angular/core';
import { NavigationEnd, Router, ActivatedRoute, RouterOutlet } from '@angular/router';
import { SidebarComponent } from './layout/sidebar/sidebar.component';
import { FooterComponent } from '@features/footer/footer'; // 🚀 حتماً چک کن مسیر درست باشد
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, FooterComponent], // 🚀 اضافه شد
  templateUrl: './app.html',
})
export class AppComponent implements OnInit { // کلاس اصلی که آنگولار دنبالش می‌گردد
  showSidebar = true;

  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);

  ngOnInit() {
    this.updateSidebar();

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.updateSidebar();
    });
  }

  private updateSidebar() {
    let child = this.activatedRoute.firstChild;

    if (child) {
      const hide = child.snapshot.data['hideSidebar'];
      this.showSidebar = hide !== true;
    } else {
      this.showSidebar = this.activatedRoute.snapshot.data['hideSidebar'] !== true;
    }
  }
}
