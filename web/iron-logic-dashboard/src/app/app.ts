import { isPlatformBrowser} from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { WorkoutService } from './core/services/workout.service';
import { CoachService } from './core/services/coach.service';
import { WorkoutStatsResponse } from './core/models/workout-stats-response.model';
import { CoachAdviceResponse } from './core/models/coach-advice-response.model';

import { Component, OnInit, PLATFORM_ID, inject, signal, ViewEncapsulation } from '@angular/core';
import { SidebarComponent } from './layout/sidebar/sidebar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent],
  templateUrl: './app.html',
  encapsulation: ViewEncapsulation.None,
})
export class App implements OnInit {
  protected readonly title = signal('iron-logic-dashboard');
  protected readonly stats = signal<WorkoutStatsResponse | null>(null);
  protected readonly advice = signal<CoachAdviceResponse | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  private readonly platformId = inject(PLATFORM_ID);
  private readonly workoutService = inject(WorkoutService);
  private readonly coachService = inject(CoachService);

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.workoutService.getStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.loading.set(false);
        console.log('Workout Stats Loaded:', data);
      },
      error: (err) => {
        this.error.set('Failed to load workout stats');
        this.loading.set(false);
        console.error('CORS or Connection Error:', err);
      },
    });

    this.coachService.analyze().subscribe({
      next: (data) => {
        this.advice.set(data);
        console.log('Coach Advice Loaded:', data);
      },
      error: (err) => {
        console.error('Coach Service Error:', err);
      },
    });
  }
}
