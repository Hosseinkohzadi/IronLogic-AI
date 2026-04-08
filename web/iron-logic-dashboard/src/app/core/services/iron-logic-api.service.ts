import {inject, Injectable, signal} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable, of, shareReplay, tap} from 'rxjs';
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

  /**
   * دریافت آمارهای تمرینی و توصیه‌های هوش مصنوعی (AI Coach)
   * مطابق با مدل WorkoutStats و فیلد advice: { advice: string }
   */
  getWorkoutStatsWithAdvice(): Observable<WorkoutStats> {
    if (!this.statsCache$) {
      this.statsCache$ = this.http.get<WorkoutStats>(this.statsUrl).pipe(
        shareReplay(1)
      );
    }
    return this.statsCache$;
  }

// اضافه کردن یک سیگنال جدید برای تعداد کل
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

  private mockUsers: UserRow[] = [
    { id: 'USR-1001', name: 'Hossein K.', email: 'hossein@example.com', status: 'Active', tier: 'Pro', sessions: 184, weights: 121, lastSeen: '2h ago', emailConfirmed: true },
    { id: 'USR-1002', name: 'Marcus Lee', email: 'marcus@example.com', status: 'Review', tier: 'Elite', sessions: 96, weights: 78, lastSeen: '5h ago', emailConfirmed: true },
    { id: 'USR-1003', name: 'Sara Bennett', email: 'sara@example.com', status: 'Suspended', tier: 'Basic', sessions: 41, weights: 22, lastSeen: '1d ago', emailConfirmed: false },
    { id: 'USR-1004', name: 'Daniel Park', email: 'daniel@example.com', status: 'Active', tier: 'Elite', sessions: 210, weights: 144, lastSeen: '12m ago', emailConfirmed: true },
  ];

  getUsers() {
    return of(this.mockUsers);
  }
}
