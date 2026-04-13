import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnDestroy,
  QueryList,
  ViewChildren,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { LucideAngularModule } from 'lucide-angular';
import { finalize } from 'rxjs/operators';
import { environment } from '@env/environment';
import { NotificationService } from '@core/services/notification.service';

const RESEND_COUNTDOWN_SEC = 60;
const DIGIT_COUNT = 6;
const REDIRECT_DELAY_MS = 2000;

@Component({
  selector: 'app-verify-email',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerifyEmailComponent implements AfterViewInit, OnDestroy {
  @ViewChildren('digitInput') digitInputs!: QueryList<ElementRef<HTMLInputElement>>;

  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);

  readonly digits = signal<string[]>(Array(DIGIT_COUNT).fill(''));
  readonly verificationCode = signal<string>('');
  readonly isVerifying = signal(false);
  readonly isVerified = signal(false);
  readonly verifyError = signal<string | null>(null);
  readonly resendCountdown = signal(0);
  readonly isResending = signal(false);

  private resendTimer: ReturnType<typeof setInterval> | null = null;
  private redirectTimer: ReturnType<typeof setTimeout> | null = null;

  readonly isCodeComplete = computed(() => this.verificationCode().length === DIGIT_COUNT);
  readonly resendCountdownText = computed(() => {
    const seconds = this.resendCountdown();
    const minutePart = Math.floor(seconds / 60)
      .toString()
      .padStart(2, '0');
    const secondPart = (seconds % 60).toString().padStart(2, '0');
    return `${minutePart}:${secondPart}`;
  });

  readonly pendingEmail = signal<string | null>(
    typeof sessionStorage !== 'undefined'
      ? sessionStorage.getItem('ironlogic.pending.email')
      : null,
  );

  ngAfterViewInit(): void {
    this.focusDigit(0);
    this.startResendCountdown();
  }

  ngOnDestroy(): void {
    if (this.resendTimer !== null) {
      clearInterval(this.resendTimer);
    }
    if (this.redirectTimer !== null) {
      clearTimeout(this.redirectTimer);
    }
  }

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const raw = input.value.replace(/\D/g, '');

    // Handle paste of full code into first box
    if (raw.length > 1 && index === 0) {
      const pasted = raw.slice(0, DIGIT_COUNT).split('');
      const filled = [...pasted, ...Array(DIGIT_COUNT - pasted.length).fill('')];
      this.digits.set(filled);
      this.updateVerificationCode();
      this.setInputValues(filled);
      this.focusDigit(Math.min(pasted.length, DIGIT_COUNT - 1));
      if (pasted.length === DIGIT_COUNT) {
        this.triggerVerify();
      }
      return;
    }

    const char = raw.slice(-1);
    this.updateDigit(index, char);
    if (char && index < DIGIT_COUNT - 1) {
      this.focusDigit(index + 1);
    }
    if (index === DIGIT_COUNT - 1 && char && this.isCodeComplete()) {
      this.triggerVerify();
    }
  }

  onBackspace(index: number, event: Event): void {
    event.preventDefault();
    if (this.digits()[index]) {
      this.updateDigit(index, '');
    } else if (index > 0) {
      this.updateDigit(index - 1, '');
      this.focusDigit(index - 1);
    }
    this.verifyError.set(null);
  }

  verifyAccount(): void {
    this.triggerVerify();
  }

  sendResend(): void {
    if (this.resendCountdown() > 0 || this.isResending()) {
      return;
    }

    const email = this.pendingEmail();
    if (!email) {
      this.notificationService.showError('No email address found. Please register again.');
      return;
    }

    this.isResending.set(true);

    this.http
      .post(`${environment.apiUrl}/auth/resend-verification`, { email })
      .pipe(finalize(() => this.isResending.set(false)))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Verification code resent.');
          this.startResendCountdown();
        },
        error: () => {
          this.notificationService.showError('Failed to resend code. Please try again.');
        },
      });
  }

  private triggerVerify(): void {
    if (this.isVerifying() || this.isVerified()) {
      return;
    }

    const code = this.verificationCode();
    const email = this.pendingEmail();

    if (!this.isCodeComplete()) {
      this.verifyError.set('Please enter all 6 digits.');
      return;
    }

    if (!email) {
      this.verifyError.set('Session expired. Please register again.');
      return;
    }

    this.isVerifying.set(true);
    this.verifyError.set(null);

    this.http
      .post(`${environment.apiUrl}/auth/verify-email`, { email, token: code })
      .pipe(finalize(() => this.isVerifying.set(false)))
      .subscribe({
        next: () => {
          this.isVerified.set(true);
          sessionStorage.removeItem('ironlogic.pending.email');
          this.notificationService.showSuccess('Email verified!');
          this.redirectTimer = setTimeout(() => {
            void this.router.navigateByUrl('/athlete/dashboard');
          }, REDIRECT_DELAY_MS);
        },
        error: () => {
          this.verifyError.set('Invalid or expired code. Please try again.');
          this.digits.set(Array(DIGIT_COUNT).fill(''));
          this.updateVerificationCode();
          this.setInputValues(Array(DIGIT_COUNT).fill(''));
          this.focusDigit(0);
        },
      });
  }

  private updateDigit(index: number, value: string): void {
    this.digits.update((prev) => {
      const next = [...prev];
      next[index] = value;
      return next;
    });
    this.updateVerificationCode();
    const el = this.digitInputs?.get(index)?.nativeElement;
    if (el) {
      el.value = value;
    }
  }

  private updateVerificationCode(): void {
    this.verificationCode.set(this.digits().join(''));
  }

  private setInputValues(values: string[]): void {
    this.digitInputs?.forEach((ref, i) => {
      ref.nativeElement.value = values[i] ?? '';
    });
  }

  private focusDigit(index: number): void {
    setTimeout(() => {
      this.digitInputs?.get(index)?.nativeElement.focus();
    }, 0);
  }

  private startResendCountdown(): void {
    if (this.resendTimer !== null) {
      clearInterval(this.resendTimer);
      this.resendTimer = null;
    }

    this.resendCountdown.set(RESEND_COUNTDOWN_SEC);
    this.resendTimer = setInterval(() => {
      const next = this.resendCountdown() - 1;
      if (next <= 0) {
        this.resendCountdown.set(0);
        if (this.resendTimer !== null) {
          clearInterval(this.resendTimer);
          this.resendTimer = null;
        }
      } else {
        this.resendCountdown.set(next);
      }
    }, 1000);
  }
}
