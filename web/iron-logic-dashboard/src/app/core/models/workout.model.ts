export interface ExerciseHistoryPoint {
  date: string;
  maxWeight: number;
  totalVolume: number;
  topSetSummary: string;
  estimated1RM: number;
}

export interface UIExercise {
  name: string;
  sets: number;
  topSet: string;
  isPr?: boolean;
  prMessage?: string | null;
  isExpanded?: boolean;
  isLoadingHistory?: boolean;
  history?: ExerciseHistoryPoint[];
}

export interface AnalyzedWorkout {
  title: string;
  date: string;
  totalVolume: string;
  exercises: UIExercise[];
  aiInsights: string[];
}

export interface Exercise {
  id: string; // تغییر از number به string برای هماهنگی با GUID
  name: string;
  primaryMuscleId?: string; // این هم در Swagger به صورت Guid است
  equipmentId?: string;    // این هم در Swagger به صورت Guid است
  mechanics?: string;
  instructions?: string;
  url?: string;
  imagePath?: string | null;
}
