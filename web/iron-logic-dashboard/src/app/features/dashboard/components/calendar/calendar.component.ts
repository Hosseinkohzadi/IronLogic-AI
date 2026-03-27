import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

// Define interface for compatibility with the new data model
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
  public hoveredDate = signal<string | null>(null);

  // Renamed from workoutDates to workoutData to fix NG8002 error
  @Input() set workoutData(data: DailyWorkout[]) {
    this._workoutData.set(data || []);
  }

  days = computed(() => {
    const date = this.currentDate();
    const month = date.getMonth();
    const year = date.getFullYear();

    const today = new Date();
    const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;

    const firstDayIndex = new Date(year, month, 1).getDay();
    const totalDaysInMonth = new Date(year, month + 1, 0).getDate();

    const daysArray = [];

    // Generate empty cells
    for (let i = 0; i < firstDayIndex; i++) {
      daysArray.push(null);
    }

    // Generate days and map workout sessions
    for (let i = 1; i <= totalDaysInMonth; i++) {
      const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;
      const dayData = this._workoutData().find(w => w.date === dateStr);

      daysArray.push({
        day: i,
        fullDate: dateStr,
        isTrained: !!dayData,
        sessions: dayData?.sessions || [],
        isToday: dateStr === todayStr
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
