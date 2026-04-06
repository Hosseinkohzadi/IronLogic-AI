import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';

@Component({
  selector: 'app-workout-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './workout-chart.html',
  styleUrl: './workout-chart.css'
})
export class WorkoutChartComponent implements OnInit {
  private api = inject(IronLogicApiService);

  multiYearTrend = signal<any[]>([]);
  visibleYears = signal<Map<number, boolean>>(new Map());
  chartWeeks = signal<string[]>(Array.from({length: 52}, (_, i) => `W${i + 1}`));

  ngOnInit() {
    this.loadChartData();
  }

  loadChartData() {
    this.api.getWorkoutStatsWithAdvice().subscribe(stats => {
      if (stats?.dailyWorkouts) {
        this.generateMultiYearChartData(stats.dailyWorkouts);
      }
    });
  }

  toggleYear(year: number) {
    const newMap = new Map(this.visibleYears());
    newMap.set(year, newMap.get(year) === false);
    this.visibleYears.set(newMap);
  }

  private getWeekNumber(d: Date): number {
    const date = new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()));
    date.setUTCDate(date.getUTCDate() + 4 - (date.getUTCDay() || 7));
    const yearStart = new Date(Date.UTC(date.getUTCFullYear(), 0, 1));
    return Math.ceil((((date.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
  }

  generateMultiYearChartData(dailyData: any[]) {
    const yearColors = ['#4f46e5', '#10b981', '#f97316', '#06b6d4'];
    const yearMap = new Map<number, number[]>();

    dailyData.forEach(d => {
      const date = new Date(d.date);
      const year = date.getFullYear();
      let weekNum = Math.min(this.getWeekNumber(date), 52);
      if (!yearMap.has(year)) yearMap.set(year, new Array(52).fill(0));
      yearMap.get(year)![weekNum - 1] += d.workoutSessionDtos.length;
    });

    let globalMax = 1;
    yearMap.forEach(counts => globalMax = Math.max(globalMax, ...counts));

    const series = Array.from(yearMap.keys()).sort((a, b) => b - a).map((year, index) => {
      if (!this.visibleYears().has(year)) this.visibleYears().set(year, true);
      const counts = yearMap.get(year)!;
      const pts = counts.map((count, i) => ({
        week: `W${i+1}`,
        count,
        percentX: (i / 51) * 100,
        percentY: 95 - ((count / globalMax) * 85)
      }));
      return {
        year,
        color: yearColors[index % yearColors.length],
        svgPath: `M ${pts.map(p => `${p.percentX},${p.percentY}`).join(' L ')}`,
        points: pts
      };
    });
    this.multiYearTrend.set(series);
  }
}
