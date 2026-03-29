import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { TrainingDurationComponent } from './components/training-duration/training-duration.component';
import { CalendarComponent } from './components/calendar/calendar.component';
import { IronLogicApiService, CoachService } from '@core/services';
import { MetricCardComponent, AiCoachCardComponent } from '@shared/ui';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MetricCardComponent,
    AiCoachCardComponent,
    TrainingDurationComponent,
    CalendarComponent
  ],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {
  private api = inject(IronLogicApiService);

  // ۱. دریافت دیتا از API (پورت 5011 با HTTPS)
  private statsData = toSignal(this.api.getWorkoutStatsWithAdvice());

  // ۲. وضعیت لودینگ
  loading = computed(() => !this.statsData());

  // ۳. استخراج آمار اصلی (Volume, Intensity, etc.)
  stats = computed(() => this.statsData());

  // ۴. استخراج متن مشاوره (با توجه به ساختار { advice: "..." } در جیسون)
  advice = computed(() => {
    const data = this.statsData();
    // چون بک‌اِند دیتا را به صورت یک آبجکت می‌فرستد: data.advice.advice
    return data?.advice?.advice || null;
  });

  // ۵. نگاشت دیتای بک‌اِند به ساختار تقویم (اصلاح نام فیلد به workoutSessionDtos)
  workouts = computed(() => {
    const data = this.statsData();

    if (!data || !data.dailyWorkouts) return [];

    return data.dailyWorkouts.map(dw => ({
      date: dw.date,
      // تغییر نام از sessions به workoutSessionDtos برای هماهنگی با جیسون بک‌اِند
      sessions: (dw.workoutSessionDtos || []).map((s: any) => ({
        id: s.id,
        title: s.title || 'Workout Session',
        // مدیریت هوشمند زمان‌های N/A
        duration: s.duration === 'Time N/A' ? '--' : s.duration
      }))
    }));
  });
}
