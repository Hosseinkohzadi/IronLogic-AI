import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { UserRow } from '@core/models/user.model';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';

import { GridComponent } from '@shared/grid/grid'; 
import { ColumnConfig } from '@shared/grid/models/column-config';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule, GridComponent],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagementComponent implements OnInit {
  private apiService = inject(IronLogicApiService);

  users = signal<any[]>([]);
  filteredUsers = signal<any[]>([]); 
  searchTerm = signal('');
  selectedUserId = signal<string | null>(null);
  selectedUsers = signal<any[]>([]); 
  isDrawerOpen = signal(false);
  isLoading = signal(true);

  userColumns: ColumnConfig[] = [
    { field: 'selection', title: '', type: 'selection', width: '50px' },
    { field: 'name', title: 'NAME', type: 'profile', sortable: true, width: '220px' },
    { field: 'status', title: 'STATUS', type: 'badge', sortable: true, width: '120px' },
    { field: 'tier', title: 'TIER', type: 'tier', sortable: true, width: '100px' },
    { field: 'sessions', title: 'SESSIONS', type: 'text', sortable: true, width: '100px' },
    { field: 'dailyWeights', title: 'WEIGHTS', type: 'text', sortable: true, width: '100px' },
    { field: 'email', title: 'EMAIL', type: 'email', sortable: true, width: '220px' },
    // تغییر نوع به calendar
    { field: 'lastLogin', title: 'LAST LOGIN', type: 'calendar', sortable: true, width: '140px' }, 
    { field: 'actions', title: 'ACTION', type: 'action', width: '80px' }
  ];

  activeUser = computed(() => this.users().find(u => u.id === this.selectedUserId()));
  
  activeUserDetails = computed(() => {
    const user = this.activeUser();
    if (!user) return null;
    return {
      ...user,
      dailyWeights: Math.floor(user.sessions * 0.6),
      roles: user.tier === 'Elite' ? 'Athlete, Premium, Beta' : 'Athlete, Premium',
      confirmed: 'Yes',
      lastActive: 'Active recently',
      accountCreated: 'Jan 2026',
      supportPriority: user.status === 'Review' ? 'High' : 'Medium',
      syncComplaints: user.status === 'Review' ? 2 : 1,
      billingFriction: 'None',
      retentionFlag: user.sessions < 50 ? 'At Risk' : 'Stable',
      auditTrail: ['Apr 7 · Password reset requested', 'Apr 6 · Role verified by admin']
    };
  });

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading.set(true);
    this.apiService.getUsers().subscribe({
      next: (data: any[]) => { // <-- اینجا اصلاح شد
        if (data) {
          const enrichedData = data.map((u: any, index: number) => {
            let calculatedStatus = 'Active';
            if (u.lockoutEnd && new Date(u.lockoutEnd) > new Date()) {
              calculatedStatus = 'Suspended';
            } else if (u.emailConfirmed === false) {
              calculatedStatus = 'Review';
            }

            const mockDate = new Date();
            mockDate.setDate(mockDate.getDate() - (index * 2)); 

            return {
              ...u,
              status: calculatedStatus,
              dailyWeights: Math.floor(u.sessions * 0.6),
              lastLogin: mockDate.toISOString() 
            };
          });
          
          this.users.set(enrichedData);
          this.filteredUsers.set(enrichedData);
        }
        this.isLoading.set(false);
      },
      error: (err: any) => { // <-- اینجا اصلاح شد
        console.error('API Error:', err);
        this.isLoading.set(false);
      }
    });
  }

  onSearch(term: string) {
    this.searchTerm.set(term);
    const lower = term.toLowerCase();
    const filtered = this.users().filter(u =>
      u.name.toLowerCase().includes(lower) ||
      u.email.toLowerCase().includes(lower) ||
      u.id.toLowerCase().includes(lower)
    );
    this.filteredUsers.set(filtered);
  }

  handleGridAction(event: { type: string, row: any }) {
    if (event.type === 'row-click' || event.type === 'edit') {
      this.selectedUserId.set(event.row.id);
      this.isDrawerOpen.set(true);
      document.body.style.overflow = 'hidden';
    }
  }

  closeDrawer() {
    this.isDrawerOpen.set(false);
    document.body.style.overflow = 'auto';
    setTimeout(() => { this.selectedUserId.set(null); }, 300);
  }
}