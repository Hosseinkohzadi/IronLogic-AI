import { Routes } from '@angular/router';
import { authGuard, athleteGuard } from '@core/guards/auth-role.guards';

export const athleteRoutes: Routes = [
  {
    path: 'athlete/portal',
    canActivate: [authGuard, athleteGuard],
    data: { hideSidebar: false },
    loadComponent: () =>
      import('@features/athlete/athlete-portal').then((m) => m.AthletePortalComponent),
  },
  {
    path: 'athlete/profile',
    canActivate: [authGuard, athleteGuard],
    data: { hideSidebar: false },
    loadComponent: () =>
      import('@features/athlete/profile/profile.component').then((m) => m.ProfileComponent),
  },
];
