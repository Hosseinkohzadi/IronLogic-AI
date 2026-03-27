import { AfterViewInit, Component, computed, effect, ElementRef, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

type MetricType = 'duration' | 'volume' | 'reps';
type RangeType = '12w' | 'year' | 'all';

@Component({
  selector: 'app-training-duration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './training-duration.component.html',
  styleUrl: './training-duration.component.css'
})
export class TrainingDurationComponent implements AfterViewInit {
  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
  chart?: Chart<"bar", number[], string>;

  // Current states using Signals
  public activeMetric = signal<MetricType>('duration');
  public activeRange = signal<RangeType>('12w');

  // Reactive data processing
  public chartData = computed(() => {
    const metric = this.activeMetric();
    const range = this.activeRange();

    let labels = ['Jan 05', 'Jan 12', 'Jan 19', 'Jan 26', 'Feb 02', 'Feb 09', 'Feb 16', 'Feb 23', 'Mar 02', 'Mar 09', 'Mar 16', 'Mar 23'];
    let values: number[] = [];

    if (range === 'year') {
      labels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
      if (metric === 'duration') values = [40, 45, 52, 48, 50, 55, 60, 58, 62, 65, 58, 20];
      else if (metric === 'volume') values = [120000, 135000, 150000, 140000, 145000, 160000, 170000, 165000, 175000, 180000, 170000, 45000];
      else values = [3000, 3200, 3500, 3100, 3300, 3600, 3800, 3700, 3900, 4000, 3800, 1200];
    } else if (range === 'all') {
      labels = ['2024', '2025', '2026'];
      values = metric === 'duration' ? [450, 520, 180] : [1500000, 1850000, 600000];
    } else {
      values = metric === 'duration'
        ? [8.5, 9.2, 10.1, 10.5, 10.2, 12.4, 9.8, 7.5, 11.2, 13.1, 12.8, 5.0]
        : metric === 'volume'
          ? [18000, 19500, 21000, 20500, 19000, 22500, 21000, 18500, 23000, 24500, 24000, 15000]
          : [450, 500, 580, 610, 590, 680, 600, 520, 650, 710, 690, 420];
    }

    return { labels, values };
  });

  constructor() {
    effect(() => {
      const currentData = this.chartData();

      if (this.chart) {
        this.chart.data.labels = currentData.labels;
        this.chart.data.datasets[0].data = currentData.values;
        this.chart.update();
      }
    });
  }

  ngAfterViewInit() {
    // Timeout helps Chart.js find the correct dimensions after DOM rendering
    setTimeout(() => this.initChart(), 0);
  }

  setMetric(m: string) {
    this.activeMetric.set(m as MetricType);
  }

  setRange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.activeRange.set(select.value as RangeType);
    select.blur();
  }

  private initChart() {
    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    this.chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: this.chartData().labels,
        datasets: [{
          data: this.chartData().values,
          backgroundColor: (c) => {
            const chart = c.chart;
            const {ctx, chartArea} = chart;
            if (!chartArea) return '#4f46e5';
            const gradient = ctx.createLinearGradient(0, chartArea.bottom, 0, chartArea.top);
            gradient.addColorStop(0, '#4f46e5');
            gradient.addColorStop(1, '#a5b4fc');
            return gradient;
          },
          borderRadius: 8,
          barThickness: 18
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            enabled: false,
            external: (c) => this.createCustomTooltip(c)
          }
        },
        scales: {
          y: { beginAtZero: true, grid: { color: 'rgba(241, 245, 249, 1)' }, ticks: { color: '#94a3b8', font: { size: 10, weight: 600 } } },
          x: { grid: { display: false }, ticks: { color: '#94a3b8', font: { size: 10, weight: 600 } } }
        }
      }
    });
  }

  private updateChart() {
    if (!this.chart) return;
    const data = this.chartData();
    this.chart.data.labels = data.labels;
    this.chart.data.datasets[0].data = data.values;
    this.chart.update();
  }

  private createCustomTooltip(context: any) {
    let tooltipEl = document.getElementById('chartjs-tooltip');
    if (!tooltipEl) {
      tooltipEl = document.createElement('div');
      tooltipEl.id = 'chartjs-tooltip';
      tooltipEl.innerHTML = '<div class="tooltip-content"></div>';
      document.body.appendChild(tooltipEl);
    }

    const tooltipModel = context.tooltip;
    if (tooltipModel.opacity === 0) {
      tooltipEl.style.opacity = '0';
      return;
    }

    if (tooltipModel.body) {
      const title = tooltipModel.title[0] || '';
      const rawValue = tooltipModel.dataPoints[0].raw;
      const metric = this.activeMetric();

      let rangeDisplay = title;
      if (this.activeRange() === '12w') {
        const startDate = new Date(`${title}, 2026`);
        const endDate = new Date(startDate);
        endDate.setDate(startDate.getDate() + 7);
        const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        rangeDisplay = `${title} - ${months[endDate.getMonth()]} ${endDate.getDate().toString().padStart(2, '0')}`;
      }

      let valDisplay = metric === 'duration'
        ? `${Math.floor(rawValue)}h ${Math.round((rawValue - Math.floor(rawValue)) * 60)}min`
        : `${rawValue.toLocaleString()} ${metric === 'volume' ? 'lbs' : 'reps'}`;

      const root = tooltipEl.querySelector('.tooltip-content');
      if (root) {
        root.innerHTML = `
          <div style="color: #94a3b8; font-size: 11px; font-weight: 600; margin-bottom: 2px;">${rangeDisplay}</div>
          <div style="color: #64748b; font-size: 14px; font-weight: 700;">${valDisplay}</div>
        `;
      }
    }

    const pos = context.chart.canvas.getBoundingClientRect();
    tooltipEl.style.opacity = '1';
    tooltipEl.style.position = 'absolute';
    tooltipEl.style.left = pos.left + window.pageXOffset + tooltipModel.caretX + 'px';
    tooltipEl.style.top = pos.top + window.pageYOffset + tooltipModel.caretY - 10 + 'px';
    tooltipEl.style.transform = 'translate(-50%, -100%)';
    tooltipEl.style.pointerEvents = 'none';
  }
}
