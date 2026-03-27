import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-import-workout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './import-workout.html',
})
export class ImportWorkoutComponent {
  rawLog: string = `Evening workout 🏋️
Thursday, Mar 26, 2026 at 12:00pm

Incline Bench Press (Smith Machine)
Set 1: 135 lbs x 12
Set 2: 185 lbs x 8
Set 3: 205 lbs x 5
Set 4: 245 lbs x 2 @ 9 rpe
Set 5: 185 lbs x 10 @ 8.5 rpe

Bench Press (Smith Machine)
Set 1: 185 lbs x 5
Set 2: 135 lbs x 10
Set 3: 135 lbs x 12

Lat Pulldown (Cable)
Set 1: 108 lbs x 12
Set 2: 122.5 lbs x 10
Set 3: 145 lbs x 8
Set 4: 153.5 lbs x 8 @ 9 rpe

Seated Row (Machine)
Set 1: 120 lbs x 12 @ 8.5 rpe
Set 2: 140 lbs x 10 @ 9 rpe
Set 3: 160 lbs x 8 @ 8.5 rpe
Set 4: 140 lbs x 10 @ 8.5 rpe`;

  isAnalyzing: boolean = false;
  analyzedWorkout: any = null;

  // Inject ChangeDetectorRef in the constructor
  constructor(private cdr: ChangeDetectorRef) {}

  analyzeData() {
    if (!this.rawLog.trim()) return;

    this.isAnalyzing = true;
    this.analyzedWorkout = null;

    // Use window.setTimeout to ensure the browser's function is called
    window.setTimeout(() => {
      this.analyzedWorkout = {
        title: "Evening workout 🏋️",
        date: "Thursday, Mar 26, 2026",
        totalVolume: "13,460 lbs",
        exercises: [
          { name: "Incline Bench Press (Smith)", sets: 5, topSet: "245 lbs x 2 @ 9 RPE" },
          { name: "Bench Press (Smith)", sets: 3, topSet: "185 lbs x 5" },
          { name: "Lat Pulldown (Cable)", sets: 4, topSet: "153.5 lbs x 8 @ 9 RPE" },
          { name: "Seated Row (Machine)", sets: 4, topSet: "160 lbs x 8 @ 8.5 RPE" }
        ],
        aiInsights: [
          "🔥 Incredible strength retention on Incline Bench (245 lbs @ 9 RPE).",
          "📊 High volume on upper body. Make sure to track recovery.",
          "💡 Perfect RPE management. Keeping it under 9.5 is ideal for a Classic Physique prep during a cut."
        ]
      };

      this.isAnalyzing = false;

      this.cdr.detectChanges();

    }, 1500);
  }
}
