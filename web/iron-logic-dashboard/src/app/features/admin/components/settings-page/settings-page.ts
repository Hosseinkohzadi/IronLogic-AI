import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { take } from 'rxjs';
import {
  AiEngineConnectionService,
  ConfigService,
  ConnectionTestResult,
  FinancialRatesService,
} from '@core/services';
import {
  AiEngineSettings,
  AiModelId,
  FinancialCurrency,
  FinancialSettings,
  PaymentProvider,
  SubscriptionTierSettings,
} from '@core/models';

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
  private readonly financialRatesService = inject(FinancialRatesService);

  readonly coachName = signal('Coach Hossein Karimi');
  readonly coachBio = signal(
    'Strength coach focused on progressive overload, fatigue management, and long-term athlete development.',
  );

  readonly showApiKey = signal(false);
  readonly showWebhookSecret = signal(false);

  readonly connectionState = signal<'idle' | 'testing' | 'success' | 'error'>('idle');
  readonly connectionMessage = signal('');
  readonly syncState = signal<'idle' | 'syncing' | 'success' | 'error'>('idle');
  readonly syncMessage = signal('');
  readonly saveMessage = signal('');

  readonly currencyOptions: ReadonlyArray<FinancialCurrency> = ['USD', 'CAD', 'EUR', 'GBP', 'AUD'];
  readonly providerOptions: ReadonlyArray<{ label: string; value: PaymentProvider }> = [
    { label: 'Stripe', value: 'stripe' },
    { label: 'PayPal', value: 'paypal' },
    { label: 'Manual', value: 'manual' },
  ];

  readonly aiModelOptions: ReadonlyArray<{ id: AiModelId; label: string; status: string }> = [
    { id: 'gpt-4o', label: 'GPT-4o', status: 'Fastest' },
    { id: 'claude-3-5-sonnet', label: 'Claude 3.5 Sonnet', status: 'Smartest' },
    { id: 'gemini-1-5-pro', label: 'Gemini 1.5 Pro', status: 'Balanced' },
  ];

  readonly aiSettingsForm = this.createAiSettingsForm();
  readonly financialSettingsForm = this.createFinancialSettingsForm();

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

    this.financialSettingsForm.valueChanges.subscribe(() => {
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

  private createFinancialSettingsForm() {
    const settings = this.configService.financialSettings();
    return this.formBuilder.nonNullable.group({
      baseCurrency: this.formBuilder.nonNullable.control<FinancialCurrency>(settings.baseCurrency),
      taxRate: this.formBuilder.nonNullable.control(settings.taxRate, [
        Validators.required,
        Validators.min(0),
        Validators.max(40),
      ]),
      currencyDisplay: this.formBuilder.nonNullable.control<'symbol' | 'code'>(
        settings.currencyDisplay,
      ),
      activeProvider: this.formBuilder.nonNullable.control<PaymentProvider>(
        settings.activeProvider,
      ),
      webhookSecret: this.formBuilder.nonNullable.control(settings.webhookSecret, [
        Validators.required,
        Validators.minLength(8),
      ]),
      testMode: this.formBuilder.nonNullable.control(settings.testMode),
      autoInvoice: this.formBuilder.nonNullable.control(settings.autoInvoice),
      churnPrevention: this.formBuilder.nonNullable.control(settings.churnPrevention),
      tiers: this.formBuilder.array(
        settings.tiers.map((tier) =>
          this.formBuilder.nonNullable.group({
            name: this.formBuilder.nonNullable.control<SubscriptionTierSettings['name']>(tier.name),
            monthlyPrice: this.formBuilder.nonNullable.control(tier.monthlyPrice, [
              Validators.required,
              Validators.min(0),
            ]),
            annualDiscount: this.formBuilder.nonNullable.control(tier.annualDiscount, [
              Validators.required,
              Validators.min(0),
              Validators.max(90),
            ]),
            trialPeriodDays: this.formBuilder.nonNullable.control(tier.trialPeriodDays, [
              Validators.required,
              Validators.min(0),
              Validators.max(60),
            ]),
          }),
        ),
      ),
    });
  }

  get tierControls() {
    return this.financialSettingsForm.controls.tiers.controls;
  }

  updateCoachName(value: string): void {
    this.coachName.set(String(value ?? ''));
  }

  updateCoachBio(value: string): void {
    this.coachBio.set(String(value ?? ''));
  }

  getModelStatus(model: AiModelId): string {
    const match = this.aiModelOptions.find((option) => option.id === model);
    return match?.status ?? 'Custom';
  }

  toggleApiKeyVisibility(): void {
    this.showApiKey.update((value) => !value);
  }

  toggleWebhookSecretVisibility(): void {
    this.showWebhookSecret.update((value) => !value);
  }

  setCurrencyDisplay(mode: 'symbol' | 'code'): void {
    this.financialSettingsForm.controls.currencyDisplay.setValue(mode);
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

  syncRates(): void {
    const baseCurrency = this.financialSettingsForm.controls.baseCurrency.value;
    this.syncState.set('syncing');
    this.syncMessage.set('Syncing tax and conversion feeds...');

    this.financialRatesService
      .syncRates(baseCurrency)
      .pipe(take(1))
      .subscribe({
        next: (result) => {
          this.financialSettingsForm.controls.taxRate.setValue(result.taxRate);
          this.syncState.set('success');
          this.syncMessage.set(`Rates synced from ${result.source}.`);
        },
        error: () => {
          this.syncState.set('error');
          this.syncMessage.set('Rate sync failed. Please retry.');
        },
      });
  }

  saveAll(): void {
    if (this.aiSettingsForm.invalid || this.financialSettingsForm.invalid) {
      this.aiSettingsForm.markAllAsTouched();
      this.financialSettingsForm.markAllAsTouched();
      this.saveMessage.set('Please fix form validation errors before saving.');
      return;
    }

    const aiPayload: AiEngineSettings = this.aiSettingsForm.getRawValue();
    const financialPayload: FinancialSettings = this.financialSettingsForm.getRawValue();

    this.configService.updateAiEngineSettings(aiPayload);
    this.configService.updateFinancialSettings(financialPayload);
    this.saveMessage.set('All settings saved successfully.');
  }
}
