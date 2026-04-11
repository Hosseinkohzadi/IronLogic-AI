export type FinancialCurrency = 'USD' | 'CAD' | 'EUR' | 'GBP' | 'AUD';
export type CurrencyDisplayMode = 'symbol' | 'code';
export type PaymentProvider = 'stripe' | 'paypal' | 'manual';

export interface SubscriptionTierSettings {
  name: 'Basic' | 'Pro' | 'Elite';
  monthlyPrice: number;
  annualDiscount: number;
  trialPeriodDays: number;
}

export interface FinancialSettings {
  baseCurrency: FinancialCurrency;
  taxRate: number;
  currencyDisplay: CurrencyDisplayMode;
  tiers: SubscriptionTierSettings[];
  activeProvider: PaymentProvider;
  webhookSecret: string;
  testMode: boolean;
  autoInvoice: boolean;
  churnPrevention: boolean;
}

export const defaultFinancialSettings: FinancialSettings = {
  baseCurrency: 'USD',
  taxRate: 8,
  currencyDisplay: 'symbol',
  tiers: [
    { name: 'Basic', monthlyPrice: 39, annualDiscount: 10, trialPeriodDays: 7 },
    { name: 'Pro', monthlyPrice: 79, annualDiscount: 15, trialPeriodDays: 10 },
    { name: 'Elite', monthlyPrice: 149, annualDiscount: 20, trialPeriodDays: 14 },
  ],
  activeProvider: 'stripe',
  webhookSecret: '',
  testMode: true,
  autoInvoice: true,
  churnPrevention: true,
};
