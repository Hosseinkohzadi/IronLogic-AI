import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs';
import { environment } from '@env/environment';

@Component({
  selector: 'app-import-workout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './import-workout.html',
})
export class ImportWorkoutComponent {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  rawLog = signal<string>(`Evening workout 🏋️
Thursday, Mar 26, 2026 at 12:00pm

Incline Bench Press (Smith Machine)
Set 1: 135 lbs x 12
Set 2: 185 lbs x 8
Set 4: 245 lbs x 2 @ 9 rpe`);

  isAnalyzing = signal<boolean>(false);
  analyzedWorkout = signal<any>(null);
  errorMessage = signal<string | null>(null);

  analyzeData() {
    const text = this.rawLog().trim();
    if (!text) return;

    this.isAnalyzing.set(true);
    this.analyzedWorkout.set(null);
    this.errorMessage.set(null);

    this.http.post<any>(`${this.apiUrl}/Workouts/import-text`, { workoutText: text })
      .pipe(finalize(() => this.isAnalyzing.set(false)))
      .subscribe({
        next: (response) => {
          const formattedUI = this.transformDataForUI(response);
          this.analyzedWorkout.set(formattedUI);
        },
        error: (err) => {
          console.error('API Error:', err);
          this.errorMessage.set('Invalid text format or server connection failed.');
        }
      });
  }

  private transformDataForUI(response: any) {
    // 1. Intelligently find the data (whether it's direct or inside a 'value' field)
    const data = response.value || response;

    // 2. Handle case-insensitivity for the exercises array
    const exercises = data.exercises || data.Exercises || [];

    let totalVolume = 0;

    const uiExercises = exercises.map((ex: any) => {
      let maxWeight = 0;
      let topSetStr = '';

      // Handle the sets list
      const sets = ex.sets || ex.Sets || [];

      sets.forEach((set: any) => {
        // Safely extract numeric values
        const weight = set.weight || set.Weight || 0;
        const reps = set.reps || set.Reps || 0;
        const rpe = set.rpe || set.Rpe;

        totalVolume += (weight * reps);

        if (weight >= maxWeight) {
          maxWeight = weight;
          topSetStr = `${weight} lbs x ${reps}${rpe ? ' @ ' + rpe : ''}`;
        }
      });

      return {
        name: ex.name || ex.Name,
        sets: sets.length,
        topSet: topSetStr || 'N/A'
      };
    });

    // 3. Find the heaviest exercise with a safe check (to avoid 'undefined' error)
    const heaviestEx = uiExercises.length > 0
      ? [...uiExercises].sort((a: any, b: any) => {
        // Extract the weight number from the string (e.g., "245" from "245 lbs x 2")
        const weightA = parseFloat(a.topSet) || 0;
        const weightB = parseFloat(b.topSet) || 0;
        return weightB - weightA;
      })[0]?.name
      : 'Unknown';

    return {
      title: data.title || data.Title || 'New Workout',
      date: new Date(data.date || data.Date).toLocaleDateString('en-US', { weekday: 'long', month: 'short', day: 'numeric' }),
      totalVolume: `${totalVolume.toLocaleString()} lbs`,
      exercises: uiExercises,
      aiInsights: [
        `✅ ${uiExercises.length} exercises identified.`,
        `🔥 Your heaviest lift was: ${heaviestEx}`
      ]
    };
  }

  saveToDashboard() {
    console.log('Saving to IronLogic database...');
  }
}
