import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { GridComponent } from '@shared/grid/grid';
import { ColumnConfig } from '@shared/grid/models/column-config';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { Exercise } from '@core/models/workout.model';

interface ExerciseGridRow {
  id: string;
  demoMediaUrl: string;
  name: string;
  primaryMuscle: string;
  equipment: string;
  mechanics: 'Compound' | 'Isolation';
  verified: 'Verified';
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  logicTag: 'CNS Intensive' | 'Hypertrophy Focus' | 'Metabolic Stress' | 'Posterior Chain';
  instructions: string;
  videoLink: string;
}

interface QuickAddForm {
  name: string;
  primaryMuscle: string;
  equipment: string;
  mechanics: 'Compound' | 'Isolation';
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  logicTag: 'CNS Intensive' | 'Hypertrophy Focus' | 'Metabolic Stress' | 'Posterior Chain';
  instructions: string;
  videoLink: string;
  demoMediaUrl: string;
}

@Component({
  selector: 'app-exercise-management',
  imports: [CommonModule, FormsModule, LucideAngularModule, GridComponent],
  templateUrl: './exercise-management.html',
  styleUrl: './exercise-management.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExerciseManagementComponent implements OnInit {
  private api = inject(IronLogicApiService);

  readonly isLoading = this.api.isLoading;
  readonly isQuickAddOpen = signal(false);
  readonly searchTerm = signal('');
  readonly muscleFilter = signal('');
  readonly allRows = signal<ExerciseGridRow[]>([]);

  readonly quickAddForm = signal<QuickAddForm>({
    name: '',
    primaryMuscle: 'Chest',
    equipment: 'Barbell',
    mechanics: 'Compound',
    difficulty: 'Intermediate',
    logicTag: 'Hypertrophy Focus',
    instructions: '',
    videoLink: '',
    demoMediaUrl: ''
  });

  readonly filteredRows = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const muscle = this.muscleFilter();
    return this.allRows().filter((row) => {
      const matchesTerm =
        !term ||
        row.name.toLowerCase().includes(term) ||
        row.primaryMuscle.toLowerCase().includes(term) ||
        row.equipment.toLowerCase().includes(term) ||
        row.mechanics.toLowerCase().includes(term) ||
        row.verified.toLowerCase().includes(term) ||
        row.logicTag.toLowerCase().includes(term) ||
        row.instructions.toLowerCase().includes(term);
      const matchesMuscle = !muscle || row.primaryMuscle === muscle;
      return matchesTerm && matchesMuscle;
    });
  });

  readonly exerciseColumns: ColumnConfig[] = [
    { field: 'demoMediaUrl', title: 'DEMO', type: 'image', width: '90px', locked: true },
    { field: 'name', title: 'NAME', type: 'text', width: '240px', sortable: true, locked: true, filterType: 'text' },
    {
      field: 'primaryMuscle',
      title: 'PRIMARY MUSCLE',
      type: 'text',
      width: '190px',
      sortable: true,
      filterType: 'select',
      filterOptions: [
        { label: 'Chest', value: 'Chest' },
        { label: 'Back', value: 'Back' },
        { label: 'Legs', value: 'Legs' },
        { label: 'Shoulders', value: 'Shoulders' },
        { label: 'Core', value: 'Core' },
      ]
    },
    {
      field: 'equipment',
      title: 'EQUIPMENT',
      type: 'text',
      width: '170px',
      sortable: true,
      filterType: 'select',
      filterOptions: [
        { label: 'Barbell', value: 'Barbell' },
        { label: 'Dumbbell', value: 'Dumbbell' },
        { label: 'Cable', value: 'Cable' },
        { label: 'Machine', value: 'Machine' },
        { label: 'Bodyweight', value: 'Bodyweight' },
      ]
    },
    {
      field: 'mechanics',
      title: 'MECHANICS',
      type: 'badge',
      width: '140px',
      sortable: true,
      filterType: 'select',
      filterOptions: [
        { label: 'Compound', value: 'Compound' },
        { label: 'Isolation', value: 'Isolation' },
      ],
      badgeStyle: 'mechanics'
    },
    {
      field: 'verified',
      title: 'VERIFIED',
      type: 'badge',
      width: '130px',
      sortable: true,
      filterType: 'select',
      filterOptions: [
        { label: 'Verified', value: 'Verified' },
      ],
      badgeStyle: 'verified'
    },
    {
      field: 'instructions',
      title: 'INSTRUCTIONS',
      type: 'text',
      width: '340px',
      sortable: true,
      filterType: 'text'
    },
    {
      field: 'difficulty',
      title: 'DIFFICULTY',
      type: 'badge',
      width: '150px',
      sortable: true,
      filterType: 'select',
      filterOptions: [
        { label: 'Beginner', value: 'Beginner' },
        { label: 'Intermediate', value: 'Intermediate' },
        { label: 'Advanced', value: 'Advanced' },
      ],
      badgeStyle: 'difficulty'
    },
    {
      field: 'logicTag',
      title: 'AI TAG',
      type: 'badge',
      width: '190px',
      sortable: true,
      filterType: 'select',
      filterOptions: [
        { label: 'CNS Intensive', value: 'CNS Intensive' },
        { label: 'Hypertrophy Focus', value: 'Hypertrophy Focus' },
        { label: 'Metabolic Stress', value: 'Metabolic Stress' },
        { label: 'Posterior Chain', value: 'Posterior Chain' },
      ],
      badgeStyle: 'aiTag'
    },
    {
      field: 'actions',
      title: 'ACTION',
      type: 'action',
      width: '92px',
      actionIcon: 'play-circle',
      actionType: 'play',
      actionLabel: 'Watch demo'
    }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    const mockRows = this.getMockExercises();
    this.allRows.set(mockRows);
  }

  onSearch(term: string): void {
    this.searchTerm.set(String(term ?? ''));
  }

  onMuscleFilter(muscle: string): void {
    this.muscleFilter.set(muscle);
  }

  openQuickAdd(): void {
    this.isQuickAddOpen.set(true);
  }

  closeQuickAdd(): void {
    this.isQuickAddOpen.set(false);
  }

  updateQuickAddField<K extends keyof QuickAddForm>(field: K, value: QuickAddForm[K]): void {
    this.quickAddForm.update((prev) => ({ ...prev, [field]: value }));
  }

  submitQuickAdd(): void {
    const form = this.quickAddForm();
    if (!form.name.trim()) {
      return;
    }

    const newRow: ExerciseGridRow = {
      id: `local-${Date.now()}`,
      demoMediaUrl: form.demoMediaUrl.trim(),
      name: form.name.trim(),
      primaryMuscle: form.primaryMuscle,
      equipment: form.equipment,
      mechanics: form.mechanics,
      verified: 'Verified',
      difficulty: form.difficulty,
      logicTag: form.logicTag,
      instructions: form.instructions.trim(),
      videoLink: form.videoLink.trim()
    };

    this.allRows.update((rows) => [newRow, ...rows]);
    this.quickAddForm.set({
      name: '',
      primaryMuscle: 'Chest',
      equipment: 'Barbell',
      mechanics: 'Compound',
      difficulty: 'Intermediate',
      logicTag: 'Hypertrophy Focus',
      instructions: '',
      videoLink: '',
      demoMediaUrl: ''
    });
    this.closeQuickAdd();
  }

  handleGridAction(event: { type: string; row: ExerciseGridRow }): void {
    if (event.type === 'play') {
      const videoUrl = event.row.videoLink?.trim();
      if (videoUrl) {
        window.open(videoUrl, '_blank', 'noopener,noreferrer');
      }
      return;
    }

    if (event.type !== 'delete') {
      return;
    }

    const rowId = event.row.id;
    if (rowId.startsWith('local-')) {
      this.allRows.update((rows) => rows.filter((row) => row.id !== rowId));
      return;
    }

    this.api.deleteExercise(rowId).subscribe({
      next: () => {
        this.allRows.update((rows) => rows.filter((row) => row.id !== rowId));
      }
    });
  }

  private mapToRows(exercises: Exercise[]): ExerciseGridRow[] {
    return exercises.map((exercise) => {
      const muscle = this.resolveMuscle(exercise.primaryMuscleId);
      const equipment = this.resolveEquipment(exercise.equipmentId);
      const difficulty = this.resolveDifficulty(exercise.mechanics);

      return {
        id: exercise.id,
        demoMediaUrl: exercise.imagePath ?? '',
        name: exercise.name,
        primaryMuscle: muscle,
        equipment,
        mechanics: this.resolveMechanics(exercise.mechanics),
        verified: 'Verified',
        difficulty,
        logicTag: this.resolveAiTag(exercise.mechanics),
        instructions: exercise.instructions ?? 'Focus on controlled tempo and full range of motion for quality reps.',
        videoLink: exercise.url ?? ''
      };
    });
  }

  private getMockExercises(): ExerciseGridRow[] {
    return [
      {
        id: 'mock-1',
        demoMediaUrl: 'https://images.unsplash.com/photo-1534258936925-c58bed479fcb?auto=format&fit=crop&w=320&q=80',
        name: 'Barbell Back Squat',
        primaryMuscle: 'Legs',
        equipment: 'Barbell',
        mechanics: 'Compound',
        verified: 'Verified',
        difficulty: 'Advanced',
        logicTag: 'CNS Intensive',
        instructions: 'Brace your core, keep a neutral spine, and drive through mid-foot while maintaining knee tracking over toes.',
        videoLink: 'https://www.youtube.com/watch?v=ultWZbUMPL8'
      },
      {
        id: 'mock-2',
        demoMediaUrl: 'https://images.unsplash.com/photo-1534367610401-9f5ed68180aa?auto=format&fit=crop&w=320&q=80',
        name: 'Incline DB Press',
        primaryMuscle: 'Chest',
        equipment: 'Dumbbell',
        mechanics: 'Compound',
        verified: 'Verified',
        difficulty: 'Intermediate',
        logicTag: 'Hypertrophy Focus',
        instructions: 'Set the bench at 30-45 degrees, control the eccentric, and press up while keeping shoulder blades retracted.',
        videoLink: 'https://www.youtube.com/watch?v=8iPEnn-ltC8'
      },
      {
        id: 'mock-3',
        demoMediaUrl: 'https://images.unsplash.com/photo-1517836357463-d25dfeac3438?auto=format&fit=crop&w=320&q=80',
        name: 'Conventional Deadlift',
        primaryMuscle: 'Back',
        equipment: 'Barbell',
        mechanics: 'Compound',
        verified: 'Verified',
        difficulty: 'Advanced',
        logicTag: 'Posterior Chain',
        instructions: 'Pull slack out of the bar, hinge from the hips, and lock out with glutes without overextending your lower back.',
        videoLink: 'https://www.youtube.com/watch?v=op9kVnSso6Q'
      },
      {
        id: 'mock-4',
        demoMediaUrl: 'https://images.unsplash.com/photo-1599058917212-d750089bc07e?auto=format&fit=crop&w=320&q=80',
        name: 'Pull-Ups',
        primaryMuscle: 'Back',
        equipment: 'Bodyweight',
        mechanics: 'Compound',
        verified: 'Verified',
        difficulty: 'Intermediate',
        logicTag: 'Hypertrophy Focus',
        instructions: 'Initiate each rep by depressing the scapula, pull chest toward the bar, and avoid swinging through the torso.',
        videoLink: 'https://www.youtube.com/watch?v=eGo4IYlbE5g'
      },
      {
        id: 'mock-5',
        demoMediaUrl: 'https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?auto=format&fit=crop&w=320&q=80',
        name: 'Lateral Raises',
        primaryMuscle: 'Shoulders',
        equipment: 'Dumbbell',
        mechanics: 'Isolation',
        verified: 'Verified',
        difficulty: 'Beginner',
        logicTag: 'Metabolic Stress',
        instructions: 'Raise the dumbbells to shoulder height with slight elbow bend and pause briefly to limit momentum.',
        videoLink: 'https://www.youtube.com/watch?v=3VcKaXpzqRo'
      },
      {
        id: 'mock-6',
        demoMediaUrl: 'https://images.unsplash.com/photo-1434682881908-b43d0467b798?auto=format&fit=crop&w=320&q=80',
        name: 'Bulgarian Split Squat',
        primaryMuscle: 'Glutes',
        equipment: 'Dumbbell',
        mechanics: 'Compound',
        verified: 'Verified',
        difficulty: 'Intermediate',
        logicTag: 'Posterior Chain',
        instructions: 'Keep your front heel planted, drop straight down under control, and drive up through the front leg.',
        videoLink: 'https://www.youtube.com/watch?v=2C-uNgKwPLE'
      },
      {
        id: 'mock-7',
        demoMediaUrl: 'https://images.unsplash.com/photo-1518644961665-ed172691aaa1?auto=format&fit=crop&w=320&q=80',
        name: 'Face Pulls',
        primaryMuscle: 'Shoulders',
        equipment: 'Cable',
        mechanics: 'Isolation',
        verified: 'Verified',
        difficulty: 'Beginner',
        logicTag: 'Hypertrophy Focus',
        instructions: 'Pull rope handles toward the bridge of your nose, externally rotate at end range, and keep ribcage down.',
        videoLink: 'https://www.youtube.com/watch?v=rep-qVOkqgk'
      },
      {
        id: 'mock-8',
        demoMediaUrl: 'https://images.unsplash.com/photo-1517964109683-1b2b0d2f7f2a?auto=format&fit=crop&w=320&q=80',
        name: 'Skull Crushers',
        primaryMuscle: 'Triceps',
        equipment: 'Barbell',
        mechanics: 'Isolation',
        verified: 'Verified',
        difficulty: 'Intermediate',
        logicTag: 'Metabolic Stress',
        instructions: 'Keep elbows fixed, lower the bar toward the forehead with control, and extend without flaring the shoulders.',
        videoLink: 'https://www.youtube.com/watch?v=d_KZxkY_0cM'
      },
      {
        id: 'mock-9',
        demoMediaUrl: 'https://images.unsplash.com/photo-1556817411-31ae72fa3ea0?auto=format&fit=crop&w=320&q=80',
        name: 'Seated Cable Row',
        primaryMuscle: 'Back',
        equipment: 'Cable',
        mechanics: 'Compound',
        verified: 'Verified',
        difficulty: 'Beginner',
        logicTag: 'Posterior Chain',
        instructions: 'Lead with elbows, keep chest tall, and finish each rep by squeezing shoulder blades without excessive lean back.',
        videoLink: 'https://www.youtube.com/watch?v=GZbfZ033f74'
      },
      {
        id: 'mock-10',
        demoMediaUrl: 'https://images.unsplash.com/photo-1517344884509-a0c97ec11bcc?auto=format&fit=crop&w=320&q=80',
        name: 'Hammer Curls',
        primaryMuscle: 'Biceps',
        equipment: 'Dumbbell',
        mechanics: 'Isolation',
        verified: 'Verified',
        difficulty: 'Beginner',
        logicTag: 'Hypertrophy Focus',
        instructions: 'Maintain a neutral grip, keep elbows close to your torso, and lower slowly to maximize biceps and brachialis tension.',
        videoLink: 'https://www.youtube.com/watch?v=zC3nLlEvin4'
      }
    ];
  }

  private resolveMuscle(muscleId?: string): string {
    const muscles = ['Chest', 'Back', 'Legs', 'Shoulders', 'Biceps', 'Triceps', 'Core', 'Glutes'];
    if (!muscleId) {
      return muscles[0];
    }

    return muscles[this.hashText(muscleId) % muscles.length];
  }

  private resolveEquipment(equipmentId?: string): string {
    const equipment = ['Barbell', 'Dumbbell', 'Cable', 'Machine', 'Bodyweight', 'Kettlebell'];
    if (!equipmentId) {
      return equipment[0];
    }

    return equipment[this.hashText(equipmentId) % equipment.length];
  }

  private resolveMechanics(mechanics?: string): 'Compound' | 'Isolation' {
    const normalized = (mechanics ?? '').toLowerCase();
    if (normalized.includes('isolation')) {
      return 'Isolation';
    }
    return 'Compound';
  }

  private resolveDifficulty(mechanics?: string): 'Beginner' | 'Intermediate' | 'Advanced' {
    const normalized = (mechanics ?? '').toLowerCase();
    if (normalized.includes('isolation')) {
      return 'Beginner';
    }

    if (normalized.includes('compound')) {
      return 'Advanced';
    }

    return 'Intermediate';
  }

  private resolveAiTag(mechanics?: string): 'CNS Intensive' | 'Hypertrophy Focus' | 'Metabolic Stress' | 'Posterior Chain' {
    const normalized = (mechanics ?? '').toLowerCase();
    if (normalized.includes('isolation')) {
      return 'Metabolic Stress';
    }

    if (normalized.includes('posterior')) {
      return 'Posterior Chain';
    }

    return 'CNS Intensive';
  }

  private hashText(value: string): number {
    let hash = 0;
    for (const char of value) {
      hash = (hash * 31 + char.charCodeAt(0)) >>> 0;
    }

    return hash;
  }
}
