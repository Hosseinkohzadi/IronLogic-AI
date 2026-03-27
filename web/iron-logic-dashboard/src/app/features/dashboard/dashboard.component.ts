import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, tap } from 'rxjs';
import { IronLogicApiService } from '../../core/services/iron-logic-api.service';
import { MetricCardComponent } from '../../shared/ui/metric-card/metric-card.component';
import { AiCoachCardComponent } from '../../shared/ui/ai-coach-card/ai-coach-card.component';
import { TrainingDurationComponent } from './components/training-duration/training-duration.component';
import { CalendarComponent } from './components/calendar/calendar.component';
import { CoachService } from '../../core/services/coach.service';

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
  private coachService = inject(CoachService);
  
  // 1. Fetch the data as a signal.
  private statsData = toSignal(this.api.getWorkoutStatsWithAdvice());

  // 2. Compute the loading state based on data availability (error-free).
  loading = computed(() => !this.statsData());
  
  stats = computed(() => this.statsData());
  
  advice = toSignal(this.coachService.analyze().pipe(
    map(res => res.advice) 
  ));

  // 3. Mock data for the calendar.
  workoutDates = signal(['2026-03-01', '2026-03-05', '2026-03-26']);
}
