import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent } from '@shared/grid/grid';
import { ColumnConfig } from '@shared/grid/models/column-config';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, GridComponent],
  template: `
    <div class="p-2">
      <app-grid
        [columns]="userColumns"
        [data]="users()">
      </app-grid>
    </div>
  `
})
export class UserManagement {
  userColumns: ColumnConfig[] = [
    { field: 'initial', title: 'آواتار', width: '10%' },
    { field: 'name', title: 'نام کاربر', width: '25%' },
    { field: 'role', title: 'نقش', width: '20%' },
    { field: 'division', title: 'دپارتمان', width: '20%' },
    { field: 'status', title: 'وضعیت', width: '10%' },
    { field: 'lastActive', title: 'فعالیت', width: '15%' }
  ];

  // users = signal([
  //   { id: 1, name: 'Hossein', role: 'Super Admin', division: 'Classic Physique', status: 'Online', lastActive: 'Now', initial: 'H' },
  //   { id: 2, name: 'Admin_Test', role: 'Editor', division: 'Management', status: 'Offline', lastActive: '2 hours ago', initial: 'A' },
  //   { id: 3, name: 'User_04', role: 'Premium Member', division: 'Powerlifting', status: 'Offline', lastActive: 'Yesterday', initial: 'U' }
  // ]);

// ایجاد ۱۰۰۰ سطر داده تستی به صورت سیگنال
  users = signal(
    Array.from({ length: 1000 }).map((_, i) => ({
      id: i + 1,
      name: `User ${i + 1}`,
      role: i % 3 === 0 ? 'Admin' : 'Athlete',
      status: i % 2 === 0 ? 'Online' : 'Offline',
      lastActive: `${i % 24} hours ago`,
      initial: 'U'
    }))
  );

}
