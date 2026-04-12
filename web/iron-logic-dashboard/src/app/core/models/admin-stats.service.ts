import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { UserAdminStats, UserDetail } from '@core/models/user.model';
import { environment } from '@env/environment';

@Injectable({ providedIn: 'root' })
export class AdminStatsService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/admin`;

  // Signals for managing admin-level state
  stats = signal<UserAdminStats | null>(null);

  // Retrieve dashboard stats (top KPI cards)
  getDashboardStats(): Observable<UserAdminStats> {
    return this.http.get<UserAdminStats>(`${this.baseUrl}/stats`).pipe(
      tap(data => this.stats.set(data))
    );
  }

  // Retrieve full user details for drawer (Drawer)
  getUserDetails(userId: string): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.baseUrl}/users/${userId}`);
  }

  // Quick admin actions
  handleAction(userId: string, action: 'reset' | 'suspend' | 'verify'): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${userId}/action`, { action });
  }
}
