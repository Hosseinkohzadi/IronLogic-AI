import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { environment } from '@env/environment';
import { Exercise, WorkoutStats } from '../models'; // استفاده از index.ts برای ایمپورت تمیز

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

    return this.http.get<{totalCount: number, items: Exercise[]}>(this.adminUrl, { params }).pipe(
      tap(response => {
        // ذخیره لیست حرکات صفحه فعلی
        this.exercises.set(response.items);
        // ذخیره تعداد کل حرکات دیتابیس
        this.totalExercises.set(response.totalCount);
        this.isLoading.set(false);
      })
    );
  }

  /**
   * جستجوی حرکات (Admin)
   */
  searchExercises(searchTerm: string): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(`${this.adminUrl}/search`, {
      params: new HttpParams().set('searchTerm', searchTerm)
    });
  }

  /**
   * حذف حرکت (Admin) - استفاده از string برای GUID مطابق Swagger
   */
  deleteExercise(id: string): Observable<void> {
    return this.http.delete<void>(`${this.adminUrl}/${id}`);
  }

  /**
   * وارد کردن دسته‌جمعی (Admin)
   */
  bulkImport(exercises: Exercise[]): Observable<{ importedCount: number; message: string }> {
    return this.http.post<{ importedCount: number; message: string }>(
      `${this.adminUrl}/bulk-import`,
      exercises
    );
  }
}
