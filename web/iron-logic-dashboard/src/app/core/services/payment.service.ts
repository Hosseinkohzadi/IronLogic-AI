import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

export interface SubscriptionPlan {
  id: string;
  name: string;
  monthlyPrice: number;
  yearlyPrice: number;
  taxRate: number;
  features: string[];
  recommended?: boolean;
}

export interface SubscribeRequest {
  planId: string;
  billingCycle: 'monthly' | 'yearly';
  cardName: string;
  cardNumber: string;
  expiry: string;
  cvc: string;
}

export interface SubscribeResponse {
  subscriptionId: string;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<SubscriptionPlan[]>(`${this.baseUrl}/Subscription/plans`);
  }

  subscribe(payload: SubscribeRequest): Observable<SubscribeResponse> {
    return this.http.post<SubscribeResponse>(`${this.baseUrl}/Subscription/subscribe`, payload);
  }
}
