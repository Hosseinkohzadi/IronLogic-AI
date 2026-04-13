import {
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { NotificationService } from '@core/services/notification.service';
import { AuthService } from '@core/services/auth.service';
import { AthleteProfile, UserService } from '@core/services/user.service';

/** Maps backend enum values to frontend display strings */
const ActivityLevelReverseMap: Record<number, string> = {
  0: 'None',
  1: 'Sedentary',
  2: 'Lightly Active',
  3: 'Moderately Active',
  4: 'Very Active',
};

const ActivityLevelMap: Record<string, number> = {
  None: 0,
  Sedentary: 1,
  'Lightly Active': 2,
  'Moderately Active': 3,
  'Very Active': 4,
};

const GenderMap: Record<string, number> = {
  Unknown: 0,
  Male: 1,
  Female: 2,
  Other: 3,
};

const GenderReverseMap: Record<number, string> = {
  0: 'Unknown',
  1: 'Male',
  2: 'Female',
  3: 'Other',
};

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent implements OnInit {
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly userService = inject(UserService);
  private readonly notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly profileError = signal<string | null>(null);
  readonly userProfile = signal<any>(null);

  readonly profileForm = this.fb.nonNullable.group({
    id: [''],
    userName: [''],
    email: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    phoneNumber: ['', [Validators.pattern(/^\+?[0-9]{10,15}$/)]],
    gender: ['Unknown'],
    profilePictureUrl: [''],
    currentWeight: [0],
    height: [0],
    targetWeight: [0],
    activityLevel: ['Moderately Active'],
    bio: [''],
  });

  // Computed display values for UI
  readonly fullName = computed(() => {
    const f = this.profileForm.controls.firstName.value.trim();
    const l = this.profileForm.controls.lastName.value.trim();
    return `${f} ${l}`.trim() || this.profileForm.controls.userName.value || 'Iron Athlete';
  });

  readonly avatarInitials = computed(() => {
    const f = this.profileForm.controls.firstName.value.charAt(0);
    const l = this.profileForm.controls.lastName.value.charAt(0);
    return (f + l).toUpperCase() || 'IL';
  });

  readonly bmi = computed(() => {
    const heightCm = Number(this.profileForm.controls.height.value ?? 0);
    const weightKg = Number(this.profileForm.controls.currentWeight.value ?? 0);
    if (!heightCm || !weightKg || heightCm <= 0 || weightKg <= 0) return null;
    const h = heightCm / 100;
    return Number((weightKg / (h * h)).toFixed(1));
  });

  readonly bmiStatus = computed(() => {
    const v = this.bmi();
    if (v === null)
      return { label: 'Pending', badgeClass: 'bg-slate-500/20 text-slate-200 border-slate-400/40' };
    if (v < 18.5)
      return { label: 'Underweight', badgeClass: 'bg-sky-500/20 text-sky-200 border-sky-400/40' };
    if (v < 25)
      return {
        label: 'Healthy',
        badgeClass: 'bg-emerald-500/20 text-emerald-200 border-emerald-400/40',
      };
    if (v < 30)
      return {
        label: 'Overweight',
        badgeClass: 'bg-amber-500/20 text-amber-200 border-amber-400/40',
      };
    return { label: 'Obese', badgeClass: 'bg-rose-500/20 text-rose-200 border-rose-400/40' };
  });

  ngOnInit(): void {
    this.profileForm.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.cdr.markForCheck();
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
        next: (profile: any) => {
          console.log('%c✅ Raw API Response:', 'color:#10b981;font-weight:700;', profile);
          const data = this.buildProfileFormValue(profile);
          console.log('%c📊 Mapped Data:', 'color:#f59e0b;font-weight:700;', data);

          this.userProfile.set(profile);

          // Reset form with new data and force UI update
          this.profileForm.reset(data);
          this.cdr.detectChanges();

          this.profileForm.markAsPristine();
        },
        error: () => {
          this.profileError.set('Failed to load profile data');
          this.notificationService.showError('Failed to load profile data');
        },
      });
  }

  saveChanges(): void {
    if (this.profileForm.invalid) {
      const controls = this.profileForm.controls;
      const errors = Object.keys(controls)
        .map((key) => {
          const control = controls[key as keyof typeof controls];
          return control.errors ? { field: key, errors: control.errors } : null;
        })
        .filter((value) => value !== null);

      console.log('%c[Save Validation] Form Errors:', 'color: #ef4444; font-weight: bold;', errors);
      this.profileForm.markAllAsTouched();
      return;
    }

    if (!this.profileForm.dirty) return;

    this.isSubmitting.set(true);
    this.profileError.set(null);
    const formValue = this.profileForm.getRawValue();
    console.log('%c[Save Step 1] Form Raw Value:', 'color: #3b82f6;', formValue);

    const profilePictureUrl = formValue.profilePictureUrl?.trim()
      ? formValue.profilePictureUrl.trim()
      : null;

    const payload = {
      userId: formValue.id,
      email: formValue.email,
      name: formValue.userName,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      phoneNumber: formValue.phoneNumber,
      profilePictureUrl,
      height: formValue.height,
      currentWeight: formValue.currentWeight,
      targetWeight: formValue.targetWeight,
      gender: GenderMap[formValue.gender] || 0,
      activityLevel: ActivityLevelMap[formValue.activityLevel] || 0,
      bio: formValue.bio,
    };

    console.log('%c[Save Step 2] Final Payload (Unwrapped):', 'color: #10b981; font-weight: bold;', payload);

    this.userService
      .updateMyProfile(payload as any)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Profile saved successfully in database');
          this.profileForm.markAsPristine();
          this.loadProfile();
        },
        error: (err) => {
          console.error('Save Error:', err);
          const msg = err.error?.message || 'Failed to save profile';
          this.profileError.set(msg);
          this.notificationService.showError(msg);
        },
      });
  }

  cancelChanges(): void {
    this.profileForm.reset(this.buildProfileFormValue(this.userProfile() ?? {}));
    this.profileForm.markAsPristine();
  }

  private buildProfileFormValue(profile: any) {
    const activityCode = Number(profile.activityLevel);
    const genderCode = Number(profile.gender);
    return {
      id: profile.userId || profile.id || '',
      userName: profile.name || profile.userName || '',
      email: profile.email || '',
      firstName: profile.firstName || '',
      lastName: profile.lastName || '',
      phoneNumber: profile.phoneNumber || '',
      gender: GenderReverseMap[genderCode] || 'Unknown',
      profilePictureUrl: profile.profilePictureUrl || '',
      currentWeight: profile.currentWeight ?? 0,
      height: profile.height ?? 0,
      targetWeight: profile.targetWeight ?? 0,
      activityLevel:
        activityCode && ActivityLevelReverseMap[activityCode]
          ? ActivityLevelReverseMap[activityCode]
          : typeof profile.activityLevel === 'string' && profile.activityLevel
            ? profile.activityLevel
            : 'Moderately Active',
      bio: profile.bio || '',
    };
  }

  openAvatarPicker(): void {
    const url = window.prompt('Enter profile picture URL:');
    if (url) {
      this.profileForm.controls.profilePictureUrl.setValue(url);
      this.profileForm.markAsDirty();
    }
  }
}
