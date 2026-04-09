import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';

import { GridComponent } from '@shared/grid/grid';
import { ColumnConfig } from '@shared/grid/models/column-config';

type EquipmentStatus = 'Active' | 'Review' | 'Suspended';

type EquipmentEntityType = 'sessions' | 'weights' | 'exerciseSessions';

interface EquipmentRecord {
  id: string;
  imageUrl: string;
  name: string;
  linkedExercises: number;
  status: EquipmentStatus;
  description: string;
  aliasConflicts: number;
  lastReview: string;
  health: 'Healthy' | 'Attention';
  isSelected?: boolean;
}

@Component({
  selector: 'app-equipment-management',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, GridComponent],
  templateUrl: './equipment-management.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EquipmentManagementComponent {
  equipments = signal<EquipmentRecord[]>([]);
  filteredEquipments = signal<EquipmentRecord[]>([]);
  searchTerm = signal('');
  isLoading = signal(false);
  isAddModalOpen = signal(false);
  isEditModalOpen = signal(false);
  editingEquipment = signal<EquipmentRecord | null>(null);
  isDrawerOpen = signal(false);
  selectedEquipmentId = signal<string | null>(null);
  newEquipmentName = signal('');
  newEquipmentAlias = signal('');

  columns: ColumnConfig[] = [
    { field: 'selection', title: '', type: 'selection', width: '50px' },
    { field: 'imageUrl', title: 'PHOTO', type: 'image', sortable: false, width: '80px' },
    { field: 'name', title: 'NAME', type: 'profile', sortable: true, width: '240px', filterType: 'text' },
    { field: 'linkedExercises', title: 'LINKED EXERCISES', type: 'text', sortable: true, width: '180px', filterType: 'number', filterMode: 'compare' },
    {
      field: 'status',
      title: 'STATUS',
      type: 'badge',
      sortable: true,
      width: '140px',
      filterType: 'select',
      filterOptions: [
        { label: 'Active', value: 'Active' },
        { label: 'Review', value: 'Review' },
        { label: 'Suspended', value: 'Suspended' }
      ]
    },
    { field: 'actions', title: 'ACTION', type: 'action', width: '90px' }
  ];

  selectedEquipment = computed(() =>
    this.equipments().find((equipment) => equipment.id === this.selectedEquipmentId()) ?? null
  );

  constructor() {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    const mockData: EquipmentRecord[] = [
      {
        id: 'EQ-01',
        imageUrl: 'https://images.unsplash.com/photo-1517836357463-d25dfeac3438?auto=format&fit=crop&w=240&q=80',
        name: 'Barbell',
        linkedExercises: 34,
        status: 'Active',
        description: 'Primary free-weight bar used in compound and strength movement plans.',
        aliasConflicts: 0,
        lastReview: 'Apr 04, 2026',
        health: 'Healthy'
      },
      {
        id: 'EQ-02',
        imageUrl: 'https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?auto=format&fit=crop&w=240&q=80',
        name: 'Dumbbell',
        linkedExercises: 52,
        status: 'Active',
        description: 'Multi-use unilateral and bilateral implement across hypertrophy programs.',
        aliasConflicts: 1,
        lastReview: 'Apr 06, 2026',
        health: 'Healthy'
      },
      {
        id: 'EQ-03',
        imageUrl: '',
        name: 'Cable',
        linkedExercises: 26,
        status: 'Review',
        description: 'Cable station metadata requiring reconciliation across naming variants.',
        aliasConflicts: 3,
        lastReview: 'Apr 02, 2026',
        health: 'Attention'
      },
      {
        id: 'EQ-04',
        imageUrl: 'https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?auto=format&fit=crop&w=240&q=80',
        name: 'Machine',
        linkedExercises: 18,
        status: 'Active',
        description: 'Selectorized machine family used for guided movement tracking.',
        aliasConflicts: 0,
        lastReview: 'Apr 05, 2026',
        health: 'Healthy'
      }
    ];

    this.equipments.set(mockData);
    this.filteredEquipments.set(mockData);
    this.isLoading.set(false);
  }

  onSearch(term: string): void {
    this.searchTerm.set(term);
    const value = term.trim().toLowerCase();

    if (!value) {
      this.filteredEquipments.set(this.equipments());
      return;
    }

    const filtered = this.equipments().filter((equipment) =>
      equipment.name.toLowerCase().includes(value)
      || equipment.id.toLowerCase().includes(value)
      || equipment.status.toLowerCase().includes(value)
    );

    this.filteredEquipments.set(filtered);
  }

  handleGridAction(event: { type: string; row: EquipmentRecord }): void {
    if (event.type === 'row-click') {
      this.selectedEquipmentId.set(event.row.id);
      this.isDrawerOpen.set(true);
      document.body.style.overflow = 'hidden';
      return;
    }

    if (event.type === 'edit') {
      this.handleEdit(event.row);
      return;
    }

    if (event.type === 'delete') {
      this.selectedEquipmentId.set(event.row.id);
      this.handleDelete(event.row.id);
    }
  }

  closeDrawer(): void {
    this.isDrawerOpen.set(false);
    this.selectedEquipmentId.set(null);
    document.body.style.overflow = 'auto';
  }

  openAddModal(): void {
    this.isAddModalOpen.set(true);
  }

  closeAddModal(): void {
    this.isAddModalOpen.set(false);
    this.newEquipmentName.set('');
    this.newEquipmentAlias.set('');
  }

  saveNewEquipment(): void {
    console.log('Saved');
    this.closeAddModal();
  }

  handleEdit(row: EquipmentRecord): void {
    this.editingEquipment.set({ ...row });
    this.isEditModalOpen.set(true);
  }

  updateEditingEquipmentField(field: 'imageUrl' | 'name' | 'status' | 'description', value: string): void {
    const current = this.editingEquipment();
    if (!current) {
      return;
    }

    this.editingEquipment.set({
      ...current,
      [field]: field === 'status' ? (value as EquipmentStatus) : value
    });
  }

  saveEquipmentChanges(): void {
    const editing = this.editingEquipment();
    if (!editing) {
      return;
    }

    this.equipments.update((current) =>
      current.map((item) =>
        item.id === editing.id
          ? {
              ...item,
              name: editing.name,
              status: editing.status,
              imageUrl: editing.imageUrl,
              description: editing.description
            }
          : item
      )
    );

    this.onSearch(this.searchTerm());
    this.closeEditModal();
    console.log('Saved!');
  }

  closeEditModal(): void {
    this.isEditModalOpen.set(false);
    this.editingEquipment.set(null);
  }

  handleDelete(id: string | null): void {
    console.log('Deleting equipment:', id);
  }

  handleMerge(id: string | null): void {
    console.log('Merging equipment:', id);
  }

  navigateLinkedEntity(entityType: EquipmentEntityType): void {
    console.info('navigateLinkedEntity placeholder', entityType, this.selectedEquipmentId());
  }
}
