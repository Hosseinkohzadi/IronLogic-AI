import { inject, Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';

export type UserRole = 'SUPER_ADMIN' | 'ATHLETE';

const ROLE_STORAGE_KEY = 'ironlogic.auth.role';
const USER_ID_STORAGE_KEY = 'ironlogic.auth.userId';
const TOKEN_STORAGE_KEY = 'ironlogic.auth.token';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly router = inject(Router);
  private readonly roleState = signal<UserRole | null>(this.readRoleFromStorage());
  private readonly currentUserIdState = signal<string | null>(this.readUserIdFromStorage());

  readonly role = computed(() => this.roleState());
  readonly currentUserId = computed(() => this.currentUserIdState());
  readonly isAuthenticated = computed(() => !!this.roleState() && !!this.currentUserIdState());

  loginAsSuperAdmin(): void {
    this.setSession('SUPER_ADMIN', 'super-admin-001');
  }

  loginAsAthlete(userId: string): void {
    this.setSession('ATHLETE', userId);
  }

  logout(): void {
    this.roleState.set(null);
    this.currentUserIdState.set(null);
    localStorage.removeItem(ROLE_STORAGE_KEY);
    localStorage.removeItem(USER_ID_STORAGE_KEY);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    void this.router.navigateByUrl('/auth/login');
  }

  hasRole(role: UserRole): boolean {
    return this.roleState() === role;
  }

  private setSession(role: UserRole, userId: string): void {
    this.roleState.set(role);
    this.currentUserIdState.set(userId);
    localStorage.setItem(ROLE_STORAGE_KEY, role);
    localStorage.setItem(USER_ID_STORAGE_KEY, userId);
  }

  private readRoleFromStorage(): UserRole | null {
    const role = localStorage.getItem(ROLE_STORAGE_KEY);
    if (role === 'SUPER_ADMIN' || role === 'ATHLETE') {
      return role;
    }

    return null;
  }

  private readUserIdFromStorage(): string | null {
    return localStorage.getItem(USER_ID_STORAGE_KEY);
  }
}
