import { inject } from '@angular/core';
import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';

function getDefaultRouteForRole(role: ReturnType<AuthService['role']>): string {
  if (role === 'SUPER_ADMIN') {
    return '/admin/dashboard';
  }

  if (role === 'ATHLETE') {
    return '/athlete/portal';
  }

  return '/login';
}

export const superAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.hasRole('SUPER_ADMIN')) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

export const superAdminChildGuard: CanActivateChildFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.hasRole('SUPER_ADMIN')) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

export const athleteGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.hasRole('ATHLETE')) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

export const publicOnlyGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree([getDefaultRouteForRole(auth.role())]);
};
