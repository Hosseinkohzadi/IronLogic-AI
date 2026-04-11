import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, timeout } from 'rxjs/operators';

export interface ConnectionTestResult {
  ok: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class AiEngineConnectionService {
  private readonly http = inject(HttpClient);

  testConnection(apiKey: string, baseUrl: string): Observable<ConnectionTestResult> {
    const normalizedApiKey = String(apiKey ?? '').trim();
    const normalizedBaseUrl = String(baseUrl ?? '')
      .trim()
      .replace(/\/$/, '');

    if (!normalizedApiKey || !normalizedBaseUrl) {
      return of({
        ok: false,
        message: 'API Key and Base URL are required.',
      });
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${normalizedApiKey}`,
      'x-api-key': normalizedApiKey,
    });

    return this.http
      .get(`${normalizedBaseUrl}/health`, {
        observe: 'response',
        responseType: 'text',
        headers,
      })
      .pipe(
        timeout(7000),
        map((response) => ({
          ok: response.status >= 200 && response.status < 300,
          message: 'Connection successful. API key and endpoint are reachable.',
        })),
        catchError(() =>
          of({
            ok: false,
            message: 'Connection failed. Check API key, base URL, and provider health endpoint.',
          }),
        ),
      );
  }
}
