import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { ApplicationUser } from '@core/models';

export type UserUpdateRequest = ApplicationUser;

export interface AthleteProfile extends ApplicationUser {
  language: string;
  currentWeight: number | null;
  height: number | null;
  targetWeight: number | null;
  activityLevel: string;
  bio: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${environment.apiUrl}/Users`;
  private readonly accountMeUrl = `${environment.apiUrl}/Account/me`;

  getUserById(id: string): Observable<ApplicationUser> {
    return this.http.get<ApplicationUser>(`${this.usersUrl}/${id}`);
  }

  updateUser(id: string, userData: UserUpdateRequest): Observable<ApplicationUser> {
    return this.http.put<ApplicationUser>(`${this.usersUrl}/${id}`, userData);
  }

  getMyProfile(): Observable<AthleteProfile> {
    return this.http.get<AthleteProfile>(this.accountMeUrl);
  }

  updateMyProfile(profile: AthleteProfile): Observable<AthleteProfile> {
    return this.http.put<AthleteProfile>(this.accountMeUrl, profile);
  }
}
