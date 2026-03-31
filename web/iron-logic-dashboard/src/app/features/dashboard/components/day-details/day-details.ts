import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-day-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './day-details.html',
  // در صورت نیاز می‌تونی استایل‌های اختصاصی رو در فایل css قرار بدی
})
export class DayDetailsComponent {
  // دریافت داده‌ها از داشبورد
  @Input({ required: true }) sessions: any[] = [];
  @Input() date: Date | null = null;
  @Input() isPinned = false;

  // ارسال رویدادها به داشبورد
  @Output() close = new EventEmitter<void>();
  @Output() togglePin = new EventEmitter<void>();

  // مدیریت باز/بسته بودن لیست تمرین‌ها (Nested Accordion)
  // کلید (Key) در اینجا می‌تونه نام تمرین یا یک ID یکتا باشه
  expandedExercises = signal<Set<string>>(new Set());

  // متد برای باز و بسته کردن یک تمرین خاص
  toggleExercise(exerciseName: string) {
    const current = new Set(this.expandedExercises());
    if (current.has(exerciseName)) {
      current.delete(exerciseName); // اگر باز بود، ببند
    } else {
      current.add(exerciseName); // اگر بسته بود، باز کن
    }
    this.expandedExercises.set(current);
  }

  // متد برای بررسی اینکه آیا یک تمرین باز است یا نه
  isExpanded(exerciseName: string): boolean {
    return this.expandedExercises().has(exerciseName);
  }

  // متد کمکی برای محاسبه بهترین ست در سمت فرانت‌اِند (اگر از بک‌اِند نیامده باشد)
  getBestSet(sets: any[]): string {
    if (!sets || sets.length === 0) return 'N/A';
    // پیدا کردن سنگین‌ترین ست
    const best = sets.reduce((prev, current) => (prev.weight > current.weight) ? prev : current);
    return `${best.weight} lbs x ${best.reps}${best.rpe ? ` @ ${best.rpe}` : ''}`;
  }
}
