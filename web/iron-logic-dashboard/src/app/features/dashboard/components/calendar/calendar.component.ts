import { Component, computed, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Day {
  date: Date;
  dayOfMonth: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  hasWorkout: boolean;
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.css'
})
export class CalendarComponent {
  @Input() set workoutDates(dates: string[]) {
    this.workouts.set(new Set(dates.map(d => new Date(d).toDateString())));
  }

  private workouts = signal<Set<string>>(new Set());
  
  currentDate = signal(new Date());
  
  daysOfWeek = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];

  monthAndYear = computed(() => {
    return this.currentDate().toLocaleString('default', { month: 'long', year: 'numeric' });
  });

  calendarGrid = computed<Day[][]>(() => {
    const date = this.currentDate();
    const year = date.getFullYear();
    const month = date.getMonth();
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const firstDayOfMonth = new Date(year, month, 1);
    const lastDayOfMonth = new Date(year, month + 1, 0);

    const days: Day[] = [];

    // Get the day of the week for the first day of the month (0=Sun, 1=Mon, ..., 6=Sat)
    let startDayOfWeek = firstDayOfMonth.getDay();
    if (startDayOfWeek === 0) { // Adjust so Monday is 0 and Sunday is 6
      startDayOfWeek = 6;
    } else {
      startDayOfWeek -= 1;
    }

    // Add days from the previous month
    for (let i = startDayOfWeek; i > 0; i--) {
      const prevMonthDate = new Date(firstDayOfMonth);
      prevMonthDate.setDate(prevMonthDate.getDate() - i);
      days.push({
        date: prevMonthDate,
        dayOfMonth: prevMonthDate.getDate(),
        isCurrentMonth: false,
        isToday: false,
        hasWorkout: false
      });
    }

    // Add days of the current month
    for (let i = 1; i <= lastDayOfMonth.getDate(); i++) {
      const currentDay = new Date(year, month, i);
      days.push({
        date: currentDay,
        dayOfMonth: i,
        isCurrentMonth: true,
        isToday: currentDay.getTime() === today.getTime(),
        hasWorkout: this.workouts().has(currentDay.toDateString())
      });
    }

    // Add days from the next month to fill the grid
    const grid_size = 42; // 6 weeks * 7 days
    let nextMonthDay = 1;
    while (days.length < grid_size) {
      const nextMonthDate = new Date(lastDayOfMonth);
      nextMonthDate.setDate(nextMonthDate.getDate() + nextMonthDay);
      days.push({
        date: nextMonthDate,
        dayOfMonth: nextMonthDate.getDate(),
        isCurrentMonth: false,
        isToday: false,
        hasWorkout: false
      });
      nextMonthDay++;
    }

    // Chunk into weeks
    const weeks: Day[][] = [];
    for (let i = 0; i < days.length; i += 7) {
      weeks.push(days.slice(i, i + 7));
    }
    
    // Only return the necessary number of weeks
    const requiredWeeks = Math.ceil((startDayOfWeek + lastDayOfMonth.getDate()) / 7);
    return weeks.slice(0, requiredWeeks);
  });
}
