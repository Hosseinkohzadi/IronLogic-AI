import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser'; // برای هایلایت کردن امن

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

  exercises = this.api.exercises;
  isLoading = this.api.isLoading;
  searchTerm = signal('');

  // وضعیت صفحه‌بندی
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
  }

  loadData() {
    // ارسال شماره صفحه فعلی و سایز صفحه به سرویس
    this.api.getExercises(this.currentPage(), this.pageSize).subscribe({
      next: (data) => {
        // اگر سرویس خودش سیگنال را آپدیت نمی‌کند، اینجا دستی آپدیت کنید:
        // this.api.exercises.set(data);
        console.log('داده‌های صفحه جدید دریافت شد:', data);
      },
      error: (err) => console.error('خطا در دریافت اطلاعات:', err)
    });
  }

  // متد جدید برای هایلایت کردن متن جستجو شده
  highlightMatch(text: string): SafeHtml {
    const term = this.searchTerm().trim();
    if (!term || !text) return text;

    const regex = new RegExp(`(${term})`, 'gi');
    const highlighted = text.replace(regex, '<mark class="bg-indigo-100 text-indigo-700 p-0 rounded">$1</mark>');
    return this.sanitizer.bypassSecurityTrustHtml(highlighted);
  }

  changePage(delta: number) {
    const newPage = this.currentPage() + delta;
    if (newPage > 0) {
      this.currentPage.set(newPage);
      this.loadData();
    }
  }

  onDelete(id: string) { // استفاده از string برای GUID
    if (confirm('Are you sure?')) {
      this.api.deleteExercise(id).subscribe(() => this.loadData());
    }
  }
}
