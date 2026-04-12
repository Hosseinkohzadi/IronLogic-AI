import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import { CommonModule, DatePipe, UpperCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';

type UserTier = 'Basic' | 'Pro' | 'Elite';
type UserStatus = 'Active' | 'Review' | 'Banned';

interface UserRow {
  id: string;
  name: string;
  email: string;
  tier: UserTier;
  status: UserStatus;
  sessions: number;
  dailyWeights: number;
  lastLogin: string;
  accessFailedCount?: number;
  lockoutEnd?: string | null;
  profileImageUrl?: string;
}

interface UserEditorState {
  name: string;
  email: string;
  tier: UserTier;
  status: UserStatus;
}

@Component({
  selector: 'app-user-row-drawer',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, DatePipe, UpperCasePipe],
  templateUrl: './user-row-drawer.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserRowDrawerComponent {
  row = input<unknown>(null);

  save = output<UserRow>();
  close = output<void>();

  readonly editor = signal<UserEditorState>({
    name: '',
    email: '',
    tier: 'Basic',
    status: 'Active',
  });

  readonly user = computed<UserRow | null>(() => {
    const source = this.row();
    if (!source || typeof source !== 'object') {
      return null;
    }

    const typed = source as Partial<UserRow>;
    if (!typed.id || !typed.name || !typed.email) {
      return null;
    }

    return {
      id: typed.id,
      name: typed.name,
      email: typed.email,
      tier: typed.tier ?? 'Basic',
      status: typed.status ?? 'Active',
      sessions: Number(typed.sessions ?? 0),
      dailyWeights: Number(typed.dailyWeights ?? 0),
      lastLogin: typed.lastLogin ?? '',
      accessFailedCount: Number(typed.accessFailedCount ?? 0),
      lockoutEnd: typed.lockoutEnd ?? null,
      profileImageUrl: typed.profileImageUrl,
    };
  });

  constructor() {
    effect(() => {
      const current = this.user();
      if (!current) {
        return;
      }

      this.editor.set({
        name: current.name,
        email: current.email,
        tier: current.tier,
        status: current.status,
      });
    });
  }

  updateField<K extends keyof UserEditorState>(key: K, value: UserEditorState[K]): void {
    this.editor.update((state) => ({ ...state, [key]: value }));
  }

  onSave(): void {
    const current = this.user();
    if (!current) {
      return;
    }

    const draft = this.editor();
    this.save.emit({
      ...current,
      name: draft.name.trim(),
      email: draft.email.trim(),
      tier: draft.tier,
      status: draft.status,
    });
  }
}
