import {
  Component,
  DestroyRef,
  OnInit,
  ChangeDetectionStrategy,
  inject,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Testimonials } from '@features/testimonials/testimonials';
import { SubscriptionPlan, SubscriptionService } from '@core/services/subscription.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, Testimonials, CommonModule, NgOptimizedImage],
  templateUrl: './landing.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingComponent implements OnInit {
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  // Allow Math operations in template
  readonly Math = Math;

  readonly plans = signal<SubscriptionPlan[]>([]);
  readonly billingCycle = signal<'monthly' | 'yearly'>('monthly');
  readonly isLoading = signal(false);
  readonly isError = signal(false);

  readonly selectedPlan = signal<SubscriptionPlan | null>(null);

  ngOnInit(): void {
    this.loadPlans();
  }

  setBillingCycle(cycle: 'monthly' | 'yearly'): void {
    this.billingCycle.set(cycle);
  }

  monthlyPrice(plan: SubscriptionPlan): number {
    return plan.price ?? plan.monthlyPrice ?? 0;
  }

  yearlyPrice(plan: SubscriptionPlan): number {
    if (plan.yearlyPrice && plan.yearlyPrice > 0) {
      return plan.yearlyPrice;
    }

    const monthly = this.monthlyPrice(plan);
    return Math.round(monthly * 12 * 0.8);
  }

  displayPrice(plan: SubscriptionPlan): number {
    return this.billingCycle() === 'monthly' ? this.monthlyPrice(plan) : this.yearlyPrice(plan);
  }

  planCurrency(plan: SubscriptionPlan): string {
    return plan.currency || 'USD';
  }

  yearlyDiscount(plan: SubscriptionPlan): number {
    const monthly = this.monthlyPrice(plan);
    const yearly = this.yearlyPrice(plan);
    if (monthly <= 0 || yearly <= 0) {
      return 0;
    }

    const annualizedMonthly = monthly * 12;
    return Math.max(0, Math.round(((annualizedMonthly - yearly) / annualizedMonthly) * 100));
  }

  getStarted(planId: string): void {
    this.router.navigate(['/register'], {
      queryParams: { planId },
    });
  }

  private loadPlans(): void {
    this.isLoading.set(true);
    this.isError.set(false);

    this.subscriptionService
      .getPlans()
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (plans) => {
          this.plans.set(plans);
          if (plans.length > 1) {
            this.selectedPlan.set(plans[1]); // Select Pro by default
          }
        },
        error: () => {
          this.isError.set(true);
          // Fallback to mock data
          const mockPlans: SubscriptionPlan[] = [
            {
              id: 'starter',
              name: 'Starter',
              price: 9,
              currency: 'USD',
              monthlyPrice: 9,
              yearlyPrice: 90,
              taxRate: 20,
              features: ['3 Workout Routines', 'Basic Analytics', 'Standard AI Coach'],
            },
            {
              id: 'pro',
              name: 'Pro',
              price: 19,
              currency: 'USD',
              monthlyPrice: 19,
              yearlyPrice: 190,
              taxRate: 20,
              features: [
                'Unlimited Programs',
                'Advanced Volume Analytics',
                'Premium AI Advice',
                'Consistency Calendar',
              ],
              recommended: true,
            },
            {
              id: 'elite',
              name: 'Elite',
              price: 39,
              currency: 'USD',
              monthlyPrice: 39,
              yearlyPrice: 390,
              taxRate: 20,
              features: [
                'Unlimited Everything',
                '1-on-1 Human Review',
                'Custom Meal Plans',
                '24/7 Priority Support',
              ],
            },
          ];
          this.plans.set(mockPlans);
          this.selectedPlan.set(mockPlans[1]);
          console.warn('Using mock plans due to backend unavailability');
        },
      });
  }
}
