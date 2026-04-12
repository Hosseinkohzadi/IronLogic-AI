import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '@core/services/auth.service';
import { SubscriptionPlan, SubscriptionService } from '@core/services/subscription.service';

@Component({
  selector: 'app-plan-selection',
  imports: [CommonModule, CurrencyPipe, LucideAngularModule],
  templateUrl: './plan-selection.html',
  styleUrl: './plan-selection.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlanSelectionComponent {
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly billingCycle = signal<'monthly' | 'yearly'>('monthly');
  readonly selectedPlanId = signal<string | null>(null);

  readonly plans = this.subscriptionService.availablePlans;
  readonly isAdmin = computed(() => this.authService.role() === 'SUPER_ADMIN');

  private readonly allFeatureLabels = [
    'AI workout recommendations',
    'Advanced progress tracking',
    'Priority coach support',
    'Nutrition insights',
    'Custom integrations',
  ];

  constructor() {
    this.loadPlans();
  }

  setBillingCycle(cycle: 'monthly' | 'yearly'): void {
    this.billingCycle.set(cycle);
  }

  selectedPlan = computed(() => {
    const planId = this.selectedPlanId();
    const plans = this.plans();
    if (!planId) {
      return plans[0] ?? null;
    }

    return plans.find((plan) => plan.id === planId) ?? null;
  });

  private readonly normalizedFeatureMap = computed(() => {
    const map = new Map<string, Set<string>>();
    for (const plan of this.plans()) {
      map.set(plan.id, new Set(plan.features.map((feature) => feature.toLowerCase().trim())));
    }

    return map;
  });

  featureRowsFor(planId: string): Array<{ label: string; included: boolean }> {
    const featureSet = this.normalizedFeatureMap().get(planId) ?? new Set<string>();
    return this.allFeatureLabels.map((label) => ({
      label,
      included: featureSet.has(label.toLowerCase()),
    }));
  }

  displayPrice(plan: SubscriptionPlan): number {
    return this.billingCycle() === 'monthly' ? plan.monthlyPrice : (plan.yearlyPrice ?? 0);
  }

  yearlyDiscount(plan: SubscriptionPlan): number {
    const yearlyPrice = plan.yearlyPrice ?? 0;
    if (plan.monthlyPrice <= 0 || yearlyPrice <= 0) {
      return 0;
    }

    const monthlyAnnualized = plan.monthlyPrice * 12;
    const discount = ((monthlyAnnualized - yearlyPrice) / monthlyAnnualized) * 100;
    return Math.max(0, Math.round(discount));
  }

  selectPlan(planId: string): void {
    this.selectedPlanId.set(planId);

    const selected = this.plans().find((plan) => plan.id === planId) ?? null;
    this.subscriptionService.setSelectedPlan(selected);

    if (!this.isAdmin()) {
      void this.router.navigate(['/athlete/subscription/checkout'], {
        queryParams: {
          planId,
          billing: this.billingCycle(),
        },
      });
    }
  }

  goToAdminDashboard(): void {
    void this.router.navigate(['/admin/dashboard']);
  }

  private loadPlans(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.subscriptionService
      .getPlans()
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (plans) => {
          if (plans.length > 0) {
            const defaultPlan =
              plans.find((plan) => plan.name.toLowerCase().includes('pro')) ?? plans[0];
            this.selectedPlanId.set(defaultPlan.id);
            this.subscriptionService.setSelectedPlan(defaultPlan);
          }
        },
        error: () => {
          this.errorMessage.set('Unable to load subscription plans right now. Please retry.');
        },
      });
  }
}
