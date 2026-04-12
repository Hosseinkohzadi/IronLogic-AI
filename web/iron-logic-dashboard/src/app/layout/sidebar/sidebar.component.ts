import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '@core/services/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent implements OnInit {
  private readonly sidebarStorageKey = 'ironlogic.admin.sidebar.collapsed';
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isCollapsed = signal(true);
  readonly isAdmin = computed(() => this.authService.role() === 'SUPER_ADMIN');

  readonly adminNavItems: NavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: 'layout-dashboard' },
    { label: 'Users', route: '/admin/users', icon: 'users' },
    { label: 'Sessions', route: '/admin/sessions', icon: 'calendar' },
    { label: 'Financial', route: '/admin/financial', icon: 'credit-card' },
    { label: 'Exercises', route: '/admin/exercises', icon: 'dumbbell' },
    { label: 'Muscles', route: '/admin/muscles', icon: 'layers' },
    { label: 'Equipment', route: '/admin/equipment', icon: 'wrench' },
    { label: 'Subscription', route: '/admin/subscription', icon: 'gem' },
    { label: 'Billing', route: '/admin/billing', icon: 'credit-card' },
    { label: 'Settings', route: '/admin/settings', icon: 'settings' },
  ];

  readonly userNavItems: NavItem[] = [
    { label: 'Athlete Portal', route: '/athlete/portal', icon: 'layout-dashboard' },
    { label: 'Upgrade to Pro', route: '/athlete/subscription', icon: 'credit-card' },
  ];

  readonly navItems = computed(() => (this.isAdmin() ? this.adminNavItems : this.userNavItems));

  ngOnInit(): void {
    const savedState = localStorage.getItem(this.sidebarStorageKey);
    if (savedState !== null) {
      this.isCollapsed.set(savedState === 'true');
    }
  }

  toggleSidebar(): void {
    this.isCollapsed.update((current) => {
      const next = !current;
      localStorage.setItem(this.sidebarStorageKey, String(next));
      return next;
    });
  }

  onLogout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/auth/login');
  }
}
