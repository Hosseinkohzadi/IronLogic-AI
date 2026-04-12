import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '@env/environment';

export interface SubscriptionPlan {
  id: string;
  name: string;
  price?: number;
  currency?: string;
  monthlyPrice: number;
  yearlyPrice?: number;
  taxRate: number;
  features: string[];
  recommended?: boolean;
}

export interface UpdatePlanRequest {
  name?: string;
  monthlyPrice?: number;
  yearlyPrice?: number;
  price?: number;
  currency?: string;
}

export interface BillingRecord {
  id: string;
  planId?: string;
  userEmail: string;
  planName: string;
  amount: number;
  currency: string;
  status: 'Paid' | 'Pending' | 'Failed';
  transactionDate: string;
}

interface BillingRecordsResponse {
  transactions?: Array<Record<string, unknown>>;
}

export interface SubscribeRequest {
  planId: string;
  billingCycle?: 'monthly' | 'yearly';
}

export interface SubscribeResponse {
  subscriptionId: string;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class SubscriptionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  readonly availablePlans = signal<SubscriptionPlan[]>([]);
  readonly selectedPlan = signal<SubscriptionPlan | null>(null);

  getPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<Array<Record<string, unknown>>>(`${this.baseUrl}/Subscription/plans`).pipe(
      map((plans) => plans.map((plan, index) => this.normalizePlan(plan, index))),
      tap((plans) => {
        this.availablePlans.set(plans);
        if (!this.selectedPlan() && plans.length > 0) {
          this.selectedPlan.set(plans[0]);
        }
      }),
      catchError((error) => {
        console.error('SubscriptionService.getPlans failed:', error);
        this.availablePlans.set([]);
        return throwError(() => error);
      }),
    );
  }

  setSelectedPlan(plan: SubscriptionPlan | null): void {
    this.selectedPlan.set(plan);
  }

  subscribe(planId: string): Observable<SubscribeResponse> {
    const payload: SubscribeRequest = { planId };

    return this.http
      .post<SubscribeResponse>(`${this.baseUrl}/Subscription/subscribe`, payload)
      .pipe(
        catchError((error) => {
          console.error('SubscriptionService.subscribe failed:', error);
          return throwError(() => error);
        }),
      );
  }

  updatePlan(planId: string, payload: UpdatePlanRequest): Observable<SubscriptionPlan> {
    return this.http
      .put<SubscriptionPlan>(`${this.baseUrl}/Subscription/plans/${planId}`, payload)
      .pipe(
        tap((updatedPlan) => {
          this.availablePlans.update((plans) =>
            plans.map((plan) => (plan.id === updatedPlan.id ? updatedPlan : plan)),
          );
        }),
        catchError((error) => {
          console.error('SubscriptionService.updatePlan failed:', error);
          return throwError(() => error);
        }),
      );
  }

  deletePlan(planId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Subscription/plans/${planId}`).pipe(
      tap(() => {
        this.availablePlans.update((plans) => plans.filter((plan) => plan.id !== planId));
      }),
      catchError((error) => {
        console.error('SubscriptionService.deletePlan failed:', error);
        return throwError(() => error);
      }),
    );
  }

  getBillingRecords(): Observable<BillingRecord[]> {
    return this.http
      .get<
        BillingRecordsResponse | Array<Record<string, unknown>>
      >(`${this.baseUrl}/Subscription/admin/all-transactions`)
      .pipe(
        map((response) => {
          const records = Array.isArray(response)
            ? response
            : Array.isArray(response.transactions)
              ? response.transactions
              : [];

          return records.map((record, index) => {
            const amount = Number(record['amount'] ?? 0);
            const rawStatus = String(record['status'] ?? '').toLowerCase();
            const status: BillingRecord['status'] =
              rawStatus === 'paid' || rawStatus === 'completed'
                ? 'Paid'
                : rawStatus === 'failed'
                  ? 'Failed'
                  : 'Pending';

            return {
              id: String(record['transactionId'] ?? record['id'] ?? `billing-${index + 1}`),
              planId:
                record['planId'] != null && String(record['planId']).trim().length > 0
                  ? String(record['planId'])
                  : undefined,
              userEmail: String(record['userEmail'] ?? record['email'] ?? 'unknown@user.com'),
              planName: String(
                record['planName'] ??
                  record['plan'] ??
                  record['subscriptionPlanName'] ??
                  'Unknown Plan',
              ),
              amount: Number.isFinite(amount) ? amount : 0,
              currency: String(record['currency'] ?? 'USD'),
              status,
              transactionDate: String(
                record['transactionDate'] ?? record['processedAt'] ?? record['createdAt'] ?? '',
              ),
            } satisfies BillingRecord;
          });
        }),
        catchError((error) => {
          console.error('SubscriptionService.getBillingRecords failed:', error);
          return throwError(() => error);
        }),
      );
  }

  private normalizePlan(plan: Record<string, unknown>, index: number): SubscriptionPlan {
    const monthlyPrice = this.toNumber(plan['monthlyPrice'] ?? plan['price']);
    const yearlyPrice = this.toNumber(plan['yearlyPrice'] ?? plan['annualPrice']);
    const features = Array.isArray(plan['features'])
      ? plan['features'].filter((feature): feature is string => typeof feature === 'string')
      : [];

    return {
      id: String(plan['id'] ?? `plan-${index + 1}`),
      name: String(plan['name'] ?? `Plan ${index + 1}`),
      price: this.toNumber(plan['price']),
      currency: String(plan['currency'] ?? 'USD'),
      monthlyPrice,
      yearlyPrice,
      taxRate: this.toNumber(plan['taxRate']),
      features,
      recommended: Boolean(plan['recommended']),
    };
  }

  private toNumber(value: unknown): number {
    const parsed = Number(value ?? 0);
    return Number.isFinite(parsed) ? parsed : 0;
  }
}
