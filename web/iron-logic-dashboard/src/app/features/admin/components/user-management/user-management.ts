import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { UserRow } from '@core/models/user.model';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagementComponent implements OnInit {
  private apiService = inject(IronLogicApiService);

  users = signal<UserRow[]>([]);
  searchTerm = signal('');
  selectedUserId = signal<string | null>(null);
  selectedUserIndices = signal<number[]>([]);
  isDrawerOpen = signal(false);
  isLoading = signal(true);

  stats = signal({
    active: { count: 2011, growth: '+4.4%' },
    suspended: { count: 38, growth: '-2.1%' },
    confirmed: { count: '97.2%', growth: '+0.6%' },
    resets: { count: 14, growth: '+1.1%' }
  });

  filteredUsers = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    const allUsers = this.users();
    if (!term) return allUsers;
    return allUsers.filter(u =>
      u.name.toLowerCase().includes(term) || u.email.toLowerCase().includes(term)
    );
  });

  activeUser = computed(() => this.users().find(u => u.id === this.selectedUserId()));

  activeUserDetails = computed(() => {
    const user = this.activeUser();
    if (!user) return null;
    return {
      ...user,
      roles: user.tier === 'Elite' ? 'Athlete, Premium, Beta' : 'Athlete, Premium',
      confirmed: 'Yes',
      dailyWeights: Math.floor(user.sessions * 0.7),
      lastActive: '2h ago',
      accountCreated: 'Jan 2026',
      supportPriority: user.status === 'Review' ? 'High' : 'Medium',
      syncComplaints: user.status === 'Review' ? 2 : 1,
      billingFriction: 'None',
      retentionFlag: user.sessions < 50 ? 'At Risk' : 'Stable',
      auditTrail: [
        'Apr 7 · Password reset requested',
        'Apr 6 · Role verified by admin',
        'Apr 4 · Premium tier renewed'
      ]
    };
  });

  ngOnInit() {
    setTimeout(() => {
      this.stats.set({
        active: { count: 2011, growth: '+4.4%' },
        suspended: { count: 38, growth: '-2.1%' },
        confirmed: { count: '97.2%', growth: '+0.6%' },
        resets: { count: 14, growth: '+1.1%' }
      });
      this.loadData();
    });
  }

  loadData() {
    this.isLoading.set(true);
    this.apiService.getUsers().subscribe({
      next: (data) => {
        if (data) {
          this.users.set(data);
          setTimeout(() => this.isLoading.set(false), 200);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  toggleSelection(event: Event, index: number) {
    event.stopPropagation();
    this.selectedUserIndices.update(indices =>
      indices.includes(index) ? indices.filter(i => i !== index) : [...indices, index]
    );
  }

  selectUser(user: UserRow) {
    this.selectedUserId.set(user.id);
    this.isDrawerOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeDrawer() {
    this.isDrawerOpen.set(false);
    document.body.style.overflow = 'auto';
    setTimeout(() => {
      this.selectedUserId.set(null);
    }, 300);
  }
}
