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
  gender?: string | number | null;
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
  gender: string;
  currentWeight: number;
  height: number;
  targetWeight: number;
  activityLevel: string;
  bio: string;
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
  gender?: string | number | null;
  currentWeight?: number | null;
  height?: number | null;
  targetWeight?: number | null;
  activityLevel?: string | number | null;
  bio?: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${environment.apiUrl}/Users`;
  private readonly adminUsersUrl = `${environment.apiUrl}/admin/users`;
  private readonly adminProfilesUrl = `${environment.apiUrl}/admin/profiles`;
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

  getMyProfile(userId?: string): Observable<AthleteProfile> {
    if (userId) {
      return this.http.get<AthleteProfile>(`${this.adminProfilesUrl}/${userId}`);
    }

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
      gender: this.normalizeGender(item.gender),
      currentWeight: Number(item.currentWeight ?? 0),
      height: Number(item.height ?? 0),
      targetWeight: Number(item.targetWeight ?? 0),
      activityLevel: this.normalizeActivityLevel(item.activityLevel),
      bio: String(item.bio ?? ''),
    };
  }

  private normalizeGender(value: string | number | null | undefined): string {
    if (typeof value === 'string' && value.trim()) {
      return value;
    }

    const code = Number(value ?? 0);
    if (code === 1) {
      return 'Male';
    }

    if (code === 2) {
      return 'Female';
    }

    if (code === 3) {
      return 'Other';
    }

    return 'Unknown';
  }

  private normalizeActivityLevel(value: string | number | null | undefined): string {
    if (typeof value === 'string' && value.trim()) {
      return value;
    }

    const code = Number(value ?? 3);
    if (code === 0) {
      return 'None';
    }

    if (code === 1) {
      return 'Sedentary';
    }

    if (code === 2) {
      return 'Lightly Active';
    }

    if (code === 4) {
      return 'Very Active';
    }

    return 'Moderately Active';
  }

  private resolveStatus(subscriptionEndDate: string | null): 'Active' | 'Expired' {
    if (!subscriptionEndDate) {
      return 'Expired';
    }

    return new Date(subscriptionEndDate) >= new Date() ? 'Active' : 'Expired';
  }
}
