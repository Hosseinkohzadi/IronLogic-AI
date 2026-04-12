import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NotificationService } from '@core/services/notification.service';
import { AuthService } from '@core/services/auth.service';
import { AthleteProfile, UserService } from '@core/services/user.service';

/** Form-reset-safe snapshot: nullable numeric fields are coerced to number. */
type ProfileFormSnapshot = Omit<AthleteProfile, 'currentWeight' | 'height' | 'targetWeight'> & {
  currentWeight: number;
  height: number;
  targetWeight: number;
  aiSyncEnabled: boolean;
};

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly userProfile = signal<AthleteProfile | null>(null);
  readonly originalProfileFormValue = signal<ProfileFormSnapshot | null>(null);
  readonly profileError = signal<string | null>(null);
  readonly isFormDirty = signal(false);
  readonly isDeleteModalOpen = signal(false);
  readonly isDeleting = signal(false);

  readonly profileForm = this.fb.nonNullable.group({
    id: [''],
    userName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^\+?[0-9]{10,15}$/)]],
    profilePictureUrl: [''],
    roles: this.fb.nonNullable.control<string[]>([]),
    isActive: [true],
    language: ['English'],
    currentWeight: [0],
    height: [0],
    targetWeight: [0],
    activityLevel: ['Moderately Active', Validators.required],
    bio: [''],
    aiSyncEnabled: [false],
  });

  readonly fullName = computed(() => {
    const firstName = this.profileForm.controls.firstName.value.trim();
    const lastName = this.profileForm.controls.lastName.value.trim();
    return `${firstName} ${lastName}`.trim() || this.profileForm.controls.userName.value;
  });

  readonly showSaveFab = computed(
    () => this.isFormDirty() && !this.isSubmitting() && !this.isLoading(),
  );

  readonly avatarInitials = computed(() => {
    const firstName = this.profileForm.controls.firstName.value.trim();
    const lastName = this.profileForm.controls.lastName.value.trim();
    const userName = this.profileForm.controls.userName.value.trim();

    const firstInitial = firstName.charAt(0);
    const lastInitial = lastName.charAt(0);
    const combined = `${firstInitial}${lastInitial}`.toUpperCase();

    if (combined.trim()) {
      return combined;
    }

    return userName.slice(0, 2).toUpperCase() || 'IL';
  });

  readonly bmi = computed(() => {
    const heightCm = Number(this.profileForm.controls.height.value ?? 0);
    const weightKg = Number(this.profileForm.controls.currentWeight.value ?? 0);

    if (!heightCm || !weightKg || heightCm <= 0 || weightKg <= 0) {
      return null;
    }

    const heightMeters = heightCm / 100;
    const value = weightKg / (heightMeters * heightMeters);
    return Number(value.toFixed(1));
  });

  readonly bmiStatus = computed(() => {
    const bmiValue = this.bmi();
    if (bmiValue === null) {
      return {
        label: 'Pending',
        badgeClass: 'bg-slate-500/20 text-slate-200 border-slate-400/40',
        icon: 'info',
      };
    }

    if (bmiValue < 18.5) {
      return {
        label: 'Underweight',
        badgeClass: 'bg-sky-500/20 text-sky-200 border-sky-400/40',
        icon: 'info',
      };
    }

    if (bmiValue < 25) {
      return {
        label: 'Healthy',
        badgeClass: 'bg-emerald-500/20 text-emerald-200 border-emerald-400/40',
        icon: 'check-circle',
      };
    }

    if (bmiValue < 30) {
      return {
        label: 'Overweight',
        badgeClass: 'bg-amber-500/20 text-amber-200 border-amber-400/40',
        icon: 'alert-triangle',
      };
    }

    return {
      label: 'Obese',
      badgeClass: 'bg-rose-500/20 text-rose-200 border-rose-400/40',
      icon: 'x-circle',
    };
  });

  ngOnInit(): void {
    this.profileForm.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.isFormDirty.set(this.profileForm.dirty);
    });

    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.profileError.set(null);

    this.userService
      .getMyProfile()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (profile) => {
          this.userProfile.set(profile);
          const formValue = this.buildProfileFormValue(profile);
          this.originalProfileFormValue.set(formValue);
          this.profileForm.reset(formValue);
          this.profileForm.markAsPristine();
          this.isFormDirty.set(false);
        },
        error: () => {
          this.profileError.set('Unable to load profile data.');
          this.notificationService.showError('Failed to load profile data.');
        },
      });
  }

  saveChanges(): void {
    if (this.profileForm.invalid || !this.profileForm.dirty || this.isSubmitting()) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const { aiSyncEnabled: _aiSyncEnabled, ...payload } =
      this.profileForm.getRawValue() as AthleteProfile & {
        aiSyncEnabled: boolean;
      };

    this.isSubmitting.set(true);
    this.profileError.set(null);

    this.userService
      .updateMyProfile(payload)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (updatedProfile) => {
          this.userProfile.set(updatedProfile);
          const formValue = this.buildProfileFormValue(updatedProfile);
          this.originalProfileFormValue.set(formValue);
          this.profileForm.reset(formValue);
          this.profileForm.markAsPristine();
          this.isFormDirty.set(false);
          this.notificationService.showSuccess('Profile updated successfully!');
        },
        error: () => {
          this.profileError.set('Unable to save changes.');
          this.notificationService.showError('Failed to update profile.');
        },
      });
  }

  cancelChanges(): void {
    const originalValue = this.originalProfileFormValue();
    if (!originalValue) {
      this.profileForm.reset();
      this.profileForm.markAsPristine();
      this.isFormDirty.set(false);
      return;
    }

    this.profileForm.reset(originalValue);
    this.profileForm.markAsPristine();
    this.isFormDirty.set(false);
  }

  openDeleteModal(): void {
    this.isDeleteModalOpen.set(true);
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen.set(false);
  }

  confirmDeleteAccount(): void {
    if (this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.profileError.set(null);

    this.userService
      .deleteMyProfile()
      .pipe(finalize(() => this.isDeleting.set(false)))
      .subscribe({
        next: () => {
          this.isDeleteModalOpen.set(false);
          this.notificationService.showSuccess('Your account has been deleted.');
          this.authService.logout();
        },
        error: () => {
          this.profileError.set('Unable to delete account.');
          this.notificationService.showError('Failed to delete account. Please try again.');
        },
      });
  }

  openAvatarPicker(): void {
    // Placeholder for upload flow wiring (file picker or media service).
    this.notificationService.showSuccess('Avatar upload flow will open here.');
  }

  private buildProfileFormValue(profile: AthleteProfile): ProfileFormSnapshot {
    return {
      ...profile,
      currentWeight: profile.currentWeight ?? 0,
      height: profile.height ?? 0,
      targetWeight: profile.targetWeight ?? 0,
      bio: profile.bio ?? '',
      activityLevel: profile.activityLevel ?? 'Moderately Active',
      language: profile.language ?? 'English',
      aiSyncEnabled: this.profileForm.controls.aiSyncEnabled.value,
    };
  }
}
