import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { AuthService, UserRole } from '@core/services/auth.service';

function getDefaultRouteForRole(role: ReturnType<AuthService['role']>): string {
  if (role === 'SUPER_ADMIN') {
    return '/admin/dashboard';
  }

  if (role === 'ATHLETE') {
    return '/athlete/dashboard';
  }

  return '/auth/login';
}

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login']);
};

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.hasRole('SUPER_ADMIN')) {
    return true;
  }

  if (auth.isAuthenticated()) {
    return router.createUrlTree([getDefaultRouteForRole(auth.role())]);
  }

  return router.createUrlTree(['/auth/login']);
};

export const adminChildGuard: CanActivateChildFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.hasRole('SUPER_ADMIN')) {
    return true;
  }

  if (auth.isAuthenticated()) {
    return router.createUrlTree([getDefaultRouteForRole(auth.role())]);
  }

  return router.createUrlTree(['/auth/login']);
};

export const athleteGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.hasRole('ATHLETE')) {
    return true;
  }

  if (auth.isAuthenticated()) {
    return router.createUrlTree([getDefaultRouteForRole(auth.role())]);
  }

  return router.createUrlTree(['/auth/login']);
};

export const publicOnlyGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree([getDefaultRouteForRole(auth.role())]);
};

export const RoleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/auth/login']);
  }

  const requiredRole = route.data['requiredRole'] as UserRole | undefined;
  if (!requiredRole) {
    return true;
  }

  if (auth.hasRole(requiredRole)) {
    return true;
  }

  if (requiredRole === 'ATHLETE' && auth.hasRole('SUPER_ADMIN')) {
    return router.createUrlTree(['/admin/dashboard']);
  }

  return router.createUrlTree([getDefaultRouteForRole(auth.role())]);
};

export const superAdminGuard = adminGuard;
export const superAdminChildGuard = adminChildGuard;
