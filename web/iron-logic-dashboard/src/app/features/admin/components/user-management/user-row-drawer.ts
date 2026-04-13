import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { CommonModule, DatePipe, UpperCasePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { UserFormComponent } from './user-form.component';
import { ApplicationUser } from '@core/models';
import { finalize } from 'rxjs/operators';
import {
  CommunicationService,
  EmailDeliveryStatus,
  UserEmailHistoryItem,
} from '@core/services/communication.service';
import { NotificationService } from '@core/services/notification.service';

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
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LucideAngularModule,
    DatePipe,
    UpperCasePipe,
    UserFormComponent,
  ],
  templateUrl: './user-row-drawer.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserRowDrawerComponent {
  row = input<unknown>(null);

  saved = output<ApplicationUser>();
  close = output<void>();
  deleteUser = output<string>();

  private readonly fb = inject(FormBuilder);
  private readonly communicationService = inject(CommunicationService);
  private readonly notificationService = inject(NotificationService);

  readonly activeTab = signal<'profile' | 'billing' | 'emails'>('profile');
  readonly isLoadingEmailHistory = signal(false);
  readonly isSendingEmail = signal(false);
  readonly emailHistory = signal<UserEmailHistoryItem[]>([]);

  readonly composeForm = this.fb.nonNullable.group({
    subject: ['', [Validators.required, Validators.minLength(3)]],
    message: ['', [Validators.required, Validators.minLength(10)]],
  });

  readonly isSendDisabled = computed(
    () => this.composeForm.invalid || this.isSendingEmail() || !this.user(),
  );

  readonly user = computed<UserRow | null>(() => {
    const source = this.row();
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

  constructor() {
    effect(() => {
      const currentUser = this.user();
      if (!currentUser?.id) {
        this.emailHistory.set([]);
        this.composeForm.reset({ subject: '', message: '' });
        this.composeForm.markAsPristine();
        return;
      }

      if (this.activeTab() === 'emails') {
        this.loadEmailHistory(currentUser.id);
      }
    });
  }

  setTab(tab: 'profile' | 'billing' | 'emails'): void {
    this.activeTab.set(tab);

    const currentUser = this.user();
    if (tab === 'emails' && currentUser?.id) {
      this.loadEmailHistory(currentUser.id);
    }
  }

  onDeleteClick(): void {
    const currentUser = this.user();
    if (!currentUser) {
      return;
    }

    this.deleteUser.emit(currentUser.id);
  }

  sendManualMessage(): void {
    const currentUser = this.user();
    if (!currentUser || this.composeForm.invalid || this.isSendingEmail()) {
      this.composeForm.markAllAsTouched();
      return;
    }

    const payload = this.composeForm.getRawValue();
    this.isSendingEmail.set(true);

    this.communicationService
      .sendEmail(currentUser.id, payload.subject, payload.message)
      .pipe(finalize(() => this.isSendingEmail.set(false)))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Email queued for sending');
          this.composeForm.reset({ subject: '', message: '' });
          this.composeForm.markAsPristine();
          this.loadEmailHistory(currentUser.id);
        },
        error: () => {
          this.notificationService.showError('Failed to send email.');
        },
      });
  }

  trackHistoryItem(_index: number, item: UserEmailHistoryItem): string {
    return item.id;
  }

  emailStatusClass(status: EmailDeliveryStatus): string {
    return this.normalizeEmailStatus(status) === 'Sent' ? 'text-emerald-400' : 'text-red-400';
  }

  private loadEmailHistory(userId: string): void {
    this.isLoadingEmailHistory.set(true);

    this.communicationService
      .getHistory(userId)
      .pipe(finalize(() => this.isLoadingEmailHistory.set(false)))
      .subscribe({
        next: (items) => {
          this.emailHistory.set(items);
        },
        error: () => {
          this.emailHistory.set([]);
          this.notificationService.showError('Failed to load email history.');
        },
      });
  }

  private normalizeEmailStatus(status: EmailDeliveryStatus): 'Sent' | 'Failed' {
    return status === 'Sent' ? 'Sent' : 'Failed';
  }
}
