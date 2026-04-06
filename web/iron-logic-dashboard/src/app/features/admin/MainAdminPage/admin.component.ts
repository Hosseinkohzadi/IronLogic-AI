import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { WorkoutChartComponent } from '@features/admin/components/workout-chart/workout-chart';
import { UserDirectoryComponent } from '@features/admin/components/user-directory/user-directory';
import { UserManagement } from '../components/user-management/user-management';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, WorkoutChartComponent,UserDirectoryComponent,UserManagement],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {
  public api = inject(IronLogicApiService);
  private sanitizer = inject(DomSanitizer);

  totalUsers = signal<number>(1);
  totalWorkouts = signal<number>(653);
  isModalOpen = signal(false);
  activeModalType = signal<'exercises' | 'workouts' | 'users' | null>(null);

  exercises = this.api.exercises;
  searchTerm = signal('');
  currentPage = signal(1);
  pageSize = 20;

  filteredExercises = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) return this.exercises();
    return this.exercises().filter(e =>
      e.name.toLowerCase().includes(term) || (e.mechanics && e.mechanics.toLowerCase().includes(term))
    );
  });

  ngOnInit() {
    this.loadData();
    this.api.getWorkoutStatsWithAdvice().subscribe(stats => {
      if (stats?.dailyWorkouts) {
        const count = stats.dailyWorkouts.reduce((acc, curr) => acc + curr.workoutSessionDtos.length, 0);
        this.totalWorkouts.set(count > 0 ? count : 653);
      }
    });
  }

  loadData() {
    this.api.getExercises(this.currentPage(), this.pageSize).subscribe();
  }

  openModal(type: 'exercises' | 'workouts' | 'users') {
    this.activeModalType.set(type);
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
    setTimeout(() => this.activeModalType.set(null), 300);
  }

  highlightMatch(text: string): SafeHtml {
    const term = this.searchTerm().trim();
    if (!term || !text) return text;
    const regex = new RegExp(`(${term})`, 'gi');
    return this.sanitizer.bypassSecurityTrustHtml(text.replace(regex, '<mark>$1</mark>'));
  }

  changePage(delta: number) {
    this.currentPage.update(p => p + delta);
    this.loadData();
  }

  onDelete(id: string) {
    if (confirm('Are you sure you want to drop this entity?')) {
      this.api.deleteExercise(id).subscribe(() => this.loadData());
    }
  }
}
