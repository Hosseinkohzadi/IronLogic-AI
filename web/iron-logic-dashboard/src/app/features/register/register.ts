import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService, RegisterRequest } from '@core/services/auth.service';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-register',
  imports: [CommonModule, RouterLink, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  readonly isLoading = signal(false);
  readonly errorMessages = signal<string[]>([]);
  readonly successMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);

  readonly registerForm = this.formBuilder.nonNullable.group({
    fullName: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.minLength(2),
    ]),
    email: this.formBuilder.nonNullable.control('', [Validators.required, Validators.email]),
    password: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.minLength(8),
    ]),
    confirmPassword: this.formBuilder.nonNullable.control('', [Validators.required]),
  });

  onSubmit(): void {
    this.errorMessages.set([]);
    this.successMessage.set(null);

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.errorMessages.set(['Please complete all required fields with valid values.']);
      return;
    }

    const formValue = this.registerForm.getRawValue();
    if (formValue.password !== formValue.confirmPassword) {
      this.errorMessages.set(['Password and confirm password must match.']);
      return;
    }

    const payload: RegisterRequest = {
      email: formValue.email,
      password: formValue.password,
      confirmPassword: formValue.confirmPassword,
      fullName: formValue.fullName,
    };

    this.isLoading.set(true);
    this.authService
      .register(payload)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          const didNavigate = this.authService.handleAuthSuccess(response);
          if (didNavigate) {
            return;
          }

          // Store email so VerifyEmailComponent and the pendingEmailGuard can access it.
          sessionStorage.setItem('ironlogic.pending.email', formValue.email);

          this.successMessage.set(
            'Registration successful. Check your email for a verification code.',
          );
          this.registerForm.reset({
            fullName: '',
            email: '',
            password: '',
            confirmPassword: '',
          });
          setTimeout(() => {
            void this.router.navigateByUrl('/auth/verify-email');
          }, 900);
        },
        error: (error: unknown) => {
          this.errorMessages.set(this.extractErrorMessages(error));
        },
      });
  }

  togglePasswordVisibility(): void {
    this.showPassword.update((v) => !v);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update((v) => !v);
  }

  private extractErrorMessages(error: unknown): string[] {
    const fallback = ['Registration failed. Please try again.'];
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    const payload = error.error as unknown;
    if (!payload) {
      return fallback;
    }

    if (typeof payload === 'string' && payload.trim()) {
      return [payload];
    }

    if (typeof payload === 'object' && payload !== null) {
      const candidate = payload as {
        message?: string;
        title?: string;
        errors?: Record<string, string[] | string>;
      };

      const validationMessages = candidate.errors
        ? Object.values(candidate.errors)
            .flatMap((value) => (Array.isArray(value) ? value : [value]))
            .filter(
              (value): value is string => typeof value === 'string' && value.trim().length > 0,
            )
        : [];

      if (validationMessages.length > 0) {
        return validationMessages;
      }

      if (candidate.message && candidate.message.trim()) {
        return [candidate.message];
      }

      if (candidate.title && candidate.title.trim()) {
        return [candidate.title];
      }
    }

    return fallback;
  }
}
