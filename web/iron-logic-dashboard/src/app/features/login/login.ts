import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '@core/services/auth.service';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-login',
  imports: [RouterLink, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './login.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  readonly loginForm = this.formBuilder.nonNullable.group({
    email: this.formBuilder.nonNullable.control('', [Validators.required]),
    password: this.formBuilder.nonNullable.control('', [Validators.required]),
  });

  loginWithForm(): void {
    this.errorMessage.set(null);

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.errorMessage.set('Please enter your email and password.');
      return;
    }

    const formValue = this.loginForm.getRawValue();

    this.isLoading.set(true);
    this.authService
      .login({
        email: formValue.email,
        password: formValue.password,
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          const didNavigate = this.authService.handleAuthSuccess(response);
          if (!didNavigate) {
            this.errorMessage.set('Unable to determine user role from login response.');
          }
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.extractErrorMessage(error));
        },
      });
  }

  loginAsSuperAdmin(): void {
    this.authService.loginAsSuperAdmin();
  }

  loginAsAthleteHossein(): void {
    this.authService.loginAsAthlete('athlete-hossein-001');
  }

  togglePasswordVisibility(): void {
    this.showPassword.update((v) => !v);
  }

  private extractErrorMessage(error: unknown): string {
    const fallback = 'Login failed. Please check your credentials and try again.';

    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    const payload = error.error as unknown;
    if (!payload) {
      return fallback;
    }

    if (typeof payload === 'string' && payload.trim()) {
      return payload;
    }

    if (typeof payload === 'object' && payload !== null) {
      const candidate = payload as { message?: string; title?: string };
      if (candidate.message && candidate.message.trim()) {
        return candidate.message;
      }
      if (candidate.title && candidate.title.trim()) {
        return candidate.title;
      }
    }

    return fallback;
  }
}
