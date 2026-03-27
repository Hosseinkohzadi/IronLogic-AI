import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { IronLogicApiService } from '../../core/services/iron-logic-api.service';
import { MetricCardComponent } from '../../shared/ui/metric-card/metric-card.component';
import { AiCoachCardComponent } from '../../shared/ui/ai-coach-card/ai-coach-card.component';
import { TrainingDurationComponent } from './components/training-duration/training-duration.component';
import { CalendarComponent } from './components/calendar/calendar.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MetricCardComponent, AiCoachCardComponent, TrainingDurationComponent, CalendarComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  private api = inject(IronLogicApiService);

  loading = signal(true);
  stats = toSignal(this.api.getWorkoutStatsWithAdvice().pipe(map(s => {
    this.loading.set(false);
    return s;
  })));
  advice = toSignal(this.api.getWorkoutStatsWithAdvice().pipe(map(s => s.advice)));

  // Mocked workout dates for demonstration. This would come from your API.
  workoutDates = computed(() => {
    const s = this.stats();
    return s ? [s.sessionDate] : [];
  });
}
