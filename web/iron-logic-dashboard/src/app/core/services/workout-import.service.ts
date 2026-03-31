import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '@env/environment';
import { AnalyzedWorkout, UIExercise } from '@core/models';

@Injectable({
  providedIn: 'root'
})
export class WorkoutImportService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  // Main method called by the component
  analyzeWorkoutText(text: string): Observable<AnalyzedWorkout> {
    return this.http.post<any>(`${this.apiUrl}/Workouts/import-text`, { workoutText: text })
      .pipe(
        // Use RxJS Map to transform raw backend data into clean UI data before it reaches the component
        map(response => this.transformDataForUI(response))
      );
  }

  // All calculation logic has been moved here
  private transformDataForUI(response: any): AnalyzedWorkout {
    const data = response.value || response;
    const exercises = data.exercises || data.Exercises || [];
    let totalVolume = 0;

    const uiExercises: UIExercise[] = exercises.map((ex: any) => {
      let maxWeight = 0;
      let topSetStr = '';
      const sets = ex.sets || ex.Sets || [];

      sets.forEach((set: any) => {
        const weight = set.weight || set.Weight || 0;
        const reps = set.reps || set.Reps || 0;
        const rpe = set.rpe || set.Rpe;

        totalVolume += (weight * reps);

        if (weight >= maxWeight) {
          maxWeight = weight;
          topSetStr = `${weight} lbs x ${reps}${rpe ? ' @ ' + rpe : ''}`;
        }
      });

      let prText = null;
      let isPr = false;
      const prInsight = ex.prInsight || ex.PrInsight;

      if (prInsight?.isNewRecord || prInsight?.IsNewRecord) {
        isPr = true;
        const currentMax = prInsight.currentMaxWeight || prInsight.CurrentMaxWeight;
        const prevMax = prInsight.previousMaxWeight || prInsight.PreviousMaxWeight;
        const prevDateRaw = prInsight.previousDate || prInsight.PreviousDate;

        const diff = prevMax ? `(+${currentMax - prevMax} lbs)` : '(First Record)';
        const prevDate = prevDateRaw ? new Date(prevDateRaw).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) : '';

        prText = `New PR! 🏆 ${currentMax} lbs ${diff} ${prevDate ? 'vs ' + prevDate : ''}`;
      }

      return {
        name: ex.name || ex.Name,
        sets: sets.length,
        topSet: topSetStr || 'N/A',
        isPr: isPr,
        prMessage: prText
      };
    });

    const heaviestEx = uiExercises.length > 0
      ? [...uiExercises].sort((a, b) => {
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
        `✅ ${uiExercises.length} exercises were identified.`,
        `🔥 Your heaviest lift: ${heaviestEx}`
      ]
    };
  }
}
