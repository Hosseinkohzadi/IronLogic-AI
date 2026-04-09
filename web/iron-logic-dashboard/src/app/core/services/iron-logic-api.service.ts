import {inject, Injectable, signal} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {delay, Observable, of, shareReplay, tap} from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import {environment} from '@env/environment';
import {Exercise, WorkoutStats} from '../models';
import {UserRow} from '@core/models';

@Injectable({
  providedIn: 'root'
})
export class IronLogicApiService {
  private http = inject(HttpClient);

  // آدرس‌های پایه بر اساس فایل OpenAPI
  private readonly baseUrl = environment.apiUrl;
  private adminUrl = `${this.baseUrl}/admin/exercises`;
  private statsUrl = `${this.baseUrl}/Workouts/stats`;

  // مدیریت وضعیت (State Management)
  exercises = signal<Exercise[]>([]);
  isLoading = signal<boolean>(false);
  private statsCache$: Observable<WorkoutStats> | undefined;

  getWorkoutStatsWithAdvice(): Observable<WorkoutStats> {
    if (!this.statsCache$) {
      this.statsCache$ = this.http.get<WorkoutStats>(this.statsUrl).pipe(
        shareReplay(1)
      );
    }
    return this.statsCache$;
  }

  totalExercises = signal<number>(0);

  getExercises(pageNumber: number = 1, pageSize: number = 20): Observable<any> {
    this.isLoading.set(true);
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<{ totalCount: number, items: Exercise[] }>(this.adminUrl, {params}).pipe(
      tap(response => {
        // ذخیره لیست حرکات صفحه فعلی
        this.exercises.set(response.items);
        // ذخیره تعداد کل حرکات دیتابیس
        this.totalExercises.set(response.totalCount);
        this.isLoading.set(false);
      })
    );
  }

  searchExercises(searchTerm: string): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(`${this.adminUrl}/search`, {
      params: new HttpParams().set('searchTerm', searchTerm)
    });
  }

  deleteExercise(id: string): Observable<void> {
    return this.http.delete<void>(`${this.adminUrl}/${id}`);
  }

  bulkImport(exercises: Exercise[]): Observable<{ importedCount: number; message: string }> {
    return this.http.post<{ importedCount: number; message: string }>(
      `${this.adminUrl}/bulk-import`,
      exercises
    );
  }

  pingServer() {
    const healthUrl = this.baseUrl.replace('/v1', '');

    return this.http.get(`${healthUrl}/health`, {
      observe: 'response',
      responseType: 'text'
    }).pipe(
      map(response => response.status === 200),
      catchError((error) => {
        console.error('Health Check Error:', error);
        return of(false);
      })
    );
  }

getUsers(): Observable<UserRow[]> {
  const mockUsers: UserRow[] = [
    { 
      id: 'USR-1001', 
      name: 'Hossein K.', 
      userName: 'hossein_k', 
      email: 'hossein@example.com', 
      profileImageUrl: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100&h=100&fit=crop',
      emailConfirmed: true,
      phoneNumberConfirmed: true, 
      twoFactorEnabled: true,     
      accessFailedCount: 0,       
      status: 'Active', 
      tier: 'Pro', 
      sessions: 184,
      weights: 121,
      // لاگین در تاریخ امروز - ساعت 10:15 صبح
      lastSeen: '2026-04-09T10:15:00Z' 
    },
    { 
      id: 'USR-1002', 
      name: 'Marcus Lee', 
      userName: 'marcus_lee',
      email: 'marcus@example.com', 
      profileImageUrl: 'https://images.unsplash.com/photo-1599566150163-29194dcaad36?w=100&h=100&fit=crop',
      emailConfirmed: false,      
      phoneNumberConfirmed: false,
      twoFactorEnabled: false,
      accessFailedCount: 1,
      status: 'Review', 
      tier: 'Elite', 
      sessions: 96,
      weights: 78,
      // لاگین در تاریخ دیروز - ساعت 22:45 شب
      lastSeen: '2026-04-08T22:45:00Z'
    },
    { 
      id: 'USR-1003', 
      name: 'Sara Bennett', 
      userName: 'sara_b',
      email: 'sara@example.com', 
      profileImageUrl: '', // بدون عکس برای تست حروف اول اسم (Initials)
      emailConfirmed: false,
      phoneNumberConfirmed: false,
      twoFactorEnabled: false,
      accessFailedCount: 5,       
      lockoutEnd: '2026-04-10T00:00:00Z', 
      status: 'Suspended', 
      tier: 'Basic', 
      sessions: 41,
      weights: 22,
      // لاگین هفته گذشته
      lastSeen: '2026-04-02T14:20:00Z'
    },
    { 
      id: 'USR-1004', 
      name: 'Daniel Park', 
      userName: 'dpark99',
      email: 'daniel@example.com', 
      profileImageUrl: 'https://images.unsplash.com/photo-1527980965255-d3b416303d12?w=100&h=100&fit=crop',
      emailConfirmed: true,
      phoneNumberConfirmed: true,
      twoFactorEnabled: true,
      accessFailedCount: 0,
      status: 'Active', 
      tier: 'Elite', 
      sessions: 210,
      weights: 144,
      lastSeen: '2026-04-09T00:13:00Z'
    }
  ];

  return of(mockUsers).pipe(delay(500));
}
}
