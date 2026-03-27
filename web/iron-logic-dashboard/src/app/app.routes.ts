import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent, pathMatch: 'full' },
  // Future routes for other features can be added here.
];
