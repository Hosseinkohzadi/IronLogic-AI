import { Routes } from '@angular/router';
import { LandingComponent } from './features/landing/landing/landing';
import { LoginComponent } from './features/login/login/login';
import { RegisterComponent } from './features/register/register/register';
import { ForgotComponent } from './features/forgot/forgot/forgot';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ImportWorkoutComponent } from './features/import-workout/import-workout/import-workout';

export const routes: Routes = [
  { path: '', component: LandingComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot', component: ForgotComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'import', component: ImportWorkoutComponent },
  { path: '**', redirectTo: '' }
];
