import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

export type MarketingAudience = 'AllUsers' | 'BasicPlanUsersOnly';

interface AudienceCountResponse {
  count: number;
}

@Injectable({
  providedIn: 'root',
})
export class MarketingService {
  private readonly http = inject(HttpClient);
  private readonly marketingUrl = `${environment.apiUrl}/Marketing`;

  getAudienceCount(audience: MarketingAudience): Observable<AudienceCountResponse> {
    const params = new HttpParams().set('audience', audience);
    return this.http.get<AudienceCountResponse>(`${this.marketingUrl}/audience-count`, { params });
  }

  broadcastDiscount(
    audience: MarketingAudience,
    currentDiscount: number,
    customMessage?: string,
  ): Observable<void> {
    return this.http.post<void>(`${this.marketingUrl}/broadcast-discount`, {
      audience,
      currentDiscount,
      customMessage: customMessage?.trim() || undefined,
    });
  }
}
