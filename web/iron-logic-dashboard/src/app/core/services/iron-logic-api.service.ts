import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { environment } from '@env/environment';
import { WorkoutStats } from '@core/models';

@Injectable({
  providedIn: 'root'
})
export class IronLogicApiService {
  private http = inject(HttpClient);
  private statsCache$: Observable<WorkoutStats> | undefined;
  private readonly baseUrl = environment.apiUrl;

  getWorkoutStatsWithAdvice(): Observable<WorkoutStats> {
    if (!this.statsCache$) {
      this.statsCache$ = this.http.get<WorkoutStats>(`${this.baseUrl}/workouts/stats`).pipe(
        shareReplay(1)
      );
    }
    return this.statsCache$;
  }
}
