import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-exercise-management',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule],
  templateUrl: './exercise-management.html',
  styleUrl: './exercise-management.css'
})
export class ExerciseManagementComponent implements OnInit {
  public api = inject(IronLogicApiService);

  searchTerm = signal('');
  currentPage = signal(1);
  pageSize = 15;

  exercises = this.api.exercises;

  // فیلتر کردن بر اساس متغیرهای موجود در اینترفیس جدید
  filteredExercises = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) return this.exercises();
    return this.exercises().filter(e =>
      e.name.toLowerCase().includes(term) ||
      (e.mechanics && e.mechanics.toLowerCase().includes(term))
    );
  });

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getExercises(this.currentPage(), this.pageSize).subscribe();
  }

  onDelete(id: string) {
    if (confirm('Are you sure you want to drop this entity?')) {
      this.api.deleteExercise(id).subscribe(() => this.loadData());
    }
  }

  changePage(delta: number) {
    this.currentPage.update(p => p + delta);
    this.loadData();
  }
}
