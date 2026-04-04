import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { environment } from '@env/environment';
import { Exercise, WorkoutStats } from '@core/models';

@Injectable({
  providedIn: 'root'
})
export class IronLogicApiService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;
  private apiUrl = `${this.baseUrl}/admin/exercises`;
  private statsCache$: Observable<WorkoutStats> | undefined;

  // مدیریت وضعیت با سیگنال‌ها
  exercises = signal<Exercise[]>([]);
  isLoading = signal<boolean>(false);

  /**
   * دریافت آمارهای تمرینی به همراه توصیه‌های مربی (AI Coach)
   */
  getWorkoutStatsWithAdvice(): Observable<WorkoutStats> {
    if (!this.statsCache$) {
      this.statsCache$ = this.http.get<WorkoutStats>(`${this.baseUrl}/workouts/stats`).pipe(
        shareReplay(1)
      );
    }
    return this.statsCache$;
  }

  /**
   * دریافت لیست حرکات ورزشی با پشتیبانی از صفحه‌بندی
   */
  getExercises(pageNumber: number = 1, pageSize: number = 20): Observable<Exercise[]> {
    this.isLoading.set(true);
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Exercise[]>(this.apiUrl, { params }).pipe(
      tap((data: Exercise[]) => { // اضافه شدن تایپ صریح برای رفع خطای TS7006
        this.exercises.set(data);
        this.isLoading.set(false);
      })
    );
  }

  /**
   * جستجوی حرکات بر اساس نام یا عضله هدف
   */
  searchExercises(searchTerm: string): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(`${this.apiUrl}/search`, {
      params: new HttpParams().set('searchTerm', searchTerm)
    });
  }

  /**
   * ایجاد یک حرکت ورزشی جدید
   */
  createExercise(exercise: Exercise): Observable<Exercise> {
    return this.http.post<Exercise>(this.apiUrl, exercise);
  }

  /**
   * ویرایش حرکت موجود
   */
  updateExercise(id: number, exercise: Exercise): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, exercise);
  }

  /**
   * حذف یک حرکت
   */
  deleteExercise(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * وارد کردن دسته‌جمعی حرکات
   */
  bulkImport(exercises: Exercise[]): Observable<{ importedCount: number; message: string }> {
    return this.http.post<{ importedCount: number; message: string }>(
      `${this.apiUrl}/bulk-import`,
      exercises
    );
  }
}
