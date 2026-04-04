import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {
  public api = inject(IronLogicApiService);
  private sanitizer = inject(DomSanitizer);

  // وضعیت سیستم
  totalUsers = signal<number>(1);
  totalWorkouts = signal<number>(0);

  // وضعیت پاپ‌آپ
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
      e.name.toLowerCase().includes(term) ||
      (e.mechanics && e.mechanics.toLowerCase().includes(term))
    );
  });

  ngOnInit() {
    this.loadData();
    this.loadGlobalStats();
  }

  loadData() {
    this.api.getExercises(this.currentPage(), this.pageSize).subscribe();
  }

  loadGlobalStats() {
    this.api.getWorkoutStatsWithAdvice().subscribe(stats => {
      if (stats?.dailyWorkouts) {
        const count = stats.dailyWorkouts.reduce((acc, curr) => acc + curr.workoutSessionDtos.length, 0);
        this.totalWorkouts.set(count);
      }
    });
  }

  openModal(type: 'exercises' | 'workouts' | 'users') {
    this.activeModalType.set(type);
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
    this.activeModalType.set(null);
  }

  highlightMatch(text: string): SafeHtml {
    const term = this.searchTerm().trim();
    if (!term || !text) return text;
    const regex = new RegExp(`(${term})`, 'gi');
    const highlighted = text.replace(regex, '<mark>$1</mark>');
    return this.sanitizer.bypassSecurityTrustHtml(highlighted);
  }

  changePage(delta: number) {
    const newPage = this.currentPage() + delta;
    if (newPage > 0) {
      this.currentPage.set(newPage);
      this.loadData();
    }
  }
}
