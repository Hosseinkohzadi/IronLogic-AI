export interface WorkoutStats {
  totalVolume: number;
  topExercise: string | null;
  intensityScore: number;
  sessionDate: string | Date | null;

  // این خط تغییر کرد تا با آبجکتِ دات‌نت همخوانی داشته باشه
  advice: { advice: string } | string | null | any;

  workoutDates?: string[] | Date[];
  average?: number;
  minimum?: number;
  maximum?: number;
}
