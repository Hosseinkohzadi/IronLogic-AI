import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '@env/environment';
import { ApplicationUser } from '@core/models';

export type UserUpdateRequest = ApplicationUser;

export interface AthleteProfile extends ApplicationUser {
  userId?: string;
  name?: string;
  profilePictureUrl: string;
  language: string;
  currentWeight: number | null;
  height: number | null;
  targetWeight: number | null;
  activityLevel: string | number;
  bio: string;
}

export interface AdminUserGridModel {
  id: string;
  firstName: string;
  lastName: string;
  name: string;
  email: string;
  role: 'Admin' | 'Coach' | 'Athlete';
  plan: 'Basic' | 'Pro' | 'Elite';
  status: 'Active' | 'Expired';
  subscriptionEndDate: string | null;
  profileImageUrl: string;
}

export interface UserDetailResponse extends ApplicationUser {
  lastLoginDate: string | null;
  failedLoginAttempts: number;
  lockoutEnd: string | null;
  lockoutEnabled: boolean;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
}

interface AdminUserApiResponse {
  id?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  avatarUrl?: string;
  profileImageUrl?: string;
  role?: string;
  roles?: string[];
  plan?: string;
  subscriptionPlan?: string;
  subscriptionEndDate?: string | null;
  planEndDate?: string | null;
}

interface UserDetailApiResponse {
  id?: string;
  userName?: string | null;
  email?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  phoneNumber?: string | null;
  profilePictureUrl?: string | null;
  profileImageUrl?: string | null;
  avatarUrl?: string | null;
  roles?: string[];
  isActive?: boolean;
  lastLoginDate?: string | null;
  failedLoginAttempts?: number;
  accessFailedCount?: number;
  lockoutEnd?: string | null;
  lockoutEnabled?: boolean;
  emailConfirmed?: boolean;
  phoneNumberConfirmed?: boolean;
  twoFactorEnabled?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${environment.apiUrl}/Users`;
  private readonly adminUsersUrl = `${environment.apiUrl}/admin/users`;
  private readonly accountMeUrl = `${environment.apiUrl}/Account/me`;

  getUserById(id: string): Observable<UserDetailResponse> {
    return this.http
      .get<UserDetailApiResponse>(`${this.adminUsersUrl}/${id}`)
      .pipe(map((item) => this.mapUserDetail(item)));
  }

  getUsers(): Observable<AdminUserGridModel[]> {
    return this.http
      .get<AdminUserApiResponse[]>(this.adminUsersUrl)
      .pipe(map((items) => items.map((item) => this.mapAdminUser(item))));
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.usersUrl}/${id}`);
  }

  updateUser(id: string, userData: UserUpdateRequest): Observable<ApplicationUser> {
    return this.http.put<ApplicationUser>(`${this.usersUrl}/${id}`, userData);
  }

  getMyProfile(): Observable<AthleteProfile> {
    return this.http.get<AthleteProfile>(this.accountMeUrl);
  }

  updateMyProfile(
    profile: AthleteProfile | { request: AthleteProfile },
  ): Observable<AthleteProfile> {
    return this.http.put<AthleteProfile>(this.accountMeUrl, profile);
  }

  deleteMyProfile(): Observable<void> {
    return this.http.delete<void>(this.accountMeUrl);
  }

  private mapAdminUser(item: AdminUserApiResponse): AdminUserGridModel {
    const firstName = (item.firstName ?? '').trim();
    const lastName = (item.lastName ?? '').trim();
    const fullName = `${firstName} ${lastName}`.trim() || 'Unknown User';

    const resolvedRole = this.normalizeRole(item.role ?? item.roles?.[0] ?? 'Athlete');
    const resolvedPlan = this.normalizePlan(item.plan ?? item.subscriptionPlan ?? 'Basic');
    const endDate = item.subscriptionEndDate ?? item.planEndDate ?? null;

    return {
      id: String(item.id ?? ''),
      firstName,
      lastName,
      name: fullName,
      email: String(item.email ?? ''),
      role: resolvedRole,
      plan: resolvedPlan,
      status: this.resolveStatus(endDate),
      subscriptionEndDate: endDate,
      profileImageUrl: String(item.avatarUrl ?? item.profileImageUrl ?? ''),
    };
  }

  private normalizeRole(value: string): 'Admin' | 'Coach' | 'Athlete' {
    const role = value.trim().toLowerCase();
    if (role.includes('admin')) {
      return 'Admin';
    }

    if (role.includes('coach')) {
      return 'Coach';
    }

    return 'Athlete';
  }

  private normalizePlan(value: string): 'Basic' | 'Pro' | 'Elite' {
    const plan = value.trim().toLowerCase();
    if (plan === 'elite') {
      return 'Elite';
    }

    if (plan === 'pro') {
      return 'Pro';
    }

    return 'Basic';
  }

  private mapUserDetail(item: UserDetailApiResponse): UserDetailResponse {
    const email = String(item.email ?? '');
    const userName = String(item.userName ?? email.split('@')[0] ?? '');

    return {
      id: String(item.id ?? ''),
      userName,
      email,
      firstName: String(item.firstName ?? ''),
      lastName: String(item.lastName ?? ''),
      phoneNumber: String(item.phoneNumber ?? ''),
      profilePictureUrl: String(
        item.profilePictureUrl ?? item.profileImageUrl ?? item.avatarUrl ?? '',
      ),
      roles: Array.isArray(item.roles) ? item.roles : [],
      isActive: item.isActive ?? true,
      lastLoginDate: item.lastLoginDate ?? null,
      failedLoginAttempts: Number(item.failedLoginAttempts ?? item.accessFailedCount ?? 0),
      lockoutEnd: item.lockoutEnd ?? null,
      lockoutEnabled: item.lockoutEnabled ?? false,
      emailConfirmed: item.emailConfirmed ?? false,
      phoneNumberConfirmed: item.phoneNumberConfirmed ?? false,
      twoFactorEnabled: item.twoFactorEnabled ?? false,
    };
  }

  private resolveStatus(subscriptionEndDate: string | null): 'Active' | 'Expired' {
    if (!subscriptionEndDate) {
      return 'Expired';
    }

    return new Date(subscriptionEndDate) >= new Date() ? 'Active' : 'Expired';
  }
}
