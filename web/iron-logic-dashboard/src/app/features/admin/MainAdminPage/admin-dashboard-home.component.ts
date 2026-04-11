import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, of } from 'rxjs';
import { catchError, startWith, switchMap } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LucideAngularModule } from 'lucide-angular';
import { DashboardFilterService } from '@core/services/dashboard-filter.service';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { WorldMapComponent } from '@shared/components/world-map/world-map.component';
import { KpiCardComponent } from '@shared/kpi-card/kpi-card.component';

type KpiTone = 'indigo' | 'emerald' | 'amber' | 'violet';
type FeedType = 'pr' | 'completed' | 'upload' | 'generic';

interface CoachKpiCard {
  title: string;
  value: string;
  badge?: string;
  tone: KpiTone;
  icon: string;
  subtitle: string;
  hasSparkline?: boolean;
  hasRevenueCTA?: boolean;
}

interface ActivityFeedItem {
  id: number;
  athlete: string;
  action: string;
  time: string;
  type: FeedType;
}

interface ExpiringAthlete {
  id: number;
  name: string;
  plan: string;
  expiresIn: string;
  initials: string;
}

interface BlockingActionItem {
  id: number;
  title: string;
  detail: string;
  dueIn: string;
  urgent: boolean;
  primaryAction: 'Approve' | 'View';
  secondaryAction: 'Reject' | 'Dismiss';
}

