import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, shareReplay } from 'rxjs';

export interface WorkoutStats {
  totalVolume: number;
  topExercise: string;
  intensityScore: number;
  sessionDate: string;
  advice: {
    advice: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class IronLogicApiService {
  private http = inject(HttpClient);
  private statsCache$: Observable<WorkoutStats> | undefined;

  getWorkoutStatsWithAdvice(): Observable<WorkoutStats> {
    if (!this.statsCache$) {
      this.statsCache$ = this.http.get<WorkoutStats>('/api/workout/stats').pipe(
        shareReplay(1)
      );
    }
    return this.statsCache$;
  }
}
