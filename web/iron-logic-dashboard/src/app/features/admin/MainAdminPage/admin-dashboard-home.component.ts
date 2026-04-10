import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, of } from 'rxjs';
import { catchError, startWith, switchMap } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LucideAngularModule } from 'lucide-angular';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
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

@Component({
  selector: 'app-admin-dashboard-home',
  imports: [CommonModule, LucideAngularModule, KpiCardComponent],
  templateUrl: './admin-dashboard-home.component.html',
  styleUrl: './admin-dashboard-home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardHomeComponent implements OnInit {
  private api = inject(IronLogicApiService);
  private destroyRef = inject(DestroyRef);

  serverStatus = signal<'OPERATIONAL' | 'DOWN' | 'CHECKING'>('CHECKING');
  remindedSet = signal<Set<number>>(new Set());
  isRevenueDetailOpen = signal(false);

  readonly revenueDetail = {
    current: '$3,200',
    pending: '$1,100',
    potential: '$4,300',
  };

  readonly weeklyExpiring = [
    { id: 1, name: 'Reza T.', plan: 'Elite Athlete', daysLeft: 1, amount: 280, initials: 'RT' },
    { id: 2, name: 'Dariush K.', plan: 'Premium Coaching', daysLeft: 2, amount: 200, initials: 'DK' },
    { id: 3, name: 'Niloofar M.', plan: 'Strength Program', daysLeft: 4, amount: 180, initials: 'NM' },
    { id: 4, name: 'Babak S.', plan: 'Elite Athlete', daysLeft: 5, amount: 280, initials: 'BS' },
    { id: 5, name: 'Arman F.', plan: 'Basic Coaching', daysLeft: 7, amount: 160, initials: 'AF' },
  ];

  openRevenueDetail(): void { this.isRevenueDetailOpen.set(true); }
  closeRevenueDetail(): void { this.isRevenueDetailOpen.set(false); }

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
    { id: 1, athlete: 'Hossein', action: 'set a new PR in Deadlift - 180 kg', time: '2m ago', type: 'pr' },
    { id: 2, athlete: 'Kasra', action: 'finished Leg Day', time: '14m ago', type: 'completed' },
    { id: 3, athlete: 'Reza', action: 'uploaded a form check video', time: '31m ago', type: 'upload' },
    { id: 4, athlete: 'Arman', action: 'set a new PR in Bench Press - 100 kg', time: '1h ago', type: 'pr' },
    { id: 5, athlete: 'Matin', action: 'finished Pull Day', time: '2h ago', type: 'completed' },
    { id: 6, athlete: 'Sara', action: 'skipped Shoulder Day', time: '3h ago', type: 'generic' },
  ];

  readonly expiringAthletes: ExpiringAthlete[] = [
    { id: 1, name: 'Dariush K.', plan: 'Premium Coaching', expiresIn: '18h', initials: 'DK' },
    { id: 2, name: 'Niloofar M.', plan: 'Strength Program', expiresIn: '24h', initials: 'NM' },
    { id: 3, name: 'Babak S.', plan: 'Elite Athlete', expiresIn: '41h', initials: 'BS' },
  ];

  sendReminder(id: number): void {
    this.remindedSet.update((prev) => new Set([...prev, id]));
  }

  ngOnInit() {
    this.startHealthCheck();
  }

  private startHealthCheck() {
    interval(15000)
      .pipe(
        startWith(0),
        switchMap(() => this.api.pingServer().pipe(catchError(() => of(false)))),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((isUp) => {
        this.serverStatus.set(isUp ? 'OPERATIONAL' : 'DOWN');
      });
  }
}
