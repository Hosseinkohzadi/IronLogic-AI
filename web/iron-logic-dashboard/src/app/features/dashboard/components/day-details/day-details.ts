import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-day-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './day-details.html',
  styleUrl: './day-details.css'
})
export class DayDetailsComponent {
  @Input({ required: true }) sessions: any[] = [];
  @Input() date: Date | null = null;
  @Input() isPinned = false;

  @Output() close = new EventEmitter<void>();
  @Output() togglePin = new EventEmitter<void>();

  // Manage open/closed state of exercise lists
  expandedExercises = signal<Set<string>>(new Set());

  toggleExercise(exerciseName: string) {
    const current = new Set(this.expandedExercises());
    if (current.has(exerciseName)) {
      current.delete(exerciseName);
    } else {
      current.add(exerciseName);
    }
    this.expandedExercises.set(current);
  }

  isExpanded(exerciseName: string): boolean {
    return this.expandedExercises().has(exerciseName);
  }

  getBestSet(sets: any[]): string {
    if (!sets || sets.length === 0) return 'N/A';
    const best = sets.reduce((prev, current) => (prev.weight > current.weight) ? prev : current);
    return `${best.weight} lbs x ${best.reps}${best.rpe ? ` @ ${best.rpe}` : ''}`;
  }

  // Dynamic color coding based on intensity
  getRpeColorClass(rpe: number | null): string {
    if (!rpe) return 'bg-slate-100 text-slate-400 border-slate-200';
    if (rpe >= 9) return 'bg-rose-100 text-rose-700 border-rose-200';
    if (rpe >= 7) return 'bg-amber-100 text-amber-700 border-amber-200';
    return 'bg-emerald-100 text-emerald-700 border-emerald-200';
  }

  // Educational descriptions for Tooltip
  getRpeDescription(rpe: number | null): string {
    if (!rpe) return '';
    const descriptions: { [key: number]: string } = {
      10: 'Maximum effort: No more reps possible.',
      9.5: 'No more reps possible, but maybe slightly more weight.',
      9: 'Only 1 more rep left.',
      8.5: 'Definitely 1 more rep, maybe 2 reps possible.',
      8: '2 more reps left.',
      7.5: 'Definitely 2 more reps, maybe 3 reps possible.',
      7: '3 more reps left.',
      6: 'Light effort: 4 or more reps left.'
    };
    return descriptions[rpe] || `Rate of Perceived Exertion: ${rpe}`;
  }
}
