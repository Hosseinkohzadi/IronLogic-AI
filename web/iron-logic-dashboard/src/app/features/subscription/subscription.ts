import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PaymentService, SubscribeRequest, SubscriptionPlan } from '@core/services/payment.service';

@Component({
  selector: 'app-subscription',
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe],
  templateUrl: './subscription.html',
  styleUrl: './subscription.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SubscriptionComponent {
  private readonly paymentService = inject(PaymentService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly plans = signal<SubscriptionPlan[]>([]);
  readonly billingCycle = signal<'monthly' | 'yearly'>('monthly');
  readonly selectedPlanId = signal<string | null>(null);
  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly checkoutForm = this.formBuilder.nonNullable.group({
    cardName: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.minLength(2),
    ]),
    cardNumber: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.pattern(/^\d{16}$/),
    ]),
    expiry: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/),
      this.expiryValidator,
    ]),
    cvc: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.pattern(/^\d{3,4}$/),
    ]),
  });

  readonly selectedPlan = computed(
    () => this.plans().find((plan) => plan.id === this.selectedPlanId()) ?? null,
  );

  readonly planPrice = computed(() => {
    const plan = this.selectedPlan();
    if (!plan) {
      return 0;
    }

    return this.billingCycle() === 'monthly' ? plan.monthlyPrice : plan.yearlyPrice;
  });

  readonly taxAmount = computed(() => {
    const plan = this.selectedPlan();
    if (!plan) {
      return 0;
    }

    return this.planPrice() * (plan.taxRate / 100);
  });

  readonly totalAmount = computed(() => this.planPrice() + this.taxAmount());

  constructor() {
    this.loadPlans();
  }

  setBillingCycle(cycle: 'monthly' | 'yearly'): void {
    this.billingCycle.set(cycle);
  }

  selectPlan(planId: string): void {
    this.selectedPlanId.set(planId);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  onSubmit(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (!this.selectedPlan()) {
      this.errorMessage.set('Please select a subscription plan before checkout.');
      return;
    }

    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      this.errorMessage.set('Please provide valid card details.');
      return;
    }

    const formValue = this.checkoutForm.getRawValue();
    const payload: SubscribeRequest = {
      planId: this.selectedPlan()!.id,
      billingCycle: this.billingCycle(),
      cardName: formValue.cardName,
      cardNumber: formValue.cardNumber,
      expiry: formValue.expiry,
      cvc: formValue.cvc,
    };

    this.isSubmitting.set(true);
    this.paymentService
      .subscribe(payload)
      .pipe(
        finalize(() => this.isSubmitting.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.successMessage.set('Subscription activated successfully.');
          this.checkoutForm.reset({
            cardName: '',
            cardNumber: '',
            expiry: '',
            cvc: '',
          });
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.extractErrorMessage(error));
        },
      });
  }

  private loadPlans(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.paymentService
      .getPlans()
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (plans) => {
          this.plans.set(plans);
          if (plans.length > 0) {
            this.selectedPlanId.set(plans[0].id);
          }
        },
        error: (error: unknown) => {
          // Fallback to mock data if API fails
          const mockPlans: SubscriptionPlan[] = [
            {
              id: 'basic',
              name: 'Basic',
              monthlyPrice: 0,
              yearlyPrice: 0,
              taxRate: 20,
              features: ['Up to 5 athletes', 'Basic workout tracking', 'Monthly reports'],
            },
            {
              id: 'pro',
              name: 'Pro',
              monthlyPrice: 19,
              yearlyPrice: 190,
              taxRate: 20,
              features: [
                'Unlimited athletes',
                'Advanced analytics',
                'AI coaching',
                'Custom reports',
                'Priority support',
              ],
              recommended: true,
            },
            {
              id: 'elite',
              name: 'Elite',
              monthlyPrice: 49,
              yearlyPrice: 490,
              taxRate: 20,
              features: [
                'All Pro features',
                'White-label portal',
                'API access',
                'Dedicated support',
                'Custom integrations',
              ],
            },
          ];

          this.plans.set(mockPlans);
          if (mockPlans.length > 0) {
            this.selectedPlanId.set(mockPlans[1].id); // Select Pro by default
          }

          // Show warning but don't block the UI with mock data
          console.warn('Using mock subscription plans due to API failure:', error);
        },
      });
  }

  private expiryValidator(control: { value: string }): ValidationErrors | null {
    const value = control.value;
    if (!value || !/^(0[1-9]|1[0-2])\/\d{2}$/.test(value)) {
      return null;
    }

    const [monthText, yearText] = value.split('/');
    const month = Number(monthText);
    const year = Number(`20${yearText}`);

    const now = new Date();
    const endOfMonth = new Date(year, month, 0, 23, 59, 59);
    if (Number.isNaN(endOfMonth.getTime()) || endOfMonth < now) {
      return { expiredCard: true };
    }

    return null;
  }

  private extractErrorMessage(error: unknown): string {
    const fallback = 'Unable to process the checkout request. Please try again.';
    if (!error || typeof error !== 'object') {
      return fallback;
    }

    const errorRecord = error as {
      error?: unknown;
      message?: string;
    };

    const payload = errorRecord.error;
    if (typeof payload === 'string' && payload.trim()) {
      return payload;
    }

    if (payload && typeof payload === 'object') {
      const objectPayload = payload as {
        message?: string;
        title?: string;
        errors?: Record<string, string[] | string>;
      };

      if (objectPayload.errors) {
        const firstError = Object.values(objectPayload.errors)
          .flatMap((value) => (Array.isArray(value) ? value : [value]))
          .find((value) => typeof value === 'string' && value.trim());

        if (typeof firstError === 'string') {
          return firstError;
        }
      }

      if (objectPayload.message?.trim()) {
        return objectPayload.message;
      }

      if (objectPayload.title?.trim()) {
        return objectPayload.title;
      }
    }

    if (errorRecord.message?.trim()) {
      return errorRecord.message;
    }

    return fallback;
  }
}
