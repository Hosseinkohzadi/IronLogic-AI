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
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { LucideAngularModule } from 'lucide-angular';
import { NotificationService } from '@core/services/notification.service';
import { UserService } from '@core/services/user.service';
import { ApplicationUser } from '@core/models';

type RoleOption = 'Admin' | 'User' | 'Athlete';

const PHONE_PATTERN = /^\+?[0-9]{10,15}$/;

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './user-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserFormComponent {
  userId = input<string | null>(null);
  userPreview = input<Partial<ApplicationUser> | null>(null);

  saved = output<ApplicationUser>();
  cancel = output<void>();

  private readonly userService = inject(UserService);
  private readonly notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly currentUser = signal<ApplicationUser | null>(null);
  readonly isLoadingUser = signal(false);
  readonly isSubmitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly availableRoles: RoleOption[] = ['Admin', 'User', 'Athlete'];

  readonly userForm = this.fb.nonNullable.group({
    userName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    phoneNumber: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
    profilePictureUrl: [''],
    roles: this.fb.nonNullable.control<string[]>(['User'], [Validators.required]),
    isActive: [true, Validators.required],
  });

  readonly isSaveDisabled = computed(
    () => this.userForm.invalid || this.userForm.pristine || this.isSubmitting(),
  );

  constructor() {
    effect(() => {
      const id = this.userId();
      if (!id) {
        return;
      }

      this.loadUser(id);
    });

    effect(() => {
      const preview = this.userPreview();
      if (!preview) {
        return;
      }

      this.patchUserForm(preview);
    });
  }

  onSubmit(): void {
    const id = this.userId();
    if (!id || this.userForm.invalid || this.userForm.pristine || this.isSubmitting()) {
      return;
    }

    this.formError.set(null);
    this.isSubmitting.set(true);

    const values = this.userForm.getRawValue();
    const payload: ApplicationUser = {
      id,
      userName: values.userName.trim(),
      email: values.email.trim(),
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      phoneNumber: values.phoneNumber.trim(),
      profilePictureUrl: values.profilePictureUrl.trim(),
      roles: values.roles,
      isActive: values.isActive,
    };

    this.userService
      .updateUser(id, payload)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (updatedUser) => {
          this.currentUser.set(updatedUser);
          this.patchUserForm(updatedUser);
          this.userForm.markAsPristine();
          this.notificationService.showSuccess('User profile updated!');
          this.saved.emit(updatedUser);
        },
        error: (error: unknown) => {
          const message = this.extractErrorMessage(error);
          this.formError.set(message);
          this.notificationService.showError('Failed to update user.');
        },
      });
  }

  private loadUser(id: string): void {
    this.isLoadingUser.set(true);
    this.formError.set(null);

    this.userService
      .getUserById(id)
      .pipe(finalize(() => this.isLoadingUser.set(false)))
      .subscribe({
        next: (user) => {
          this.currentUser.set(user);
          this.patchUserForm(user);
          this.userForm.markAsPristine();
        },
        error: (error: unknown) => {
          this.formError.set(this.extractErrorMessage(error));
        },
      });
  }

  toggleRole(role: RoleOption): void {
    const nextRoles = new Set(this.userForm.controls.roles.value);
    if (nextRoles.has(role)) {
      nextRoles.delete(role);
    } else {
      nextRoles.add(role);
    }

    const values = Array.from(nextRoles);
    this.userForm.controls.roles.setValue(values.length > 0 ? values : ['User']);
    this.userForm.controls.roles.markAsDirty();
  }

  hasRole(role: RoleOption): boolean {
    return this.userForm.controls.roles.value.includes(role);
  }

  private patchUserForm(user: Partial<ApplicationUser>): void {
    this.userForm.patchValue({
      userName: user.userName ?? '',
      email: user.email ?? '',
      firstName: user.firstName ?? '',
      lastName: user.lastName ?? '',
      phoneNumber: user.phoneNumber ?? '',
      profilePictureUrl: user.profilePictureUrl ?? '',
      roles: Array.isArray(user.roles) && user.roles.length > 0 ? user.roles : ['User'],
      isActive: user.isActive ?? true,
    });
  }

  private extractErrorMessage(error: unknown): string {
    if (typeof error !== 'object' || error === null) {
      return 'Failed to update user.';
    }

    const err = error as {
      error?: { message?: string; title?: string; detail?: string };
      message?: string;
    };

    return (
      err.error?.message ??
      err.error?.title ??
      err.error?.detail ??
      err.message ??
      'Failed to update user.'
    );
  }
}
