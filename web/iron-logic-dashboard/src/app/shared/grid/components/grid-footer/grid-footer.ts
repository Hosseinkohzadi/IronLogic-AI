import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-grid-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid-footer px-4 py-2 border-t border-slate-100 bg-slate-50/50 text-[10px] font-bold text-slate-400 uppercase tracking-widest text-left">
      Total Records: {{ count }}
    </div>
  `
})
export class GridFooterComponent {
  @Input() count: number = 0;
}
