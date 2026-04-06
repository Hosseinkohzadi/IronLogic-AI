import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {environment} from '@env/environment';
import {AnalyzedWorkout, ExerciseHistoryPoint, UIExercise} from '@core/models';

@Injectable({providedIn: 'root'})
export class WorkoutImportService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  analyzeWorkoutText(text: string): Observable<AnalyzedWorkout> {
    // Fixed 400 error by sending as object
    return this.http.post<any>(`${this.apiUrl}/Workouts/import-text`, {workoutText: text})
      .pipe(
        map(response => {
          const rawData = response.value || response;
          return this.transformDataForUI(rawData);
        })
      );
  }

  getExerciseHistory(exerciseName: string): Observable<ExerciseHistoryPoint[]> {
    const encodedName = encodeURIComponent(exerciseName);
    return this.http.get<any>(`${this.apiUrl}/Exercises/${encodedName}/history`)
      .pipe(map(res => res.value || res));
  }

  private transformDataForUI(data: any): AnalyzedWorkout {
    const rawExercises = data.exercises || data.Exercises || [];
    let totalVolume = 0;

    const uiExercises: UIExercise[] = rawExercises.map((ex: any) => {
      const sets = ex.sets || ex.Sets || [];
      let maxWeight = 0;
      let topSetStr = '';

      sets.forEach((s: any) => {
        const w = s.weight || s.Weight || 0;
        const r = s.reps || s.Reps || 0;
        totalVolume += (w * r);
        if (w >= maxWeight) {
          maxWeight = w;
          topSetStr = `${w} lbs x ${r}${s.rpe ? ' @ ' + s.rpe : ''}`;
        }
      });

      return {
        name: ex.name || ex.Name,
        sets: sets.length,
        topSet: topSetStr || 'N/A',
        isPr: ex.prInsight?.isNewRecord || ex.PrInsight?.IsNewRecord || false,
        prMessage: ex.prInsight?.currentMaxWeight
          ? `New PR! 🏆 ${ex.prInsight.currentMaxWeight} lbs`
          : 'First Record 🚀'
      };
    });

    return {
      title: data.title || data.Title || 'New Session',
      date: new Date(data.date || data.Date).toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'short',
        day: 'numeric'
      }),
      totalVolume: `${totalVolume.toLocaleString()} lbs`,
      exercises: uiExercises,
      aiInsights: [
        `✅ ${uiExercises.length} movements detected.`,
        `🔥 Total volume: ${totalVolume.toLocaleString()} lbs`
      ]
    };
  }
}
