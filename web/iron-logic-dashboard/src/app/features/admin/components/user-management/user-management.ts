import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { UserRow } from '@core/models/user.model';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { GridComponent } from '@shared/grid/grid'; 
import { ColumnConfig } from '@shared/grid/models/column-config';
import { KpiCardComponent } from '@shared/kpi-card/kpi-card.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule, GridComponent, KpiCardComponent],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagementComponent implements OnInit {
  private apiService = inject(IronLogicApiService);
  private router = inject(Router);

  cards = [
    {
      label: 'PREMIUM SUBSCRIBERS',
      val: '842',
      trend: '+12.5%',
      context: 'pro & elite tiers',
      icon: 'star',
      info: 'Users on paid Pro/Elite plans. This tracks monetization quality and recurring revenue strength. A positive trend means premium conversion is improving.'
    },
    {
      label: 'WEEKLY ACTIVE (WAU)',
      val: '1,420',
      trend: '+5.2%',
      context: 'logged a workout',
      icon: 'activity',
      info: 'Unique users active in the last 7 days. This indicates engagement health, not just signups. A positive trend means more users are returning weekly.'
    },
    {
      label: 'TOTAL SESSIONS',
      val: '84.5K',
      trend: '+18.2%',
      context: 'platform volume',
      icon: 'zap',
      info: 'Total workout sessions completed in the period. This reflects platform usage volume and habit intensity. A positive trend means stronger activity throughput.'
    },
    {
      label: 'CHURN RISK',
      val: '156',
      trend: '-3.4%',
      context: 'inactive > 14 days',
      icon: 'alert-triangle',
      info: 'Users likely to churn due to inactivity beyond 14 days. Lower is better for retention. A negative trend here is good because fewer users are at risk.'
    }
  ];

  users = signal<any[]>([]);
  filteredUsers = signal<any[]>([]); 
  searchTerm = signal('');
  selectedUserId = signal<string | null>(null);
  selectedUsers = signal<any[]>([]); 
  isDrawerOpen = signal(false);
  isLoading = signal(true);

  userColumns: ColumnConfig[] = [
    { field: 'selection', title: '', type: 'selection', width: '50px' },
    { field: 'name', title: 'NAME', type: 'profile', sortable: true, width: '250px', locked: true, filterType: 'text' },
    {
      field: 'status',
      title: 'STATUS',
      type: 'badge',
      sortable: true,
      width: '120px',
      locked: true,
      filterType: 'select',
      filterOptions: [
        { label: 'Active', value: 'Active' },
        { label: 'Review', value: 'Review' },
        { label: 'Suspended', value: 'Suspended' }
      ]
    },
    {
      field: 'tier',
      title: 'TIER',
      type: 'tier',
      sortable: true,
      width: '100px',
      filterType: 'select',
      filterOptions: [
        { label: 'Elite', value: 'Elite' },
        { label: 'Premium', value: 'Premium' },
        { label: 'Pro', value: 'Pro' },
        { label: 'Basic', value: 'Basic' },
        { label: 'Free', value: 'Free' }
      ]
    },
    { field: 'sessions', title: 'SESSIONS', type: 'number', sortable: true, width: '100px', filterType: 'number', filterMode: 'compare' },
    { field: 'dailyWeights', title: 'WEIGHTS', type: 'number', sortable: true, width: '100px', filterType: 'number', filterMode: 'compare' },
    { field: 'email', title: 'EMAIL', type: 'email', sortable: true, width: '220px', filterType: 'text' },
    { field: 'lastLogin', title: 'LAST LOGIN', type: 'calendar', sortable: true, width: '140px', filterType: 'date', filterMode: 'exact' },
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
          const avatarUrls = [
            'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=96&q=80',
            'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=96&q=80',
            'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=96&q=80',
            'https://images.unsplash.com/photo-1502685176499-5d707b212601?auto=format&fit=crop&w=96&q=80',
            'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=96&q=80'
          ];

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
              profileImageUrl: avatarUrls[index % avatarUrls.length],
              dailyWeights: Math.floor(u.sessions * 0.6),
              lastLogin: mockDate.toISOString() 
            };
          });

          const targetRecordCount = 100;
          const expandedData = enrichedData.length
            ? Array.from({ length: targetRecordCount }, (_, index) => {
                const source = enrichedData[index % enrichedData.length];
                const uniqueSuffix = String(index + 1).padStart(3, '0');
                const duplicateWave = Math.floor(index / enrichedData.length);

                const loginDate = new Date();
                loginDate.setDate(loginDate.getDate() - index);

                const baseSessions = Number(source.sessions ?? 0);
                const nextSessions = baseSessions + (index % 9);

                const baseEmail = String(source.email ?? '').trim();
                let nextEmail = `user${uniqueSuffix}@example.com`;
                if (baseEmail.includes('@')) {
                  const [local, domain] = baseEmail.split('@');
                  nextEmail = `${local}+${uniqueSuffix}@${domain}`;
                }

                return {
                  ...source,
                  id: source.id ? `${source.id}-${uniqueSuffix}` : `usr-${uniqueSuffix}`,
                  userName: source.userName
                    ? `${source.userName}${duplicateWave > 0 ? `_${duplicateWave + 1}` : ''}`
                    : `user_${uniqueSuffix}`,
                  name: source.name ? `${source.name} ${uniqueSuffix}` : `User ${uniqueSuffix}`,
                  email: nextEmail,
                  sessions: nextSessions,
                  dailyWeights: Math.floor(nextSessions * 0.6),
                  lastLogin: loginDate.toISOString(),
                  profileImageUrl: source.profileImageUrl,
                  isSelected: false
                };
              })
            : [];

          this.users.set(expandedData);
          this.filteredUsers.set(expandedData);
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

  navigateToEntity(entityType: 'sessions' | 'weights' | 'exercises') {
    const currentId = this.selectedUserId();
    if (!currentId) return;

    this.closeDrawer();

    const routeMap: Record<'sessions' | 'weights' | 'exercises', string[]> = {
      sessions: ['/admin/sessions'],
      weights: ['/admin/weights'],
      exercises: ['/admin/exercises']
    };

    this.router.navigate(routeMap[entityType], { queryParams: { userId: currentId } });
  }

  MapsToEntity(entityType: 'sessions' | 'weights' | 'exercises') {
    this.navigateToEntity(entityType);
  }
}