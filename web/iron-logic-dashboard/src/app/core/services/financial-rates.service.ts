import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

export interface FinancialRatesSyncResult {
  taxRate: number;
  source: string;
  syncedAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class FinancialRatesService {
  syncRates(baseCurrency: string): Observable<FinancialRatesSyncResult> {
    const rateByCurrency: Record<string, number> = {
      USD: 8,
      CAD: 13,
      EUR: 20,
      GBP: 20,
      AUD: 10,
    };

    return of({
      taxRate: rateByCurrency[baseCurrency] ?? 8,
      source: 'FinanceOps Mock Feed',
      syncedAt: new Date().toISOString(),
    }).pipe(delay(700));
  }
}