@Component({
  selector: 'app-admin-dashboard-home',
  imports: [CommonModule, LucideAngularModule, KpiCardComponent, WorldMapComponent],
  templateUrl: './admin-dashboard-home.component.html',
  styleUrl: './admin-dashboard-home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardHomeComponent implements OnInit {
  private api = inject(IronLogicApiService);
  private dashboardFilterService = inject(DashboardFilterService);
  private destroyRef = inject(DestroyRef);

  serverStatus = signal<'OPERATIONAL' | 'DOWN' | 'CHECKING'>('CHECKING');
  remindedSet = signal<Set<number>>(new Set());
  appliedActionSet = signal<Set<string>>(new Set());
  isRevenueDetailOpen = signal(false);
  isNotificationMenuOpen = signal(false);
  selectedCountry = this.dashboardFilterService.selectedCountry;

  private readonly countryMetrics: Record<
    string,
    {
      activeAthletes: number;
      complianceRate: number;
      pendingPrograms: number;
      monthlyRevenue: number;
    }
  > = {
    Canada: { activeAthletes: 10, complianceRate: 84, pendingPrograms: 1, monthlyRevenue: 700 },
    Iran: { activeAthletes: 18, complianceRate: 89, pendingPrograms: 3, monthlyRevenue: 1400 },
    USA: { activeAthletes: 9, complianceRate: 82, pendingPrograms: 2, monthlyRevenue: 800 },
    Germany: { activeAthletes: 5, complianceRate: 86, pendingPrograms: 1, monthlyRevenue: 300 },
  };

  readonly activeAthletesValue = computed(() => {
    const country = this.selectedCountry();
    if (!country) return '42';
    return String(this.countryMetrics[country]?.activeAthletes ?? 0);
  });

  readonly activeAthletesContext = computed(() => {
    const country = this.selectedCountry();
    return country ? `${country} only` : 'Currently enrolled athletes';
  });

  readonly activeAthletesTrend = computed(() => {
    const country = this.selectedCountry();
    return country ? '+1 this week' : '+5 this week';
  });

  readonly complianceRateValue = computed(() => {
    const country = this.selectedCountry();
    if (!country) return '85%';
    return `${this.countryMetrics[country]?.complianceRate ?? 0}%`;
  });

  readonly pendingProgramsValue = computed(() => {
    const country = this.selectedCountry();
    if (!country) return '7';
    return String(this.countryMetrics[country]?.pendingPrograms ?? 0);
  });

  readonly monthlyRevenueValue = computed(() => {
    const country = this.selectedCountry();
    if (!country) return '$3,200';
    return `$${this.countryMetrics[country]?.monthlyRevenue ?? 0}`;
  });

  readonly monthlyRevenueContext = computed(() => {
    const country = this.selectedCountry();
    return country ? `${country} regional revenue` : 'vs $2,940 last month';
  });

  readonly monthlyRevenueTrend = computed(() => {
    const country = this.selectedCountry();
    return country ? '+3.1%' : '+8.8%';
  });

  readonly revenueDetail = {
    current: '$3,200',
    pending: '$1,100',
    potential: '$4,300',
  };

  readonly weeklyExpiring = [
    { id: 1, name: 'Reza T.', plan: 'Elite Athlete', daysLeft: 1, amount: 280, initials: 'RT' },
    {
      id: 2,
      name: 'Dariush K.',
      plan: 'Premium Coaching',
      daysLeft: 2,
      amount: 200,
      initials: 'DK',
    },
    {
      id: 3,
      name: 'Niloofar M.',
      plan: 'Strength Program',
      daysLeft: 4,
      amount: 180,
      initials: 'NM',
    },
    { id: 4, name: 'Babak S.', plan: 'Elite Athlete', daysLeft: 5, amount: 280, initials: 'BS' },
    { id: 5, name: 'Arman F.', plan: 'Basic Coaching', daysLeft: 7, amount: 160, initials: 'AF' },
  ];

  openRevenueDetail(): void {
    this.isRevenueDetailOpen.set(true);
  }
  closeRevenueDetail(): void {
    this.isRevenueDetailOpen.set(false);
  }

  readonly kpiCards: CoachKpiCard[] = [
    {
      title: 'Active Athletes',
      value: '42',
      badge: '+5 this week',
      tone: 'indigo',
      icon: 'users',
      subtitle: 'Currently enrolled athletes',
    },
    {
      title: 'Compliance Rate',
      value: '85%',
      tone: 'emerald',
      icon: 'check',
      subtitle: 'Workouts completed on schedule',
    },
    {
      title: 'Pending Programs',
      value: '7',
      tone: 'amber',
      icon: 'dumbbell',
      subtitle: 'Athletes awaiting new plans',
    },
    {
      title: 'Monthly Revenue',
      value: '$3,200',
      tone: 'violet',
      icon: 'trending-up',
      subtitle: 'vs $2,940 last month',
      hasSparkline: true,
      hasRevenueCTA: true,
    },
  ];

  readonly activityFeed: ActivityFeedItem[] = [
    {
      id: 1,
      athlete: 'Hossein',
      action: 'set a new PR in Deadlift - 180 kg',
      time: '2m ago',
      type: 'pr',
    },
    { id: 2, athlete: 'Kasra', action: 'finished Leg Day', time: '14m ago', type: 'completed' },
    {
      id: 3,
      athlete: 'Reza',
      action: 'uploaded a form check video',
      time: '31m ago',
      type: 'upload',
    },
    {
      id: 4,
      athlete: 'Arman',
      action: 'set a new PR in Bench Press - 100 kg',
      time: '1h ago',
      type: 'pr',
    },
    { id: 5, athlete: 'Matin', action: 'finished Pull Day', time: '2h ago', type: 'completed' },
    { id: 6, athlete: 'Sara', action: 'skipped Shoulder Day', time: '3h ago', type: 'generic' },
  ];

  readonly expiringAthletes: ExpiringAthlete[] = [
    { id: 1, name: 'Dariush K.', plan: 'Premium Coaching', expiresIn: '18h', initials: 'DK' },
    { id: 2, name: 'Niloofar M.', plan: 'Strength Program', expiresIn: '24h', initials: 'NM' },
    { id: 3, name: 'Babak S.', plan: 'Elite Athlete', expiresIn: '41h', initials: 'BS' },
  ];

  readonly blockingActions: BlockingActionItem[] = [
    {
      id: 1,
      title: 'Pending Coach Approvals',
      detail: '3 coach profiles are waiting for verification and final decision.',
      dueIn: 'Now',
      urgent: true,
      primaryAction: 'Approve',
      secondaryAction: 'Reject',
    },
    {
      id: 2,
      title: 'Critical Security Risks',
      detail: '5 lockout bursts detected from a shared IP segment.',
      dueIn: '15m',
      urgent: true,
      primaryAction: 'View',
      secondaryAction: 'Dismiss',
    },
    {
      id: 3,
      title: 'Payment Discrepancies',
      detail: '2 billing records differ from settlement values.',
      dueIn: '1h',
      urgent: true,
      primaryAction: 'View',
      secondaryAction: 'Dismiss',
    },
  ];

  readonly hasBlockingActions = computed(() =>
    this.blockingActions.some((action) => action.urgent),
  );

  sendReminder(id: number): void {
    this.remindedSet.update((prev) => new Set([...prev, id]));
  }

  toggleNotificationsMenu(): void {
    this.isNotificationMenuOpen.update((isOpen) => !isOpen);
  }

  closeNotificationsMenu(): void {
    this.isNotificationMenuOpen.set(false);
  }

  applyBlockingAction(itemId: number, actionLabel: string): void {
    this.appliedActionSet.update((prev) => new Set([...prev, `${itemId}:${actionLabel}`]));
  }

  hasAppliedAction(itemId: number, actionLabel: string): boolean {
    return this.appliedActionSet().has(`${itemId}:${actionLabel}`);
  }

  clearCountrySelection(): void {
    this.dashboardFilterService.setFilter(null);
  }

  ngOnInit() {
    this.startHealthCheck();
  }

  private startHealthCheck() {
    interval(15000)
      .pipe(
        startWith(0),
        switchMap(() => this.api.pingServer().pipe(catchError(() => of(false)))),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((isUp) => {
        this.serverStatus.set(isUp ? 'OPERATIONAL' : 'DOWN');
      });
  }
}
