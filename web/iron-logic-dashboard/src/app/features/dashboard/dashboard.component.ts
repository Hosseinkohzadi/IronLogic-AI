import {Component, computed, inject, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {toSignal} from '@angular/core/rxjs-interop';
import {TrainingDurationComponent} from './components/training-duration/training-duration.component';
import {CalendarComponent} from './components/calendar/calendar.component';
import {IronLogicApiService, WorkoutService} from '@core/services';
import {AiCoachCardComponent, MetricCardComponent} from '@shared/ui';
// مسیر ایمپورت کامپوننت کشویی (اگر نام فایل در پروژه شما فرق دارد، اصلاحش کنید)
import {DayDetailsComponent} from './components/day-details/day-details';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MetricCardComponent,
    AiCoachCardComponent,
    TrainingDurationComponent,
    CalendarComponent,
    DayDetailsComponent // 🚀 ثبت کامپوننت در ماژول
  ],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {
  private api = inject(IronLogicApiService);
  private workoutService = inject(WorkoutService);

  // دریافت دیتای اصلی داشبورد
  private statsData = toSignal(this.api.getWorkoutStatsWithAdvice());

  loading = computed(() => !this.statsData());
  stats = computed(() => this.statsData());

  advice = computed(() => {
    const data = this.statsData();
    return data?.advice?.advice || null;
  });

  workouts = computed(() => {
    const data = this.statsData();
    if (!data || !data.dailyWorkouts) return [];

    return data.dailyWorkouts.map((dw: any) => ({
      date: dw.date,
      sessions: (dw.workoutSessionDtos || []).map((s: any) => ({
        id: s.id,
        title: s.title || 'Workout Session',
        duration: s.duration === 'Time N/A' ? '--' : s.duration
      }))
    }));
  });

  // ==========================================
  // منطق مربوط به پنل کشویی (Offcanvas)
  // ==========================================
  isOffcanvasOpen = signal(false);
  selectedDate = signal<Date | null>(null);
  selectedDateSessions = signal<any[]>([]);
  isLoadingSessions = signal(false);

  // 🚀 سیگنال جدید برای مدیریت حالت پین
  isPinned = signal(false);

  togglePin() {
    this.isPinned.update(v => !v);
  }

  // این متد توسط تقویم صدا زده می‌شود
  openDayDetails(dateInput: Date | string) {
    const date = new Date(dateInput);
    this.selectedDate.set(date);
    this.isOffcanvasOpen.set(true);
    this.isLoadingSessions.set(true);

    const dateString = new Date(date.getTime() - (date.getTimezoneOffset() * 60000))
      .toISOString()
      .split('T')[0];

    this.workoutService.getSessionsByDate(dateString).subscribe({
      next: (sessions) => {
        this.selectedDateSessions.set(sessions);
        this.isLoadingSessions.set(false);
      },
      error: (err) => {
        console.error('Error loading sessions:', err);
        this.isLoadingSessions.set(false);
      }
    });
  }

  closeOffcanvas() {
    this.isOffcanvasOpen.set(false);
    this.isPinned.set(false); // وقتی پنل بسته میشه، از حالت پین در بیاد
  }
}
