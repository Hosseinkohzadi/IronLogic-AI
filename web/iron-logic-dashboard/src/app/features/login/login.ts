import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './login.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  loginWithForm(): void {
    // Mock behavior for the default login form during development.
    this.authService.loginAsSuperAdmin();
    this.router.navigate(['/admin/dashboard']);
  }

  loginAsSuperAdmin(): void {
    this.authService.loginAsSuperAdmin();
    this.router.navigate(['/admin/dashboard']);
  }

  loginAsAthleteHossein(): void {
    this.authService.loginAsAthlete('athlete-hossein-001');
    this.router.navigate(['/athlete/portal']);
  }
}
