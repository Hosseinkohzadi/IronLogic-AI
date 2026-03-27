import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-metric-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="rounded-3xl bg-white p-8 shadow-[0_2px_10px_-3px_rgba(6,81,237,0.05)] flex flex-col justify-between h-full">
      <div>
        <div [ngClass]="themeClass()" class="w-10 h-10 rounded-full flex items-center justify-center mb-4">
          <ng-content select="[icon]"></ng-content> </div>
        <p class="text-xs font-bold tracking-widest text-slate-400 uppercase mb-1">{{ title() }}</p>
        <h3 class="text-4xl font-black text-slate-800 tracking-tight">
          {{ value() }} <span *ngIf="unit()" class="text-lg font-medium text-slate-400 ml-1">{{ unit() }}</span>
        </h3>
      </div>
      
      <div class="mt-6 w-full">
        <ng-content select="[footer]"></ng-content>
      </div>
    </div>
  `
})
export class MetricCardComponent {
  title = input.required<string>();
  value = input.required<string | number | null>();
  unit = input<string>();
  theme = input<'indigo' | 'orange' | 'emerald'>('indigo');

  themeClass() {
    switch (this.theme()) {
      case 'orange': return 'bg-orange-50 text-orange-500';
      case 'emerald': return 'bg-emerald-50 text-emerald-500';
      default: return 'bg-indigo-50 text-indigo-600';
    }
  }
}
