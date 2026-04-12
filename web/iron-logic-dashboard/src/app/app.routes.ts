import { Routes } from '@angular/router';
import { LandingComponent } from '@features/landing/landing';
import { LoginComponent } from '@features/login/login';
import { RegisterComponent } from '@features/register/register';
import { ForgotComponent } from '@features/forgot/forgot';
import { DashboardComponent } from '@features/dashboard/dashboard.component';
import { ImportWorkoutComponent } from '@features/import-workout/import-workout';
import { FaqComponent } from './pages/faq/faq';
import { ContactComponent } from './pages/contact/contact';
import { AdminComponent } from '@features/admin/MainAdminPage/admin.component';
import {
  adminChildGuard,
  adminGuard,
  authGuard,
  athleteGuard,
  RoleGuard,
  publicOnlyGuard,
} from '@core/guards/auth-role.guards';
import { athleteRoutes } from '@features/athlete/athlete.routes';

export const routes: Routes = [
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [authGuard, adminGuard],
    canActivateChild: [adminChildGuard],
    data: { hideSidebar: false },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('@features/admin/MainAdminPage/admin-dashboard-home.component').then(
            (m) => m.AdminDashboardHomeComponent,
          ),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('@features/admin/components/user-management/user-management').then(
            (m) => m.UserManagementComponent,
          ),
      },
      {
        path: 'exercises',
        loadComponent: () =>
          import('@features/admin/components/exercise-management/exercise-management').then(
            (m) => m.ExerciseManagementComponent,
          ),
      },
      {
        path: 'sessions',
        loadComponent: () =>
          import('@features/admin/components/workout-logs/workout-logs').then(
            (m) => m.WorkoutLogsComponent,
          ),
      },
      {
        path: 'financial',
        loadComponent: () =>
          import('@features/admin/components/financial-dashboard/financial-dashboard').then(
            (m) => m.FinancialDashboardComponent,
          ),
      },
      {
        path: 'equipment',
        loadComponent: () =>
          import('@features/admin/components/equipment-management/equipment-management.component').then(
            (m) => m.EquipmentManagementComponent,
          ),
      },
      {
        path: 'muscles',
        loadComponent: () =>
          import('@features/admin/components/muscle-management/muscle-management.component').then(
            (m) => m.MuscleManagementComponent,
          ),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('@features/admin/components/settings-page/settings-page').then(
            (m) => m.SettingsPageComponent,
          ),
      },
      {
        path: 'billing',
        loadComponent: () =>
          import('./features/admin/components/billing-stats/billing-stats').then(
            (m) => m.BillingStatsComponent,
          ),
      },
      {
        path: 'subscription',
        loadComponent: () =>
          import('./features/admin/components/subscription-management/subscription-management').then(
            (m) => m.SubscriptionManagementComponent,
          ),
      },
    ],
  },
  {
    path: 'athlete/subscription',
    canActivate: [authGuard, athleteGuard],
    data: { hideSidebar: false },
    loadComponent: () =>
      import('./features/subscription/plan-selection').then((m) => m.PlanSelectionComponent),
  },
  {
    path: 'athlete/subscription/checkout',
    canActivate: [authGuard, RoleGuard],
    data: { hideSidebar: false, requiredRole: 'ATHLETE' },
    loadComponent: () =>
      import('@features/subscription/subscription').then((m) => m.SubscriptionComponent),
  },
  {
    path: 'subscription',
    pathMatch: 'full',
    redirectTo: 'athlete/subscription',
  },
  {
    path: 'subscription/checkout',
    pathMatch: 'full',
    redirectTo: 'athlete/subscription/checkout',
  },
  {
    path: 'athlete/dashboard',
    pathMatch: 'full',
    redirectTo: 'athlete/portal',
  },
  ...athleteRoutes,
  { path: '', component: LandingComponent, pathMatch: 'full', data: { hideSidebar: true } },
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [publicOnlyGuard],
    data: { hideSidebar: true },
  },
  {
    path: 'auth/login',
    component: LoginComponent,
    canActivate: [publicOnlyGuard],
    data: { hideSidebar: true },
  },
  { path: 'register', component: RegisterComponent, data: { hideSidebar: true } },
  { path: 'forgot', component: ForgotComponent, data: { hideSidebar: true } },
  { path: 'dashboard', component: DashboardComponent, data: { hideSidebar: false } },
  { path: 'import', component: ImportWorkoutComponent, data: { hideSidebar: false } },
  { path: 'faq', component: FaqComponent, data: { hideSidebar: true } },
  { path: 'contact', component: ContactComponent, data: { hideSidebar: true } },
  { path: '**', redirectTo: '' },
];
