import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { CommonModule, DatePipe, UpperCasePipe } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { UserFormComponent } from './user-form.component';
import { ApplicationUser } from '@core/models';

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
  imports: [CommonModule, LucideAngularModule, DatePipe, UpperCasePipe, UserFormComponent],
  templateUrl: './user-row-drawer.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserRowDrawerComponent {
  row = input<unknown>(null);

  saved = output<ApplicationUser>();
  close = output<void>();

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
}
