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

export const routes: Routes = [
  {
    path: 'admin',
    component: AdminComponent, // این کامپوننت نقش Shell را دارد
    data: { hideSidebar: false },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        // نکته مهم: اینجا نباید دوباره AdminComponent لود شود.
        // یا یک کامپوننت داشبورد جدید بسازید یا فعلاً این مسیر را خالی بگذارید.
        loadComponent: () => import('@features/admin/MainAdminPage/admin.component').then(m => m.AdminComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('@features/admin/components/user-management/user-management').then(m => m.UserManagementComponent)
      },
      {
        path: 'exercises',
        loadComponent: () => import('@features/admin/components/exercise-management/exercise-management').then(m => m.ExerciseManagementComponent)
      },
      {
        path: 'sessions',
        loadComponent: () => import('@features/admin/components/workout-logs/workout-logs').then(m => m.WorkoutLogsComponent)
      },
      {
        path: 'equipment',
        loadComponent: () => import('@features/admin/components/equipment-management/equipment-management.component').then(m => m.EquipmentManagementComponent)
      },
      {
        path: 'muscles',
        loadComponent: () => import('@features/admin/components/muscle-management/muscle-management.component').then(m => m.MuscleManagementComponent)
      }
    ]
  },
  { path: '', component: LandingComponent, pathMatch: 'full', data: { hideSidebar: true } },
  { path: 'login', component: LoginComponent, data: { hideSidebar: true } },
  { path: 'register', component: RegisterComponent, data: { hideSidebar: true } },
  { path: 'forgot', component: ForgotComponent, data: { hideSidebar: true } },
  { path: 'dashboard', component: DashboardComponent, data: { hideSidebar: false } },
  { path: 'import', component: ImportWorkoutComponent, data: { hideSidebar: false } },
  { path: 'faq', component: FaqComponent, data: { hideSidebar: true } },
  { path: 'contact', component: ContactComponent, data: { hideSidebar: true } },
  { path: '**', redirectTo: '' }
];
