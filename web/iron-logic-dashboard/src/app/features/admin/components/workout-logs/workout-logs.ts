import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-workout-logs',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-3">
      <div *ngFor="let log of logs" class="log-row group">
        <div class="flex items-center gap-4 p-3 border border-slate-200 rounded-sm bg-white hover:border-emerald-400 transition-colors">
          <div class="text-[10px] font-mono bg-slate-100 p-2 text-slate-500 uppercase">{{log.date}}</div>
          <div class="flex-1">
            <h4 class="text-xs font-bold text-slate-800 uppercase">{{log.title}}</h4>
            <div class="flex gap-4 mt-1">
              <span class="text-[9px] font-mono text-slate-400">VOL: {{log.volume}}</span>
              <span class="text-[9px] font-mono text-slate-400">DUR: {{log.duration}}</span>
            </div>
          </div>
          <button class="text-[10px] font-bold text-emerald-600 hover:underline">FETCH_DETAILS</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .log-row { @apply relative transition-all; }
  `]
})
export class WorkoutLogsComponent {
  @Input() logs: any[] = [];
}
