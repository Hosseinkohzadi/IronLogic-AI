export interface UIExercise {
  name: string;
  sets: number;
  topSet: string;
  isPr?: boolean;
  prMessage?: string | null;
}

export interface AnalyzedWorkout {
  title: string;
  date: string;
  totalVolume: string;
  exercises: UIExercise[];
  aiInsights: string[];
}
