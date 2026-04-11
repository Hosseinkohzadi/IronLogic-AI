import { ChangeDetectionStrategy, Component, OnDestroy, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface ExerciseSet {
  set: number;
  prev: string;
  target: string;
  actualWeight: number;
  actualReps: number;
  done: boolean;
}

interface Exercise {
  id: number;
  name: string;
  sets: ExerciseSet[];
}

interface Routine {
  id: number;
  name: string;
  duration: string;
  description: string;
  exercisesCount: number;
  exercises: Exercise[];
}

interface ActiveRoutine {
  name: string;
  exercises: Exercise[];
}

@Component({
  selector: 'app-athlete-portal',
  imports: [CommonModule, FormsModule],
  templateUrl: './athlete-portal.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AthletePortalComponent implements OnDestroy {
  // ============= STATE MANAGEMENT =============
  readonly isWorkingOut = signal(false);
  private timerInterval: ReturnType<typeof setInterval> | null = null;
  readonly elapsedTime = signal(0);

  readonly todayDateDisplay = computed(() => {
    return new Date().toLocaleDateString('en-US', {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
    });
  });

  readonly activeRoutine = signal<ActiveRoutine>({
    name: 'No routine selected',
    exercises: [],
  });

  readonly routinePreview = computed(() =>
    this.activeRoutine().exercises.map((ex) => ({ name: ex.name, sets: ex.sets.length })),
  );

  readonly searchQuery = signal('');

  readonly availableRoutines = signal<Routine[]>([
    {
      id: 1,
      name: 'Leg Day Hypertrophy',
      duration: '50 mins',
      description: 'Quads, hamstrings, and glutes focus',
      exercisesCount: 4,
      exercises: [
        {
          id: 1,
          name: 'Barbell Squat',
          sets: [
            {
              set: 1,
              prev: '100 x 8',
              target: '105 x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
            {
              set: 2,
              prev: '105 x 8',
              target: '107.5 x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
            {
              set: 3,
              prev: '107.5 x 7',
              target: '110 x 7',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 2,
          name: 'Romanian Deadlift',
          sets: [
            {
              set: 1,
              prev: '80 x 10',
              target: '82.5 x 10',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
            {
              set: 2,
              prev: '82.5 x 9',
              target: '85 x 9',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 3,
          name: 'Leg Press',
          sets: [
            {
              set: 1,
              prev: '200 x 12',
              target: '220 x 12',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 4,
          name: 'Leg Curl Machine',
          sets: [
            {
              set: 1,
              prev: '60 x 15',
              target: '65 x 15',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
      ],
    },
    {
      id: 2,
      name: 'Chest & Arms',
      duration: '45 mins',
      description: 'Benching, pressing, and arm work',
      exercisesCount: 4,
      exercises: [
        {
          id: 1,
          name: 'Barbell Bench Press',
          sets: [
            {
              set: 1,
              prev: '80 x 8',
              target: '82.5 x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
            {
              set: 2,
              prev: '85 x 6',
              target: '87.5 x 6',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 2,
          name: 'Incline Dumbbell Fly',
          sets: [
            {
              set: 1,
              prev: '15kg x 12',
              target: '17.5kg x 10',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
            {
              set: 2,
              prev: '17kg x 10',
              target: '20kg x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 3,
          name: 'Barbell Curl',
          sets: [
            {
              set: 1,
              prev: '30 x 10',
              target: '32.5 x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 4,
          name: 'Tricep Rope Pushdown',
          sets: [
            {
              set: 1,
              prev: '40 x 12',
              target: '45 x 12',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
      ],
    },
    {
      id: 3,
      name: 'Back & Biceps',
      duration: '55 mins',
      description: 'Lat pulldown, rows, and curls',
      exercisesCount: 5,
      exercises: [
        {
          id: 1,
          name: 'Lat Pulldown',
          sets: [
            {
              set: 1,
              prev: '80 x 10',
              target: '85 x 10',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 2,
          name: 'Barbell Row',
          sets: [
            {
              set: 1,
              prev: '100 x 8',
              target: '105 x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 3,
          name: 'Dumbbell Row',
          sets: [
            {
              set: 1,
              prev: '30kg x 10',
              target: '32.5kg x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 4,
          name: 'Barbell Curl',
          sets: [
            {
              set: 1,
              prev: '30 x 10',
              target: '32.5 x 10',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 5,
          name: 'Machine Curl',
          sets: [
            {
              set: 1,
              prev: '50 x 12',
              target: '55 x 12',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
      ],
    },
    {
      id: 4,
      name: 'Shoulder Press',
      duration: '40 mins',
      description: 'Overhead pressing and lateral raises',
      exercisesCount: 4,
      exercises: [
        {
          id: 1,
          name: 'Barbell Shoulder Press',
          sets: [
            {
              set: 1,
              prev: '50 x 8',
              target: '52.5 x 8',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 2,
          name: 'Machine Shoulder Press',
          sets: [
            {
              set: 1,
              prev: '80 x 10',
              target: '85 x 10',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 3,
          name: 'Lateral Raise',
          sets: [
            {
              set: 1,
              prev: '12kg x 12',
              target: '15kg x 12',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
        {
          id: 4,
          name: 'Reverse Pec Deck',
          sets: [
            {
              set: 1,
              prev: '80 x 12',
              target: '90 x 12',
              actualWeight: 0,
              actualReps: 0,
              done: false,
            },
          ],
        },
      ],
    },
  ]);

  readonly selectedRoutine = signal<Routine | null>(null);

  readonly filteredRoutines = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    if (!query) {
      return this.availableRoutines();
    }
    return this.availableRoutines().filter((routine) => routine.name.toLowerCase().includes(query));
  });

  // ============= METHODS =============
  selectAndStartWorkout(routine: Routine): void {
    this.selectedRoutine.set(routine);
    this.activeRoutine.set({
      name: routine.name,
      exercises: routine.exercises.map((ex) => ({
        ...ex,
        sets: ex.sets.map((s) => ({ ...s })),
      })),
    });
    console.log('🏋️ Workout started:', routine.name);
    this.isWorkingOut.set(true);
    this.elapsedTime.set(0);
    this.startTimer();
  }

  startEmptyWorkout(): void {
    this.selectedRoutine.set(null);
    this.activeRoutine.set({
      name: 'Empty Workout',
      exercises: [],
    });
    console.log('🏋️ Empty workout started');
    this.isWorkingOut.set(true);
    this.elapsedTime.set(0);
    this.startTimer();
  }

  cancelWorkout(): void {
    this.stopTimer();
    this.isWorkingOut.set(false);
    this.selectedRoutine.set(null);
    this.activeRoutine.set({
      name: 'No routine selected',
      exercises: [],
    });
    this.elapsedTime.set(0);
    console.log('Workout cancelled');
  }

  finishWorkout(): void {
    this.stopTimer();
    this.isWorkingOut.set(false);
    console.log('Workout Saved');
  }

  updateSetWeight(exerciseId: number, setNumber: number, weight: number): void {
    this.activeRoutine.update((routine) => ({
      ...routine,
      exercises: routine.exercises.map((ex) =>
        ex.id === exerciseId
          ? {
              ...ex,
              sets: ex.sets.map((s) => (s.set === setNumber ? { ...s, actualWeight: weight } : s)),
            }
          : ex,
      ),
    }));
  }

  updateSetReps(exerciseId: number, setNumber: number, reps: number): void {
    this.activeRoutine.update((routine) => ({
      ...routine,
      exercises: routine.exercises.map((ex) =>
        ex.id === exerciseId
          ? {
              ...ex,
              sets: ex.sets.map((s) => (s.set === setNumber ? { ...s, actualReps: reps } : s)),
            }
          : ex,
      ),
    }));
  }

  toggleSetDone(exerciseId: number, setNumber: number): void {
    this.activeRoutine.update((routine) => ({
      ...routine,
      exercises: routine.exercises.map((ex) =>
        ex.id === exerciseId
          ? {
              ...ex,
              sets: ex.sets.map((s) => (s.set === setNumber ? { ...s, done: !s.done } : s)),
            }
          : ex,
      ),
    }));
  }

  // ============= TIMER LOGIC =============
  private startTimer(): void {
    this.timerInterval = setInterval(() => {
      this.elapsedTime.update((time) => time + 1);
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60)
      .toString()
      .padStart(2, '0');
    const secs = (seconds % 60).toString().padStart(2, '0');
    return `${mins}:${secs}`;
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }
}
