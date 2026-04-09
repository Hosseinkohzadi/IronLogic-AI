export interface MuscleRow {
  id: string;
  name: string;
  scientificName: string;
  imageUrl: string;
  region: string;
  linkedExercises: number;
  status: 'Active' | 'Inactive' | 'Beta';
  isSelected?: boolean;
}
