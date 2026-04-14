import {
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { ApplicationUser } from '@core/models';
import { finalize } from 'rxjs/operators';
import { CommunicationService, UserEmailHistoryItem } from '@core/services/communication.service';
import { NotificationService } from '@core/services/notification.service';
import { AthleteProfile, UserDetailResponse, UserService } from '@core/services/user.service';

type UserTier = 'Basic' | 'Pro' | 'Elite';
type UserStatus = 'Active' | 'Review' | 'Banned';

interface UserRow {
  id: string;
  userName?: string;
  firstName?: string;
  lastName?: string;
  name: string;
  email: string;
  phoneNumber?: string;
  tier: UserTier;
  status: UserStatus;
  sessions: number;
  dailyWeights: number;
  lastLogin: string;
  accessFailedCount?: number;
  lockoutEnd?: string | null;
  profileImageUrl?: string;
  profilePictureUrl?: string;
  roles?: string[];
  isActive?: boolean;
}

@Component({
  selector: 'app-user-row-drawer',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, DatePipe],
  templateUrl: './user-row-drawer.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserRowDrawerComponent {
  row = input<unknown>(null);
  record = input<unknown>(null);
  data = input<unknown>(null);

  saved = output<ApplicationUser>();
  close = output<void>();
  closeDrawer = output<void>();
  deleteUser = output<string>();
  editRecord = output<UserRow | null>();

  private readonly communicationService = inject(CommunicationService);
  private readonly notificationService = inject(NotificationService);
  private readonly userService = inject(UserService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly isLoadingEmailHistory = signal(false);
  readonly emailHistory = signal<UserEmailHistoryItem[]>([]);
  readonly userDetail = signal<UserDetailResponse | null>(null);
  readonly isProfilePreviewExpanded = signal(false);

  readonly user = computed<UserRow | null>(() => {
    const source = this.record() ?? this.data() ?? this.row();
    if (!source || typeof source !== 'object') {
      return null;
    }

    const typed = source as Partial<UserRow>;
    if (!typed.id || !typed.name || !typed.email) {
      return null;
    }

    const [derivedFirstName = '', ...lastParts] = typed.name.split(' ');
    const derivedLastName = lastParts.join(' ').trim();

    return {
      id: typed.id,
      userName: typed.userName ?? typed.email.split('@')[0] ?? '',
      firstName: typed.firstName ?? derivedFirstName,
      lastName: typed.lastName ?? derivedLastName,
      name: typed.name,
      email: typed.email,
      phoneNumber: typed.phoneNumber ?? '',
      tier: typed.tier ?? 'Basic',
      status: typed.status ?? 'Active',
      sessions: Number(typed.sessions ?? 0),
      dailyWeights: Number(typed.dailyWeights ?? 0),
      lastLogin: typed.lastLogin ?? '',
      accessFailedCount: Number(typed.accessFailedCount ?? 0),
      lockoutEnd: typed.lockoutEnd ?? null,
      profileImageUrl: typed.profileImageUrl ?? typed.profilePictureUrl,
      profilePictureUrl: typed.profilePictureUrl ?? typed.profileImageUrl,
      roles: Array.isArray(typed.roles) && typed.roles.length > 0 ? typed.roles : ['User'],
      isActive: typed.isActive ?? typed.status !== 'Banned',
    };
  });

  readonly detailRecordId = computed(() => this.user()?.id ?? null);
  readonly isLockedOut = computed(() => {
    const lockoutEnd = this.userDetail()?.lockoutEnd;
    return !!lockoutEnd && new Date(lockoutEnd).getTime() > Date.now();
  });
  readonly avatarUrl = computed(() => {
    const details = this.user();
    return (
      this.userDetail()?.profilePictureUrl ??
      details?.profilePictureUrl ??
      details?.profileImageUrl ??
      null
    );
  });
  private readonly linkedEntityRouteMap: Readonly<Record<string, string>> = {
    '/admin/workouts': '/admin/sessions',
    '/admin/weights': '/admin/sessions',
    '/admin/billing': '/admin/billing',
    '/admin/communications': '/admin/users',
  };

  get avatarInitials(): string {
    const currentUser = this.user();
    if (!currentUser) {
      return 'USR';
    }

    const firstSource = currentUser.firstName?.trim() || currentUser.name?.trim() || '';
    const lastSource = currentUser.lastName?.trim() || '';
    const firstInitial = firstSource.charAt(0).toUpperCase();
    const lastInitial = lastSource.charAt(0).toUpperCase();
    const initials = `${firstInitial}${lastInitial}`.trim();

    return initials || 'USR';
  }

  constructor() {
    effect(() => {
      const currentUserId = this.detailRecordId();
      if (currentUserId) {
        this.loadUserDetail(currentUserId);
        this.loadEmailHistory(currentUserId);
      } else {
        this.userDetail.set(null);
        this.emailHistory.set([]);
        this.triggerChangeDetection();
      }
    });
  }

  onDeleteClick(): void {
    const currentUser = this.user();
    if (!currentUser) {
      return;
    }

    this.deleteUser.emit(currentUser.id);
  }

  onEditClick(): void {
    const currentUser = this.user();
    if (!currentUser) {
      return;
    }

    this.editRecord.emit(currentUser);
    this.closeDrawer.emit();
  }

  toggleProfilePreview(): void {
    this.isProfilePreviewExpanded.update((value) => !value);
  }

  navigateToEntity(path: string): void {
    const userId = this.detailRecordId();
    if (!userId) {
      return;
    }

    const targetPath = this.linkedEntityRouteMap[path] ?? path;

    this.close.emit();
    this.closeDrawer.emit();

    void this.router.navigate([targetPath], {
      queryParams: { userId },
    });
  }

  private loadUserDetail(userId: string): void {
    this.isLoading.set(true);

    this.userService
      .getUserById(userId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (user) => {
          this.userService.getMyProfile(userId).subscribe({
            next: (profile) => {
              this.userDetail.set(this.mergeProfileDetail(user, profile));
              this.triggerChangeDetection();
            },
            error: () => {
              this.userDetail.set(user);
              this.triggerChangeDetection();
            },
          });
          this.triggerChangeDetection();
        },
        error: () => {
          this.userService.getMyProfile(userId).subscribe({
            next: (profile) => {
              this.userDetail.set(this.createDetailFromProfile(userId, profile));
              this.triggerChangeDetection();
            },
            error: () => {
              this.userDetail.set(null);
              this.triggerChangeDetection();
              this.notificationService.showError('Failed to load user details.');
            },
          });
        },
      });
  }

  private mergeProfileDetail(user: UserDetailResponse, profile: AthleteProfile): UserDetailResponse {
    return {
      ...user,
      id: String(profile.userId ?? user.id ?? ''),
      userName: String(profile.name ?? profile.userName ?? user.userName ?? ''),
      email: String(profile.email ?? user.email ?? ''),
      firstName: String(profile.firstName ?? user.firstName ?? ''),
      lastName: String(profile.lastName ?? user.lastName ?? ''),
      phoneNumber: String(profile.phoneNumber ?? user.phoneNumber ?? ''),
      profilePictureUrl: String(profile.profilePictureUrl ?? user.profilePictureUrl ?? ''),
      gender: this.normalizeGender(profile.gender),
      currentWeight: Number(profile.currentWeight ?? user.currentWeight ?? 0),
      height: Number(profile.height ?? user.height ?? 0),
      targetWeight: Number(profile.targetWeight ?? user.targetWeight ?? 0),
      activityLevel: this.normalizeActivityLevel(profile.activityLevel),
      bio: String(profile.bio ?? user.bio ?? ''),
    };
  }

  private createDetailFromProfile(userId: string, profile: AthleteProfile): UserDetailResponse {
    return {
      id: String(profile.userId ?? userId),
      userName: String(profile.name ?? profile.userName ?? profile.email?.split('@')[0] ?? ''),
      email: String(profile.email ?? ''),
      firstName: String(profile.firstName ?? ''),
      lastName: String(profile.lastName ?? ''),
      phoneNumber: String(profile.phoneNumber ?? ''),
      profilePictureUrl: String(profile.profilePictureUrl ?? ''),
      roles: [],
      isActive: true,
      lastLoginDate: null,
      failedLoginAttempts: 0,
      lockoutEnd: null,
      lockoutEnabled: false,
      emailConfirmed: false,
      phoneNumberConfirmed: false,
      twoFactorEnabled: false,
      gender: this.normalizeGender(profile.gender),
      currentWeight: Number(profile.currentWeight ?? 0),
      height: Number(profile.height ?? 0),
      targetWeight: Number(profile.targetWeight ?? 0),
      activityLevel: this.normalizeActivityLevel(profile.activityLevel),
      bio: String(profile.bio ?? ''),
    };
  }

  private normalizeGender(value: string | number | null | undefined): string {
    if (typeof value === 'string' && value.trim()) {
      return value;
    }

    const code = Number(value ?? 0);
    if (code === 1) {
      return 'Male';
    }

    if (code === 2) {
      return 'Female';
    }

    if (code === 3) {
      return 'Other';
    }

    return 'Unknown';
  }

  private normalizeActivityLevel(value: string | number | null | undefined): string {
    if (typeof value === 'string' && value.trim()) {
      return value;
    }

    const code = Number(value ?? 3);
    if (code === 0) {
      return 'None';
    }

    if (code === 1) {
      return 'Sedentary';
    }

    if (code === 2) {
      return 'Lightly Active';
    }

    if (code === 4) {
      return 'Very Active';
    }

    return 'Moderately Active';
  }

  private loadEmailHistory(userId: string): void {
    this.isLoadingEmailHistory.set(true);

    this.communicationService
      .getHistory(userId)
      .pipe(finalize(() => this.isLoadingEmailHistory.set(false)))
      .subscribe({
        next: (items) => {
          this.emailHistory.set(items);
          this.triggerChangeDetection();
        },
        error: () => {
          this.emailHistory.set([]);
          this.triggerChangeDetection();
          this.notificationService.showError('Failed to load email history.');
        },
      });
  }

  private triggerChangeDetection(): void {
    queueMicrotask(() => {
      this.cdr.markForCheck();
      this.cdr.detectChanges();
    });
  }
}
