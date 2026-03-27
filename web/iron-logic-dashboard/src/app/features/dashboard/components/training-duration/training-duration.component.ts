import { Component, ElementRef, ViewChild, AfterViewInit, signal, computed, effect } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common'; // DecimalPipe added
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

// Define types for better type safety
type MetricType = 'duration' | 'volume' | 'reps';
type RangeType = '12w' | 'year' | 'all';

@Component({
  selector: 'app-training-duration',
  standalone: true,
  imports: [CommonModule], // DecimalPipe is available in CommonModule
  templateUrl: './training-duration.component.html',
  styleUrl: './training-duration.component.css'
})
export class TrainingDurationComponent implements AfterViewInit {
  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
  chart?: Chart;

  // 1. These must be public for the HTML template to access them
  public activeMetric = signal<MetricType>('duration');
  public activeRange = signal<RangeType>('12w');

  // 2. Changed from private to public
  public chartData = computed(() => {
    const metric = this.activeMetric();
    return {
      labels: ['Jan 12', 'Jan 26', 'Feb 09', 'Feb 23', 'Mar 09', 'Mar 23'],
      values: metric === 'duration' ? [5, 13, 10, 8, 11, 9] :
        metric === 'volume' ? [15000, 21000, 18000, 19500, 22000, 21629] :
          [400, 622, 550, 480, 590, 610]
    };
  });

  constructor() {
    effect(() => {
      this.updateChart();
    });
  }

  // Helper method to avoid type errors in the HTML template
  setMetric(m: string) {
    this.activeMetric.set(m as MetricType);
  }

  setRange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.activeRange.set(value as RangeType);
  }

  ngAfterViewInit() {
    this.initChart();
  }

  initChart() {
    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    this.chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: this.chartData().labels,
        datasets: [{
          data: this.chartData().values,
          backgroundColor: '#4f46e5',
          borderRadius: 8,
          barThickness: 12
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { display: false } },
          x: { grid: { display: false } }
        }
      }
    });
  }

  updateChart() {
    if (!this.chart) return;
    this.chart.data.labels = this.chartData().labels;
    this.chart.data.datasets[0].data = this.chartData().values;
    this.chart.update();
  }
}
