import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

export interface PlatformSettings {
  YearlyDiscountPercentage: number;
}

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  private readonly http = inject(HttpClient);
  private readonly settingsUrl = `${environment.apiUrl}/Settings`;

  getPlatformSettings(): Observable<PlatformSettings> {
    return this.http.get<PlatformSettings>(`${this.settingsUrl}/platform`);
  }

  getPublicPricingConfig(): Observable<PlatformSettings> {
    return this.http.get<PlatformSettings>(`${this.settingsUrl}/public-pricing`);
  }

  updateSetting(key: 'YearlyDiscountPercentage', value: number): Observable<void> {
    return this.http.put<void>(`${this.settingsUrl}/${key}`, { value });
  }
}
