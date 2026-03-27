import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface WorkoutSession {
  type: string;
  duration: string;
}

export interface DailyWorkout {
  date: string;
  sessions: WorkoutSession[];
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.css'
})
export class CalendarComponent {
  private _workoutData = signal<DailyWorkout[]>([]);
  public currentDate = signal(new Date());

  public weekDays = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];

  @Input() set workoutData(data: DailyWorkout[]) {
    this._workoutData.set(data || []);
  }

  // گرفتن نام ماه برای هدر تقویم
  public monthName = computed(() => {
    return this.currentDate().toLocaleString('en-US', { month: 'long', year: 'numeric' });
  });

  public calendarDays = computed(() => {
    const date = this.currentDate();
    const month = date.getMonth();
    const year = date.getFullYear();

    const firstDayIndex = new Date(year, month, 1).getDay();
    const totalDaysInMonth = new Date(year, month + 1, 0).getDate();

    const daysArray = [];
    const today = new Date();

    // روزهای ماه قبل
    for (let i = 0; i < firstDayIndex; i++) {
      const prevMonthDate = new Date(year, month, 0 - (firstDayIndex - i - 1));
      daysArray.push({ date: prevMonthDate, hasWorkout: false, isAdjacentMonth: true, isToday: false });
    }

    // روزهای ماه جاری
    for (let i = 1; i <= totalDaysInMonth; i++) {
      const currentDay = new Date(year, month, i);
      const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;
      const dayData = this._workoutData().find(w => w.date === dateStr);

      // تشخیص روز جاری
      const isToday = currentDay.getDate() === today.getDate() &&
        currentDay.getMonth() === today.getMonth() &&
        currentDay.getFullYear() === today.getFullYear();

      daysArray.push({
        date: currentDay,
        hasWorkout: !!dayData,
        isAdjacentMonth: false,
        isToday: isToday // اضافه شدن مجدد
      });
    }

    return daysArray;
  });

  // متدهای جابجایی ماه‌ها
  prevMonth() {
    const d = this.currentDate();
    this.currentDate.set(new Date(d.getFullYear(), d.getMonth() - 1, 1));
  }

  nextMonth() {
    const d = this.currentDate();
    this.currentDate.set(new Date(d.getFullYear(), d.getMonth() + 1, 1));
  }
}
