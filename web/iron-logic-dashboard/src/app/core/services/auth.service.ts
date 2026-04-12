import { inject, Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

export type UserRole = 'SUPER_ADMIN' | 'ATHLETE';

interface AuthSessionResult {
  role: UserRole;
  userId: string;
  token?: string;
  user?: AuthUser;
}

export interface AuthUser {
  id: string;
  email?: string;
  fullName?: string;
  role?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
}

const ROLE_STORAGE_KEY = 'ironlogic.auth.role';
const USER_ID_STORAGE_KEY = 'ironlogic.auth.userId';
const TOKEN_STORAGE_KEY = 'ironlogic.auth.token';
const USER_STORAGE_KEY = 'ironlogic.auth.user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly roleState = signal<UserRole | null>(this.readRoleFromStorage());
  private readonly currentUserIdState = signal<string | null>(this.readUserIdFromStorage());
  private readonly currentUserState = signal<AuthUser | null>(this.readUserFromStorage());

  readonly role = computed(() => this.roleState());
  readonly currentUserId = computed(() => this.currentUserIdState());
  readonly currentUser = computed(() => this.currentUserState());
  readonly isAuthenticated = computed(() => !!this.roleState() && !!this.currentUserIdState());

  loginAsSuperAdmin(): void {
    this.setSession('SUPER_ADMIN', 'super-admin-001');
  }

  loginAsAthlete(userId: string): void {
    this.setSession('ATHLETE', userId);
  }

  login(data: LoginRequest): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/auth/login`, {
      Email: data.email,
      Password: data.password,
    });
  }

  register(data: RegisterRequest): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/auth/register`, {
      Email: data.email,
      Password: data.password,
      ConfirmPassword: data.confirmPassword,
      FullName: data.fullName,
    });
  }

  handleAuthSuccess(payload: unknown): boolean {
    const session = this.extractSessionFromPayload(payload);
    if (!session) {
      return false;
    }

    this.setSession(session.role, session.userId, session.user);

    if (session.token) {
      localStorage.setItem(TOKEN_STORAGE_KEY, session.token);
    }

    void this.router.navigate(this.getHomeRoute(session.role));
    return true;
  }

  navigateToHomeByRole(role: UserRole): void {
    void this.router.navigate(this.getHomeRoute(role));
  }

  getHomeRoute(role: UserRole): string[] {
    return role === 'SUPER_ADMIN' ? ['/admin/dashboard'] : ['/athlete/dashboard'];
  }

  logout(): void {
    this.roleState.set(null);
    this.currentUserIdState.set(null);
    this.currentUserState.set(null);
    localStorage.removeItem(ROLE_STORAGE_KEY);
    localStorage.removeItem(USER_ID_STORAGE_KEY);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(USER_STORAGE_KEY);
    void this.router.navigateByUrl('/auth/login');
  }

  hasRole(role: UserRole): boolean {
    return this.roleState() === role;
  }

  private setSession(role: UserRole, userId: string, user?: AuthUser): void {
    this.roleState.set(role);
    this.currentUserIdState.set(userId);
    localStorage.setItem(ROLE_STORAGE_KEY, role);
    localStorage.setItem(USER_ID_STORAGE_KEY, userId);

    if (user) {
      this.currentUserState.set(user);
      localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
    }
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

  private readUserFromStorage(): AuthUser | null {
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as Partial<AuthUser>;
      if (typeof parsed.id === 'string' && parsed.id.trim()) {
        return {
          id: parsed.id,
          email: parsed.email,
          fullName: parsed.fullName,
          role: parsed.role,
        };
      }
      return null;
    } catch {
      return null;
    }
  }

  private extractSessionFromPayload(payload: unknown): AuthSessionResult | null {
    if (typeof payload !== 'object' || payload === null) {
      return null;
    }

    const response = payload as Record<string, unknown>;
    const token = this.readString(response, ['token', 'accessToken', 'jwt', 'access_token']);
    const userObject = this.readUserObject(response);
    const tokenClaims = token ? this.decodeJwtPayload(token) : null;

    const roleRaw =
      this.readString(response, ['role', 'Role']) ??
      this.readString(userObject, ['role', 'Role']) ??
      this.readFirstStringArrayValue(response, ['roles', 'Roles']) ??
      this.readFirstStringArrayValue(userObject, ['roles', 'Roles']) ??
      this.readString(tokenClaims, [
        'role',
        'roles',
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
      ]);

    const role = this.normalizeRole(roleRaw);
    if (!role) {
      return null;
    }

    const userId =
      this.readString(response, ['userId', 'id', 'Id']) ??
      this.readString(userObject, ['id', 'Id', 'userId', 'UserId']) ??
      this.readString(tokenClaims, [
        'sub',
        'nameid',
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
      ]) ??
      'current-user';

    const user: AuthUser = {
      id: userId,
      email: this.readString(userObject, ['email', 'Email']) ?? undefined,
      fullName:
        this.readString(userObject, ['fullName', 'FullName', 'name', 'Name']) ?? undefined,
      role: roleRaw ?? undefined,
    };

    return {
      role,
      userId,
      token: token ?? undefined,
      user,
    };
  }

  private normalizeRole(roleRaw: string | null): UserRole | null {
    if (!roleRaw) {
      return null;
    }

    const normalized = roleRaw.trim().toUpperCase();
    if (normalized === 'SUPER_ADMIN' || normalized === 'ADMIN') {
      return 'SUPER_ADMIN';
    }

    if (normalized === 'ATHLETE' || normalized === 'USER') {
      return 'ATHLETE';
    }

    return null;
  }

  private readUserObject(source: Record<string, unknown>): Record<string, unknown> {
    const candidate = source['user'] ?? source['User'];
    if (typeof candidate === 'object' && candidate !== null) {
      return candidate as Record<string, unknown>;
    }

    return source;
  }

  private readString(
    source: Record<string, unknown> | null,
    keys: string[],
  ): string | null {
    if (!source) {
      return null;
    }

    for (const key of keys) {
      const value = source[key];
      if (typeof value === 'string' && value.trim()) {
        return value;
      }
    }

    return null;
  }

  private readFirstStringArrayValue(
    source: Record<string, unknown> | null,
    keys: string[],
  ): string | null {
    if (!source) {
      return null;
    }

    for (const key of keys) {
      const value = source[key];
      if (Array.isArray(value)) {
        const first = value.find((entry) => typeof entry === 'string' && entry.trim());
        if (typeof first === 'string') {
          return first;
        }
      }
    }

    return null;
  }

  private decodeJwtPayload(token: string): Record<string, unknown> | null {
    const parts = token.split('.');
    if (parts.length < 2) {
      return null;
    }

    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
      const decoded = atob(padded);
      return JSON.parse(decoded) as Record<string, unknown>;
    } catch {
      return null;
    }
  }
}
