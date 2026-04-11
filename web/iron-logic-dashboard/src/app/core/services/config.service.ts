import { Injectable, signal } from '@angular/core';
import {
  AiEngineSettings,
  FinancialSettings,
  defaultAiEngineSettings,
  defaultFinancialSettings,
} from '@core/models';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private readonly aiStorageKey = 'ironlogic.aiEngineSettings';
  private readonly financialStorageKey = 'ironlogic.financialSettings';

  readonly aiEngineSettings = signal<AiEngineSettings>(this.loadAiEngineSettings());
  readonly financialSettings = signal<FinancialSettings>(this.loadFinancialSettings());

  updateAiEngineSettings(settings: AiEngineSettings): void {
    this.aiEngineSettings.set(settings);
    this.persistAiEngineSettings(settings);
  }

  updateFinancialSettings(settings: FinancialSettings): void {
    this.financialSettings.set(settings);
    this.persistFinancialSettings(settings);
  }

  private loadAiEngineSettings(): AiEngineSettings {
    if (!this.hasBrowserStorage()) {
      return defaultAiEngineSettings;
    }

    const raw = localStorage.getItem(this.aiStorageKey);
    if (!raw) {
      return defaultAiEngineSettings;
    }

    try {
      const parsed = JSON.parse(raw) as Partial<AiEngineSettings>;
      return {
        ...defaultAiEngineSettings,
        ...parsed,
      };
    } catch {
      return defaultAiEngineSettings;
    }
  }

  private persistAiEngineSettings(settings: AiEngineSettings): void {
    if (!this.hasBrowserStorage()) {
      return;
    }

    localStorage.setItem(this.aiStorageKey, JSON.stringify(settings));
  }

  private loadFinancialSettings(): FinancialSettings {
    if (!this.hasBrowserStorage()) {
      return defaultFinancialSettings;
    }

    const raw = localStorage.getItem(this.financialStorageKey);
    if (!raw) {
      return defaultFinancialSettings;
    }

    try {
      const parsed = JSON.parse(raw) as Partial<FinancialSettings>;
      return {
        ...defaultFinancialSettings,
        ...parsed,
      };
    } catch {
      return defaultFinancialSettings;
    }
  }

  private persistFinancialSettings(settings: FinancialSettings): void {
    if (!this.hasBrowserStorage()) {
      return;
    }

    localStorage.setItem(this.financialStorageKey, JSON.stringify(settings));
  }

  private hasBrowserStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
