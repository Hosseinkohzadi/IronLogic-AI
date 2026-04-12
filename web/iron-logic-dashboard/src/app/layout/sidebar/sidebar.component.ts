import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
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

  readonly isCollapsed = signal(true);

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: 'layout-dashboard' },
    { label: 'Users', route: '/admin/users', icon: 'users' },
    { label: 'Sessions', route: '/admin/sessions', icon: 'calendar' },
    { label: 'Financial', route: '/admin/financial', icon: 'credit-card' },
    { label: 'Exercises', route: '/admin/exercises', icon: 'dumbbell' },
    { label: 'Exercise Sessions', route: '/admin/exercise-sessions', icon: 'activity' },
    { label: 'Muscles', route: '/admin/muscles', icon: 'layers' },
    { label: 'Equipment', route: '/admin/equipment', icon: 'wrench' },
    { label: 'Integrity', route: '/admin/integrity', icon: 'shield-check' },
    { label: 'Settings', route: '/admin/settings', icon: 'settings' },
  ];

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
  }
}
