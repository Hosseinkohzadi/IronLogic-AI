import {Routes} from '@angular/router';
import {LandingComponent} from '@features/landing/landing';
import {LoginComponent} from '@features/login/login';
import {RegisterComponent} from '@features/register/register';
import {ForgotComponent} from '@features/forgot/forgot';
import {DashboardComponent} from '@features/dashboard/dashboard.component';
import {ImportWorkoutComponent} from '@features/import-workout/import-workout';
import {FaqComponent} from './pages/faq/faq';
import {ContactComponent} from './pages/contact/contact';
import {AdminComponent} from '@features/admin/admin.component';

export const routes: Routes = [
  { path: 'admin', component: AdminComponent },
  {path: '', component: LandingComponent, pathMatch: 'full', data: {hideSidebar: true}},
  {path: 'login', component: LoginComponent, data: {hideSidebar: true}},
  {path: 'register', component: RegisterComponent, data: {hideSidebar: true}},
  {path: 'forgot', component: ForgotComponent, data: {hideSidebar: true}},
  {path: 'dashboard', component: DashboardComponent, data: {hideSidebar: false}},
  {path: 'import', component: ImportWorkoutComponent, data: {hideSidebar: false}},
  {path: 'faq', component: FaqComponent, data: {hideSidebar: true}},
  { path: 'contact', component: ContactComponent, data: { hideSidebar: true } },
  {path: '**', redirectTo: ''}
];
