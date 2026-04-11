import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { GridComponent } from '@shared/grid/grid';
import { ColumnConfig } from '@shared/grid/models/column-config';
import { KpiCardComponent } from '@shared/kpi-card/kpi-card.component';
import { ConfigService } from '@core/services';

type PaymentStatus = 'Paid' | 'Pending';
type PlanType = 'Gold' | 'Silver';

interface RevenuePoint {
  month: string;
  amount: number;
}

interface PaymentRecord {
  id: number;
  athleteName: string;
  planType: PlanType;
  amount: number;
  date: string;
  status: PaymentStatus;
}

interface PaymentGridRow {
  id: number;
  athleteName: string;
  planType: PlanType;
  amountLabel: string;
  date: string;
  status: PaymentStatus;
}

@Component({
  selector: 'app-financial-dashboard',
  imports: [CommonModule, LucideAngularModule, GridComponent, KpiCardComponent],
  templateUrl: './financial-dashboard.html',
  styleUrl: './financial-dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FinancialDashboardComponent {
  private readonly configService = inject(ConfigService);

  readonly revenueData: RevenuePoint[] = [
    { month: 'Nov', amount: 11200 },
    { month: 'Dec', amount: 12600 },
    { month: 'Jan', amount: 13850 },
    { month: 'Feb', amount: 14700 },
    { month: 'Mar', amount: 15950 },
    { month: 'Apr', amount: 17100 },
  ];

  readonly payments: PaymentRecord[] = [
    {
      id: 1,
      athleteName: 'Ali Rezaei',
      planType: 'Gold',
      amount: 220,
      date: '2026-04-08',
      status: 'Paid',
    },
    {
      id: 2,
      athleteName: 'Niloofar M.',
      planType: 'Silver',
      amount: 160,
      date: '2026-04-07',
      status: 'Pending',
    },
    {
      id: 3,
      athleteName: 'Kasra Ahmadi',
      planType: 'Gold',
      amount: 220,
      date: '2026-04-05',
      status: 'Paid',
    },
    {
      id: 4,
      athleteName: 'Sara Jafari',
      planType: 'Silver',
      amount: 160,
      date: '2026-04-04',
      status: 'Paid',
    },
    {
      id: 5,
      athleteName: 'Arman Fathi',
      planType: 'Gold',
      amount: 220,
      date: '2026-04-03',
      status: 'Paid',
    },
    {
      id: 6,
      athleteName: 'Babak Sharifi',
      planType: 'Silver',
      amount: 160,
      date: '2026-04-02',
      status: 'Pending',
    },
    {
      id: 7,
      athleteName: 'Dariush Kazemi',
      planType: 'Gold',
      amount: 220,
      date: '2026-03-31',
      status: 'Paid',
    },
    {
      id: 8,
      athleteName: 'Parisa Vahidi',
      planType: 'Silver',
      amount: 160,
      date: '2026-03-30',
      status: 'Paid',
    },
    {
      id: 9,
      athleteName: 'Matin Amini',
      planType: 'Gold',
      amount: 220,
      date: '2026-03-29',
      status: 'Pending',
    },
    {
      id: 10,
      athleteName: 'Mehrdad Nouri',
      planType: 'Silver',
      amount: 160,
      date: '2026-03-28',
      status: 'Paid',
    },
  ];

  readonly financialSettings = computed(() => this.configService.financialSettings());

  readonly transactionColumns = computed<ColumnConfig[]>(() => {
    const currencyCode = this.financialSettings().baseCurrency;
    return [
      {
        field: 'athleteName',
        title: 'ATHLETE NAME',
        type: 'text',
        width: '240px',
        sortable: true,
        locked: true,
        filterType: 'text',
      },
      {
        field: 'planType',
        title: 'PLAN TYPE',
        type: 'badge',
        width: '140px',
        sortable: true,
        filterType: 'select',
        filterOptions: [
          { label: 'Gold', value: 'Gold' },
          { label: 'Silver', value: 'Silver' },
        ],
        badgeStyle: 'financePlan',
      },
      {
        field: 'amountLabel',
        title: `AMOUNT (${currencyCode})`,
        type: 'text',
        width: '170px',
        sortable: true,
        filterType: 'text',
      },
      { field: 'date', title: 'DATE', type: 'date', width: '160px', sortable: true },
      {
        field: 'status',
        title: 'STATUS',
        type: 'badge',
        width: '140px',
        sortable: true,
        filterType: 'select',
        filterOptions: [
          { label: 'Paid', value: 'Paid' },
          { label: 'Pending', value: 'Pending' },
        ],
        badgeStyle: 'financeStatus',
      },
    ];
  });

  readonly paymentRows = computed<PaymentGridRow[]>(() =>
    this.payments.map((payment) => ({
      id: payment.id,
      athleteName: payment.athleteName,
      planType: payment.planType,
      amountLabel: this.formatMoney(payment.amount),
      date: payment.date,
      status: payment.status,
    })),
  );

  readonly monthlyRevenue = computed(() => {
    const latest = this.revenueData[this.revenueData.length - 1];
    return latest?.amount ?? 0;
  });

  readonly yearlyRevenue = computed(() => {
    const sixMonthTotal = this.revenueData.reduce((sum, point) => sum + point.amount, 0);
    return sixMonthTotal * 2;
  });

  readonly activeSubscriptions = computed(
    () => this.payments.filter((payment) => payment.status === 'Paid').length + 28,
  );

  readonly pendingPayments = computed(
    () => this.payments.filter((payment) => payment.status === 'Pending').length,
  );

  readonly churnRate = computed(() => 4.8);

  readonly monthlyRevenueLabel = computed(() => this.formatMoney(this.monthlyRevenue()));
  readonly yearlyRevenueLabel = computed(() => this.formatMoney(this.yearlyRevenue()));
  readonly activeSubscriptionsLabel = computed(() => String(this.activeSubscriptions()));
  readonly pendingLabel = computed(() => String(this.pendingPayments()));

  readonly chartPoints = computed(() => {
    const width = 720;
    const height = 240;
    const padding = 18;
    const max = Math.max(...this.revenueData.map((point) => point.amount));
    const min = Math.min(...this.revenueData.map((point) => point.amount));
    const range = Math.max(1, max - min);
    const stepX = (width - padding * 2) / Math.max(1, this.revenueData.length - 1);

    return this.revenueData.map((point, index) => {
      const x = padding + index * stepX;
      const normalized = (point.amount - min) / range;
      const y = height - padding - normalized * (height - padding * 2);
      return { ...point, x, y };
    });
  });

  readonly chartPath = computed(() => {
    const points = this.chartPoints();
    if (points.length === 0) {
      return '';
    }

    let path = `M ${points[0].x} ${points[0].y}`;
    for (let index = 1; index < points.length; index += 1) {
      const previous = points[index - 1];
      const current = points[index];
      const controlX = (previous.x + current.x) / 2;
      path += ` C ${controlX} ${previous.y}, ${controlX} ${current.y}, ${current.x} ${current.y}`;
    }

    return path;
  });

  readonly areaPath = computed(() => {
    const line = this.chartPath();
    if (!line) {
      return '';
    }

    const width = 720;
    const height = 240;
    const padding = 18;
    return `${line} L ${width - padding} ${height - padding} L ${padding} ${height - padding} Z`;
  });

  formatMoney(amount: number): string {
    const financialSettings = this.financialSettings();
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: financialSettings.baseCurrency,
      currencyDisplay: financialSettings.currencyDisplay,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  formatDate(isoDate: string): string {
    const parsedDate = new Date(`${isoDate}T00:00:00`);
    return parsedDate.toLocaleDateString('en-US', {
      month: 'short',
      day: '2-digit',
      year: 'numeric',
    });
  }
}
