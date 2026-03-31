import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WorkoutStatsResponse } from '@core/models';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class WorkoutService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  /**
   * GET /api/v1/workouts/stats
   * Retrieves monthly workout analytics including total volume,
   * top exercise, intensity score, and session date.
   */
  getStats(): Observable<WorkoutStatsResponse> {
    return this.http.get<WorkoutStatsResponse>(`${this.baseUrl}/workouts/stats`);
  }
}
