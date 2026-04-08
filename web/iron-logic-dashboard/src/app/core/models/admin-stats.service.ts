import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { UserAdminStats, UserDetail } from '@core/models/user.model';
import { environment } from '@env/environment';

@Injectable({ providedIn: 'root' })
export class AdminStatsService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/admin`;

  // Signals برای مدیریت وضعیت کلان ادمین
  stats = signal<UserAdminStats | null>(null);

  // دریافت آمار داشبورد (کارت‌های بالایی)
  getDashboardStats(): Observable<UserAdminStats> {
    return this.http.get<UserAdminStats>(`${this.baseUrl}/stats`).pipe(
      tap(data => this.stats.set(data))
    );
  }

  // دریافت جزئیات کامل یک کاربر برای دراور (Drawer)
  getUserDetails(userId: string): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.baseUrl}/users/${userId}`);
  }

  // اکشن‌های مدیریتی سریع
  handleAction(userId: string, action: 'reset' | 'suspend' | 'verify'): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${userId}/action`, { action });
  }
}
