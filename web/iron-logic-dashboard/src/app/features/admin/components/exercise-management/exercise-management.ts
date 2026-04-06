import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-exercise-management',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="admin-panel-content">
      <div class="panel-header">
        <span class="text-[10px] font-mono text-slate-400">DB_SECTOR: EXERCISES</span>
        <button class="action-btn-primary">+ ADD_NEW_ENTRY</button>
      </div>

      <div class="table-container">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50 border-y border-slate-200 text-[10px] font-mono text-slate-500">
              <th class="p-3">UID</th>
              <th class="p-3">EXERCISE_NAME</th>
              <th class="p-3">MECHANICS</th>
              <th class="p-3 text-right">OPERATIONS</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let ex of exercises" class="border-b border-slate-100 hover:bg-slate-50 font-mono text-[11px]">
              <td class="p-3 text-slate-400">#{{ex.id.substring(0,8)}}</td>
              <td class="p-3 font-bold text-slate-700">{{ex.name}}</td>
              <td class="p-3 text-slate-500">{{ex.mechanics || 'COMPOUND'}}</td>
              <td class="p-3 text-right space-x-2">
                <button class="text-indigo-600 hover:underline">EDIT</button>
                <button class="text-red-500 hover:underline">DEL</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .panel-header { @apply flex justify-between items-center mb-4 p-2 border-l-2 border-indigo-600 bg-slate-50; }
    .action-btn-primary { @apply bg-slate-900 text-white px-3 py-1 text-[10px] font-bold rounded hover:bg-slate-800; }
    .table-container { @apply border border-slate-200 rounded-sm bg-white; }
  `]
})
export class ExerciseManagementComponent {
  @Input() exercises: any[] = [];
}
