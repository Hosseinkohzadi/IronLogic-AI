import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AnalyzedWorkout, UIExercise, ExerciseHistoryPoint } from '@core/models';
import { WorkoutImportService } from '@core/services';
import { Chart } from 'chart.js/auto';

@Component({
  selector: 'app-import-workout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './import-workout.html',
  styleUrl: './import-workout.css'
})
export class ImportWorkoutComponent {
  private workoutService = inject(WorkoutImportService);
  chartInstances: { [key: string]: Chart } = {};

  rawLog = signal<string>(`Evening workout 🏋️\nThursday, Mar 26, 2026 at 12:00pm\n\nIncline Bench Press (Smith Machine)\nSet 1: 135 lbs x 12\nSet 2: 185 lbs x 8\nSet 4: 245 lbs x 2 @ 9 rpe`);

  isAnalyzing = signal(false);
  analyzedWorkout = signal<AnalyzedWorkout | null>(null);
  errorMessage = signal<string | null>(null);

  // تولید ID معتبر برای Canvas
  getChartId(exerciseName: string): string {
    return 'chart-' + exerciseName.replace(/[^a-zA-Z0-9]/g, '-');
  }

  analyzeData() {
    const text = this.rawLog()?.trim();
    if (!text) return;

    this.isAnalyzing.set(true);
    this.errorMessage.set(null);

    this.workoutService.analyzeWorkoutText(text)
      .pipe(finalize(() => this.isAnalyzing.set(false)))
      .subscribe({
        next: (data) => this.analyzedWorkout.set(data),
        error: () => this.errorMessage.set('خطا در آنالیز متن یا عدم برقراری ارتباط با سرور.')
      });
  }

  toggleDetails(ex: UIExercise) {
    ex.isExpanded = !ex.isExpanded;
    this.refreshSignal(); // بروزرسانی آنی UI برای باز شدن آکاردئون

    if (ex.isExpanded && !ex.history) {
      ex.isLoadingHistory = true;
      this.refreshSignal(); // نمایش وضعیت لودینگ

      this.workoutService.getExerciseHistory(ex.name).subscribe({
        next: (data: ExerciseHistoryPoint[]) => {
          ex.history = data;
          ex.isLoadingHistory = false;
          this.refreshSignal(); // حذف لودینگ و آماده‌سازی نمودار

          setTimeout(() => this.renderChart(ex), 50);
        },
        error: () => {
          ex.isLoadingHistory = false;
          this.refreshSignal();
        }
      });
    } else if (ex.isExpanded && ex.history) {
      setTimeout(() => this.renderChart(ex), 50);
    }
  }

  renderChart(ex: UIExercise) {
    const canvasId = this.getChartId(ex.name);
    const ctx = document.getElementById(canvasId) as HTMLCanvasElement;
    if (!ctx || !ex.history) return;

    if (this.chartInstances[ex.name]) this.chartInstances[ex.name].destroy();

    this.chartInstances[ex.name] = new Chart(ctx, {
      type: 'line',
      data: {
        labels: ex.history.map(h => new Date(h.date).toLocaleDateString(undefined, {month:'short', day:'numeric'})),
        datasets: [{
          data: ex.history.map(h => h.maxWeight),
          borderColor: '#818cf8',
          backgroundColor: 'rgba(129, 140, 248, 0.15)',
          fill: true,
          tension: 0.4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (ctx) => {
                const p = ex.history![ctx.dataIndex];
                return [
                  ` Best: ${p.topSetSummary}`,
                  ` Volume: ${p.totalVolume.toLocaleString()}`,
                  ` Est. 1RM: ${p.estimated1RM}`
                ];
              }
            }
          }
        },
        scales: {
          y: { grid: { color: 'rgba(255, 255, 255, 0.05)' }, ticks: { color: '#94a3b8' } },
          x: { grid: { display: false }, ticks: { color: '#94a3b8' } }
        }
      }
    });
  }

  // متد کلیدی برای حل مشکل لودینگ با Signal
  private refreshSignal() {
    this.analyzedWorkout.update(value => value ? { ...value } : null);
  }

  saveToDashboard() {
    console.log('در حال ذخیره در دیتابیس...');
  }
}
