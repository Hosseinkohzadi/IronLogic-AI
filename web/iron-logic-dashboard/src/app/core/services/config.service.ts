import { Injectable, signal } from '@angular/core';
import {
  AiEngineSettings,
  FinancialSettings,
  UserAccessConfig,
  defaultAiEngineSettings,
  defaultFinancialSettings,
  defaultUserAccessConfig,
} from '@core/models';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private readonly aiStorageKey = 'ironlogic.aiEngineSettings';
  private readonly financialStorageKey = 'ironlogic.financialSettings';
  private readonly userAccessStorageKey = 'ironlogic.userAccessConfig';

  readonly aiEngineSettings = signal<AiEngineSettings>(this.loadAiEngineSettings());
  readonly financialSettings = signal<FinancialSettings>(this.loadFinancialSettings());
  readonly userAccessConfig = signal<UserAccessConfig>(this.loadUserAccessConfig());

  updateAiEngineSettings(settings: AiEngineSettings): void {
    this.aiEngineSettings.set(settings);
    this.persistAiEngineSettings(settings);
  }

  updateFinancialSettings(settings: FinancialSettings): void {
    this.financialSettings.set(settings);
    this.persistFinancialSettings(settings);
  }

  updateUserAccessConfig(settings: UserAccessConfig): void {
    this.userAccessConfig.set(settings);
    this.persistUserAccessConfig(settings);
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

  private loadUserAccessConfig(): UserAccessConfig {
    if (!this.hasBrowserStorage()) {
      return defaultUserAccessConfig;
    }

    const raw = localStorage.getItem(this.userAccessStorageKey);
    if (!raw) {
      return defaultUserAccessConfig;
    }

    try {
      const parsed = JSON.parse(raw) as Partial<UserAccessConfig>;
      return {
        ...defaultUserAccessConfig,
        ...parsed,
      };
    } catch {
      return defaultUserAccessConfig;
    }
  }

  private persistUserAccessConfig(settings: UserAccessConfig): void {
    if (!this.hasBrowserStorage()) {
      return;
    }

    localStorage.setItem(this.userAccessStorageKey, JSON.stringify(settings));
  }

  private hasBrowserStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
