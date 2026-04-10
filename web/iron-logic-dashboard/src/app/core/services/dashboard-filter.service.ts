import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class DashboardFilterService {
  selectedCountry = signal<string | null>(null);

  setFilter(countryId: string | null): void {
    const current = this.selectedCountry();
    this.selectedCountry.set(current === countryId ? null : countryId);
  }
}
