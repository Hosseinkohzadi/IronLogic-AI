import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { GridComponent } from '@shared/grid/grid';
import { ColumnConfig } from '@shared/grid/models/column-config';
import { MuscleRow } from '@core/models/muscle.model';

@Component({
  selector: 'app-muscle-management',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule, GridComponent],
  templateUrl: './muscle-management.component.html',
  styleUrl: './muscle-management.component.css'
})
export class MuscleManagementComponent implements OnInit {
  muscles = signal<MuscleRow[]>([]);
  searchTerm = signal('');
  isLoading = signal(false);
  isDrawerOpen = signal(false);
  selectedMuscleId = signal<string | null>(null);
  isAddModalOpen = signal(false);
  newMuscleName = signal('');
  newMuscleScientific = signal('');
  newMuscleRegion = signal('');

  filteredMuscles = computed(() => {
    const searchLower = this.searchTerm().toLowerCase();
    return this.muscles().filter(muscle =>
      muscle.name.toLowerCase().includes(searchLower) ||
      muscle.scientificName.toLowerCase().includes(searchLower) ||
      muscle.region.toLowerCase().includes(searchLower)
    );
  });

  selectedMuscleDetails = computed(() => {
    const id = this.selectedMuscleId();
    return this.muscles().find(m => m.id === id);
  });

  columns: ColumnConfig[] = [
    { field: 'isSelected', title: '', type: 'selection', width: '50px' },
    { field: 'imageUrl', title: 'ANATOMY', type: 'image', width: '80px' },
    { field: 'name', title: 'MUSCLE', type: 'profile', width: '220px', subfield: 'scientificName' },
    { field: 'region', title: 'REGION', type: 'text', width: '150px' },
    { field: 'linkedExercises', title: 'EXERCISES', type: 'text', width: '120px' },
    { field: 'status', title: 'STATUS', type: 'badge', width: '120px' },
    { field: 'id', title: 'ACTION', type: 'action', width: '80px' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    const baseMuscles: MuscleRow[] = [
      {
        id: 'msl-001',
        name: 'Chest',
        scientificName: 'Pectoralis Major',
        imageUrl: 'https://images.unsplash.com/photo-1534368541638-5d16dc5fb264?w=400&h=400&fit=crop',
        region: 'Upper Body',
        linkedExercises: 24,
        status: 'Active'
      },
      {
        id: 'msl-002',
        name: 'Back',
        scientificName: 'Latissimus Dorsi',
        imageUrl: 'https://images.unsplash.com/photo-1517836357463-d25ddfcbf042?w=400&h=400&fit=crop',
        region: 'Upper Body',
        linkedExercises: 28,
        status: 'Active'
      },
      {
        id: 'msl-003',
        name: 'Quadriceps',
        scientificName: 'Rectus Femoris',
        imageUrl: 'https://images.unsplash.com/photo-1533241749264-ba03214ba159?w=400&h=400&fit=crop',
        region: 'Lower Body',
        linkedExercises: 18,
        status: 'Active'
      },
      {
        id: 'msl-004',
        name: 'Hamstrings',
        scientificName: 'Biceps Femoris',
        imageUrl: 'https://images.unsplash.com/photo-1434682881908-b91d3cb6b3b5?w=400&h=400&fit=crop',
        region: 'Lower Body',
        linkedExercises: 16,
        status: 'Active'
      },
      {
        id: 'msl-005',
        name: 'Shoulders',
        scientificName: 'Deltoid',
        imageUrl: 'https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=400&h=400&fit=crop',
        region: 'Upper Body',
        linkedExercises: 32,
        status: 'Active'
      },
      {
        id: 'msl-006',
        name: 'Core',
        scientificName: 'Rectus Abdominis',
        imageUrl: 'https://images.unsplash.com/photo-1576091160550-112173f7f869?w=400&h=400&fit=crop',
        region: 'Core',
        linkedExercises: 22,
        status: 'Active'
      },
      {
        id: 'msl-007',
        name: 'Biceps',
        scientificName: 'Biceps Brachii',
        imageUrl: 'https://images.unsplash.com/photo-1580127574784-86c17fb76ea2?w=400&h=400&fit=crop',
        region: 'Upper Body',
        linkedExercises: 14,
        status: 'Beta'
      },
      {
        id: 'msl-008',
        name: 'Triceps',
        scientificName: 'Triceps Brachii',
        imageUrl: 'https://images.unsplash.com/photo-1599926752551-40f085e4b64b?w=400&h=400&fit=crop',
        region: 'Upper Body',
        linkedExercises: 12,
        status: 'Active'
      }
    ];

    this.muscles.set(baseMuscles);
    this.isLoading.set(false);
  }

  onSearch(event: string): void {
    this.searchTerm.set(event);
  }

  handleGridAction(action: { type: string; row: MuscleRow }): void {
    switch (action.type) {
      case 'view':
        this.selectedMuscleId.set(action.row.id);
        this.isDrawerOpen.set(true);
        console.log('View muscle:', action.row.id);
        break;
      case 'edit':
        this.handleEdit(action.row.id);
        break;
      case 'delete':
        this.handleDelete(action.row.id);
        break;
    }
  }

  openAddModal(): void {
    this.newMuscleName.set('');
    this.newMuscleScientific.set('');
    this.newMuscleRegion.set('Chest');
    this.isAddModalOpen.set(true);
  }

  closeAddModal(): void {
    this.isAddModalOpen.set(false);
    this.newMuscleName.set('');
    this.newMuscleScientific.set('');
    this.newMuscleRegion.set('');
  }

  saveNewMuscle(): void {
    if (!this.newMuscleName().trim() || !this.newMuscleScientific().trim()) {
      console.warn('Muscle name and scientific name are required');
      return;
    }

    const newMuscle: MuscleRow = {
      id: `msl-${Date.now()}`,
      name: this.newMuscleName(),
      scientificName: this.newMuscleScientific(),
      imageUrl: 'https://images.unsplash.com/photo-1534368541638-5d16dc5fb264?w=400&h=400&fit=crop',
      region: this.newMuscleRegion(),
      linkedExercises: 0,
      status: 'Active'
    };

    this.muscles.update(current => [...current, newMuscle]);
    this.closeAddModal();
    console.log('Saved new muscle:', newMuscle);
  }

  closeDrawer(): void {
    this.isDrawerOpen.set(false);
    this.selectedMuscleId.set(null);
  }

  handleEdit(id: string): void {
    console.log('Edit muscle:', id);
  }

  handleMerge(id: string): void {
    console.log('Merge muscle:', id);
  }

  handleDelete(id: string): void {
    if (confirm('Are you sure you want to delete this muscle?')) {
      this.muscles.update(current => current.filter(m => m.id !== id));
      this.closeDrawer();
      console.log('Deleted muscle:', id);
    }
  }

  navigateLinkedEntity(type: string): void {
    console.log('Navigate to:', type, 'for muscle:', this.selectedMuscleDetails()?.id);
  }
}
