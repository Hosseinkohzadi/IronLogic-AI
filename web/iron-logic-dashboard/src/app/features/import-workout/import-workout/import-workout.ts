import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AnalyzedWorkout } from '@core/models';
import { WorkoutImportService } from '@core/services/workout-import.service';

@Component({
  selector: 'app-import-workout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './import-workout.html',
  styleUrl: './import-workout.css'
})
export class ImportWorkoutComponent {
  private workoutImportService = inject(WorkoutImportService);

  rawLog = signal<string>(`Evening workout 🏋️\nThursday, Mar 26, 2026 at 12:00pm\n\nIncline Bench Press (Smith Machine)\nSet 1: 135 lbs x 12\nSet 2: 185 lbs x 8\nSet 4: 245 lbs x 2 @ 9 rpe`);

  isAnalyzing = signal<boolean>(false);
  analyzedWorkout = signal<AnalyzedWorkout | null>(null);
  errorMessage = signal<string | null>(null);

  analyzeData() {
    const text = this.rawLog().trim();
    if (!text) return;

    this.isAnalyzing.set(true);
    this.analyzedWorkout.set(null);
    this.errorMessage.set(null);

    // Call the service, which now returns the cleaned data.
    this.workoutImportService.analyzeWorkoutText(text)
      .pipe(finalize(() => this.isAnalyzing.set(false)))
      .subscribe({
        next: (formattedUI) => {
          this.analyzedWorkout.set(formattedUI);
        },
        error: (err) => {
          console.error('API Error:', err);
          this.errorMessage.set('Invalid text format or connection to the server has been lost.');
        }
      });
  }

  saveToDashboard() {
    console.log('Saving to database...');
  }
}
