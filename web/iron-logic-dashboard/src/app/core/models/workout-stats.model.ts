export interface WorkoutSession { // نام کلاس را PascalCase کن (استاندارد)
  id: string;
  title: string;
  duration: string;
}

export interface DailyWorkout {
  date: string;
  // نام این فیلد باید دقیقاً مطابق جیسون بک‌اِند باشد
  workoutSessionDtos: WorkoutSession[];
}

export interface WorkoutStats {
  totalVolume: number;
  topExercise: string | null;
  intensityScore: number;
  sessionDate: string | Date | null;
  advice: { advice: string } | null; // دقیقاً مطابق ساختار جیسون
  dailyWorkouts: DailyWorkout[];
  streak: number;
  average?: number;
}
