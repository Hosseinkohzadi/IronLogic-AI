import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';

import { WorkoutStats } from '../models/workout-stats.model';

@Injectable({
  providedIn: 'root'
})
export class IronLogicApiService {
  private http = inject(HttpClient);
  private statsCache$: Observable<WorkoutStats> | undefined;

  getWorkoutStatsWithAdvice(): Observable<WorkoutStats> {
    if (!this.statsCache$) {
      this.statsCache$ = this.http.get<WorkoutStats>('http://localhost:5010/api/v1/workouts/stats').pipe(
        shareReplay(1)
      );
    }
    return this.statsCache$;
  }
}
