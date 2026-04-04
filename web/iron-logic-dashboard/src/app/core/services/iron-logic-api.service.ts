import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Exercise } from '../models/workout.model';

@Injectable({
  providedIn: 'root'
})
export class IronLogicApiService {
  private http = inject(HttpClient);
  // استفاده از backtick برای Template Literal
  private apiUrl = `${environment.apiUrl}/admin/exercises`;

  exercises = signal<Exercise[]>([]);
  isLoading = signal<boolean>(false);

  getExercises(pageNumber: number = 1, pageSize: number = 20): Observable<Exercise[]> {
    this.isLoading.set(true);
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Exercise[]>(this.apiUrl, { params }).pipe(
      tap(data => {
        this.exercises.set(data);
        this.isLoading.set(false);
      })
    );
  }

  searchExercises(searchTerm: string): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(`${this.apiUrl}/search`, {
      params: new HttpParams().set('searchTerm', searchTerm)
    });
  }

  createExercise(exercise: Exercise): Observable<Exercise> {
    return this.http.post<Exercise>(this.apiUrl, exercise);
  }

  updateExercise(id: number, exercise: Exercise): Observable<void> {
    // اضافه شدن ID به انتهای مسیر
    return this.http.put<void>(`${this.apiUrl}/${id}`, exercise);
  }

  deleteExercise(id: number): Observable<void> {
    // اضافه شدن ID به انتهای مسیر
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  bulkImport(exercises: Exercise[]): Observable<{ importedCount: number; message: string }> {
    return this.http.post<{ importedCount: number; message: string }>(
      `${this.apiUrl}/bulk-import`,
      exercises
    );
  }
}
