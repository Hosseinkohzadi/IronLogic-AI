import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.css'
})
export class CalendarComponent {
  private _workoutDates = signal<string[]>([]);

  @Input() set workoutDates(dates: string[]) {
    this._workoutDates.set(dates);
  }

  // Use a signal to manage the current date
  currentDate = signal(new Date());

  // Compute the days of the month whenever the current date or workout data changes
  days = computed(() => {
    const date = this.currentDate();
    const month = date.getMonth();
    const year = date.getFullYear();

    // The current system date
    const today = new Date();
    const todayDay = today.getDate();
    const todayMonth = today.getMonth();
    const todayYear = today.getFullYear();

    const firstDay = new Date(year, month, 1).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();

    const daysArray = [];
    for (let i = 0; i < firstDay; i++) { daysArray.push(null); }

    for (let i = 1; i <= daysInMonth; i++) {
      const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;

      daysArray.push({
        day: i,
        fullDate: dateStr,
        isTrained: this._workoutDates().includes(dateStr),
        // Precise comparison to identify "today"
        isToday: i === todayDay && month === todayMonth && year === todayYear
      });
    }
    return daysArray;
  });

  monthName = computed(() =>
    this.currentDate().toLocaleString('default', { month: 'long', year: 'numeric' })
  );

  prevMonth() {
    const d = this.currentDate();
    this.currentDate.set(new Date(d.getFullYear(), d.getMonth() - 1, 1));
  }

  nextMonth() {
    const d = this.currentDate();
    this.currentDate.set(new Date(d.getFullYear(), d.getMonth() + 1, 1));
  }
}
