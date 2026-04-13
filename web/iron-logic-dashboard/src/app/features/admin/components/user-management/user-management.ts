import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  signal,
  inject,
  viewChild,
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';

import { DetailViewConfig, GridComponent } from '@shared/grid/grid';

import { ColumnConfig } from '@shared/grid/models/column-config';
import { KpiCardComponent } from '@shared/kpi-card/kpi-card.component';
import { UserRowDrawerComponent } from './user-row-drawer';
import { ApplicationUser } from '@core/models';
import { AdminUserGridModel, UserService } from '@core/services/user.service';
import { NotificationService } from '@core/services/notification.service';

type UserGridStatus = 'Active' | 'Review' | 'Banned';
type UserGridTier = 'Basic' | 'Pro' | 'Elite';

interface UserFormState {
  fullName: string;
  email: string;
  tier: UserGridTier;
  status: 'Active' | 'Review';
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule, GridComponent, KpiCardComponent],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserManagementComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly notificationService = inject(NotificationService);
  private readonly gridRef = viewChild(GridComponent);

  readonly cards = [
    {
      label: 'PREMIUM SUBSCRIBERS',
      val: '842',
      trend: '+12.5%',
      context: 'pro & elite tiers',
      icon: 'star',
      info: 'Users on paid Pro/Elite plans. This tracks monetization quality and recurring revenue strength. A positive trend means premium conversion is improving.',
    },
    {
      label: 'WEEKLY ACTIVE (WAU)',
      val: '1,420',
      trend: '+5.2%',
      context: 'logged a workout',
      icon: 'activity',
      info: 'Unique users active in the last 7 days. This indicates engagement health, not just signups. A positive trend means more users are returning weekly.',
    },
    {
      label: 'TOTAL SESSIONS',
      val: '84.5K',
      trend: '+18.2%',
      context: 'platform volume',
      icon: 'zap',
      info: 'Total workout sessions completed in the period. This reflects platform usage volume and habit intensity. A positive trend means stronger activity throughput.',
    },
    {
      label: 'CHURN RISK',
      val: '156',
      trend: '-3.4%',
      context: 'inactive > 14 days',
      icon: 'alert-triangle',
      info: 'Users likely to churn due to inactivity beyond 14 days. Lower is better for retention. A negative trend here is good because fewer users are at risk.',
    },
  ];

  readonly users = signal<AdminUserGridModel[]>([]);
  readonly users$ = computed(() => {
    const lower = this.searchTerm().trim().toLowerCase();
    if (!lower) {
      return this.users();
    }

    return this.users().filter(
      (u) =>
        u.name.toLowerCase().includes(lower) ||
        u.email.toLowerCase().includes(lower) ||
        u.role.toLowerCase().includes(lower) ||
        u.plan.toLowerCase().includes(lower),
    );
  });
  readonly searchTerm = signal('');
  readonly selectedUsers = signal<any[]>([]);
  readonly isUserFormOpen = signal(false);
  readonly isLoading = signal(true);
  readonly editingUserId = signal<string | null>(null);

  readonly userDrawerConfig: DetailViewConfig = {
    enabled: true,
    position: 'right',
    component: UserRowDrawerComponent,
  };

  readonly userForm = signal<UserFormState>({
    fullName: '',
    email: '',
    tier: 'Basic',
    status: 'Active',
  });

  readonly userColumns: ColumnConfig[] = [
    { field: 'selection', title: '', type: 'selection', width: '50px' },
    {
      field: 'name',
      title: 'USER',
      type: 'profile',
      sortable: true,
      width: '300px',
      locked: true,
      filterType: 'text',
      subfield: 'email',
    },
    {
      field: 'email',
      title: 'EMAIL',
      type: 'email',
      sortable: true,
      width: '260px',
      filterType: 'text',
    },
    {
      field: 'role',
      title: 'ROLE',
      type: 'badge',
      badgeStyle: 'userRole',
      sortable: true,
      width: '120px',
      filterType: 'select',
      filterOptions: [
        { label: 'Admin', value: 'Admin' },
        { label: 'Coach', value: 'Coach' },
        { label: 'Athlete', value: 'Athlete' },
      ],
    },
    {
      field: 'plan',
      title: 'PLAN',
      type: 'badge',
      badgeStyle: 'subscriptionPlan',
      sortable: true,
      width: '120px',
      filterType: 'select',
      filterOptions: [
        { label: 'Basic', value: 'Basic' },
        { label: 'Pro', value: 'Pro' },
        { label: 'Elite', value: 'Elite' },
      ],
    },
    {
      field: 'status',
      title: 'STATUS',
      type: 'badge',
      badgeStyle: 'subscriptionStatus',
      sortable: true,
      width: '120px',
      filterType: 'select',
      filterOptions: [
        { label: 'Active', value: 'Active' },
        { label: 'Expired', value: 'Expired' },
      ],
    },
    { field: 'actions', title: 'ACTION', type: 'action', width: '80px' },
  ];

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading.set(true);
    this.userService.getUsers().subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: (err: unknown) => {
        console.error('API Error:', err);
        this.users.set([]);
        this.isLoading.set(false);
      },
    });
  }

  onSearch(term: string) {
    this.searchTerm.set(term);
  }

  handleGridAction(event: { type: string; row: any }) {
    if (event.type === 'edit') {
      this.openUserForm(event.row);
    }
  }

  openQuickAddUser(): void {
    this.editingUserId.set(null);
    this.userForm.set({
      fullName: '',
      email: '',
      tier: 'Basic',
      status: 'Active',
    });
    this.isUserFormOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  openUserForm(row: any): void {
    this.editingUserId.set(row.id);
    this.userForm.set({
      fullName: String(row.name ?? ''),
      email: String(row.email ?? ''),
      tier: (row.plan ?? 'Basic') as UserGridTier,
      status: row.status === 'Expired' ? 'Review' : 'Active',
    });
    this.isUserFormOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeUserForm(): void {
    this.isUserFormOpen.set(false);
    this.editingUserId.set(null);
    document.body.style.overflow = 'auto';
  }

  updateUserFormField<K extends keyof UserFormState>(field: K, value: UserFormState[K]): void {
    this.userForm.update((current) => ({ ...current, [field]: value }));
  }

  submitUserForm(): void {
    const form = this.userForm();
    const fullName = form.fullName.trim();
    const email = form.email.trim();

    if (!fullName || !email) {
      return;
    }

    const editingId = this.editingUserId();
    if (editingId) {
      this.users.update((current) =>
        current.map((user) =>
          user.id === editingId
            ? {
                ...user,
                name: fullName,
                email,
                plan: form.tier,
                status: form.status === 'Active' ? 'Active' : 'Expired',
              }
            : user,
        ),
      );
    } else {
      const createdAt = new Date().toISOString();
      const nextUser = {
        id: `usr-${Date.now()}`,
        userName: email.split('@')[0] ?? `user-${Date.now()}`,
        name: fullName,
        email,
        emailConfirmed: form.status === 'Active',
        role: 'Athlete' as const,
        plan: form.tier,
        status: (form.status === 'Active' ? 'Active' : 'Expired') as 'Active' | 'Expired',
        subscriptionEndDate: null,
        profileImageUrl: '',
        firstName: fullName.split(' ')[0] ?? fullName,
        lastName: fullName.split(' ').slice(1).join(' '),
        isSelected: false,
      };

      this.users.update((current) => [nextUser, ...current]);
    }
    this.closeUserForm();
  }

  onDrawerSave(event: { row: any; payload: any }): void {
    const nextRow = this.mapApplicationUserToGridRow(
      (event.payload ?? event.row) as ApplicationUser,
    );
    const targetId = String(nextRow?.id ?? event.row?.id ?? '');

    if (!targetId) {
      return;
    }

    this.users.update((current) =>
      current.map((user) => (user.id === targetId ? { ...user, ...nextRow } : user)),
    );
    this.refreshGridData();
  }

  refreshGridData(): void {
    this.searchTerm.update((v) => v);
  }

  onDeleteUserRequest(userId: string): void {
    if (!userId) {
      return;
    }

    const confirmed = confirm(
      'Are you sure you want to permanently delete this user? This action cannot be undone.',
    );
    if (!confirmed) {
      return;
    }

    this.userService.deleteUser(userId).subscribe({
      next: () => {
        this.users.update((current) => current.filter((u) => u.id !== userId));
        this.refreshGridData();
        this.notificationService.showSuccess('User deleted successfully.');
        this.gridRef()?.closeDetailView();
      },
      error: () => {
        this.notificationService.showError('Failed to delete user. Please try again.');
      },
    });
  }

  private mapApplicationUserToGridRow(user: ApplicationUser): any {
    const fullName = `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim();

    return {
      ...user,
      name: fullName || user.userName,
      profileImageUrl: user.profilePictureUrl,
      status: user.isActive ? 'Active' : 'Banned',
    };
  }
}
