import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-directory',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <div *ngFor="let user of users" class="user-card">
        <div class="avatar">{{user.initial}}</div>
        <div class="flex-1">
          <p class="text-[11px] font-bold text-slate-800 uppercase">{{user.name}}</p>
          <p class="text-[9px] font-mono text-slate-400">{{user.role}}</p>
        </div>
        <div class="status-tag" [ngClass]="{'online': user.status === 'Online'}">
          {{user.status}}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .user-card { @apply flex items-center gap-3 p-3 border border-slate-200 rounded-sm bg-white hover:bg-slate-50 transition-colors; }
    .avatar { @apply w-8 h-8 bg-slate-800 text-white flex items-center justify-center font-mono text-xs rounded-sm; }
    .status-tag { @apply text-[8px] font-bold uppercase px-1.5 py-0.5 border border-slate-200 rounded-sm text-slate-400; }
    .status-tag.online { @apply text-emerald-600 border-emerald-200 bg-emerald-50; }
  `]
})
export class UserDirectoryComponent {
  @Input() users: any[] = [];
}
