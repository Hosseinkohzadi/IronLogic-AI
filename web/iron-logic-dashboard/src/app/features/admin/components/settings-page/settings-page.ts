import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { take } from 'rxjs';
import { AiEngineConnectionService, ConfigService, ConnectionTestResult } from '@core/services';
import { AiEngineSettings, AiModelId } from '@core/models';

type Currency = 'USD' | 'IRT';

@Component({
  selector: 'app-settings-page',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly configService = inject(ConfigService);
  private readonly aiConnectionService = inject(AiEngineConnectionService);

  readonly coachName = signal('Coach Hossein Karimi');
  readonly coachBio = signal(
    'Strength coach focused on progressive overload, fatigue management, and long-term athlete development.',
  );

  readonly monthlyFee = signal(220);
  readonly currency = signal<Currency>('USD');
  readonly autoRemindAthletes = signal(true);
  readonly showApiKey = signal(false);

  readonly connectionState = signal<'idle' | 'testing' | 'success' | 'error'>('idle');
  readonly connectionMessage = signal('');
  readonly saveMessage = signal('');

  readonly aiModelOptions: ReadonlyArray<{ id: AiModelId; label: string; status: string }> = [
    { id: 'gpt-4o', label: 'GPT-4o', status: 'Fastest' },
    { id: 'claude-3-5-sonnet', label: 'Claude 3.5 Sonnet', status: 'Smartest' },
    { id: 'gemini-1-5-pro', label: 'Gemini 1.5 Pro', status: 'Balanced' },
  ];

  readonly aiSettingsForm = this.createAiSettingsForm();

  readonly selectedModelStatus = computed(() => {
    const selectedModel = this.aiSettingsForm.controls.model.value;
    return this.getModelStatus(selectedModel);
  });

  constructor() {
    this.aiSettingsForm.valueChanges.subscribe(() => {
      if (this.saveMessage()) {
        this.saveMessage.set('');
      }
    });
  }

  private createAiSettingsForm() {
    const settings = this.configService.aiEngineSettings();
    return this.formBuilder.nonNullable.group({
      model: this.formBuilder.nonNullable.control<AiModelId>(settings.model),
      apiKey: this.formBuilder.nonNullable.control(settings.apiKey, [
        Validators.required,
        Validators.minLength(10),
      ]),
      baseUrl: this.formBuilder.nonNullable.control(settings.baseUrl, [Validators.required]),
      maxTokensPerUserMonth: this.formBuilder.nonNullable.control(settings.maxTokensPerUserMonth, [
        Validators.required,
        Validators.min(1000),
      ]),
      contextWindowSize: this.formBuilder.nonNullable.control(settings.contextWindowSize, [
        Validators.required,
        Validators.min(4000),
        Validators.max(128000),
      ]),
      temperature: this.formBuilder.nonNullable.control(settings.temperature, [
        Validators.required,
        Validators.min(0),
        Validators.max(1),
      ]),
      strictFormChecking: this.formBuilder.nonNullable.control(settings.strictFormChecking),
      masterPrompt: this.formBuilder.nonNullable.control(settings.masterPrompt, [
        Validators.required,
        Validators.minLength(20),
      ]),
    });
  }

  updateCoachName(value: string): void {
    this.coachName.set(String(value ?? ''));
  }

  updateCoachBio(value: string): void {
    this.coachBio.set(String(value ?? ''));
  }

  updateMonthlyFee(value: number | string): void {
    const normalized = Number(value);
    this.monthlyFee.set(Number.isFinite(normalized) ? normalized : 0);
  }

  updateCurrency(value: Currency): void {
    this.currency.set(value);
  }

  updateAutoRemind(value: boolean): void {
    this.autoRemindAthletes.set(!!value);
  }

  getModelStatus(model: AiModelId): string {
    const match = this.aiModelOptions.find((option) => option.id === model);
    return match?.status ?? 'Custom';
  }

  toggleApiKeyVisibility(): void {
    this.showApiKey.update((value) => !value);
  }

  testConnection(): void {
    const apiKey = this.aiSettingsForm.controls.apiKey.value;
    const baseUrl = this.aiSettingsForm.controls.baseUrl.value;

    if (!apiKey || !baseUrl) {
      this.aiSettingsForm.controls.apiKey.markAsTouched();
      this.aiSettingsForm.controls.baseUrl.markAsTouched();
      this.connectionState.set('error');
      this.connectionMessage.set('API Key and Base URL are required before testing.');
      return;
    }

    this.connectionState.set('testing');
    this.connectionMessage.set('Verifying API credentials...');

    this.aiConnectionService
      .testConnection(apiKey, baseUrl)
      .pipe(take(1))
      .subscribe((result: ConnectionTestResult) => {
        this.connectionState.set(result.ok ? 'success' : 'error');
        this.connectionMessage.set(result.message);
      });
  }

  saveAll(): void {
    if (this.aiSettingsForm.invalid) {
      this.aiSettingsForm.markAllAsTouched();
      this.saveMessage.set('Please fix AI Engine validation errors before saving.');
      return;
    }

    const aiPayload: AiEngineSettings = this.aiSettingsForm.getRawValue();
    this.configService.updateAiEngineSettings(aiPayload);
    this.saveMessage.set('All settings saved successfully.');
  }
}
