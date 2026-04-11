import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';

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
})
export class SidebarComponent {
  private readonly sidebarStorageKey = 'ironlogic.sidebarCollapsed';

  isCollapsed = true;

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: 'layout-dashboard' },
    { label: 'Users', route: '/admin/users', icon: 'users' },
    { label: 'Sessions', route: '/admin/sessions', icon: 'calendar' },
    { label: 'Financial', route: '/admin/financial', icon: 'credit-card' },
    { label: 'Exercises', route: '/admin/exercises', icon: 'dumbbell' },
    { label: 'Muscles', route: '/admin/muscles', icon: 'layers' },
    { label: 'Equipment', route: '/admin/equipment', icon: 'wrench' },
    { label: 'Integrity', route: '/admin/integrity', icon: 'shield-check' },
    { label: 'Settings', route: '/admin/settings', icon: 'settings' },
  ];

  constructor() {
    this.isCollapsed = this.readCollapsedState();
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
    this.persistCollapsedState(this.isCollapsed);
  }

  private readCollapsedState(): boolean {
    if (!this.hasBrowserStorage()) {
      return true;
    }

    const raw = localStorage.getItem(this.sidebarStorageKey);
    if (raw === null) {
      return true;
    }

    return raw === 'true';
  }

  private persistCollapsedState(value: boolean): void {
    if (!this.hasBrowserStorage()) {
      return;
    }

    localStorage.setItem(this.sidebarStorageKey, String(value));
  }

  private hasBrowserStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
