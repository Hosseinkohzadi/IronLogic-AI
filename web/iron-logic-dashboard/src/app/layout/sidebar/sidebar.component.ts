import { Component } from '@angular/core';
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
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    LucideAngularModule
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: 'layout-dashboard' },
    { label: 'Users', route: '/admin/users', icon: 'users' },
    { label: 'Sessions', route: '/admin/sessions', icon: 'calendar' },
    { label: 'Financial', route: '/admin/financial', icon: 'credit-card' },
    { label: 'Exercises', route: '/admin/exercises', icon: 'dumbbell' },
    { label: 'Exercise Sessions', route: '/admin/exercise-sessions', icon: 'activity' },
    { label: 'Daily Weights', route: '/admin/daily-weights', icon: 'weight' },
    { label: 'Muscles', route: '/admin/muscles', icon: 'layers' },
    { label: 'Equipment', route: '/admin/equipment', icon: 'wrench' },
    { label: 'Integrity', route: '/admin/integrity', icon: 'shield-check' },
    { label: 'Settings', route: '/admin/settings', icon: 'settings' },
  ];
}
