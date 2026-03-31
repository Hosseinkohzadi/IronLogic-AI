import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CoachAdviceResponse } from '../models/coach-advice-response.model';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CoachService {
  private readonly baseUrl = environment.apiUrl;
  constructor(private readonly http: HttpClient) {}

  /**
   * GET /api/v1/coach/analyze
   * Retrieves AI-driven coaching advice based on the athlete's
   * latest workout stats and body metrics.
   */
  analyze(): Observable<CoachAdviceResponse> {
    return this.http.get<CoachAdviceResponse>(`${this.baseUrl}/coach/analyze`);
  }
}
