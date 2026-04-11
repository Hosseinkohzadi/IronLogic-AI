import { Injectable, signal } from '@angular/core';
import { AiEngineSettings, defaultAiEngineSettings } from '@core/models';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private readonly storageKey = 'ironlogic.aiEngineSettings';

  readonly aiEngineSettings = signal<AiEngineSettings>(this.loadAiEngineSettings());

  updateAiEngineSettings(settings: AiEngineSettings): void {
    this.aiEngineSettings.set(settings);
    this.persistAiEngineSettings(settings);
  }

  private loadAiEngineSettings(): AiEngineSettings {
    if (!this.hasBrowserStorage()) {
      return defaultAiEngineSettings;
    }

    const raw = localStorage.getItem(this.storageKey);
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

    localStorage.setItem(this.storageKey, JSON.stringify(settings));
  }

  private hasBrowserStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
