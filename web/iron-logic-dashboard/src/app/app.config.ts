import { ApplicationConfig, importProvidersFrom, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

// ایمپورت آیکون‌ها با نام‌های استاندارد کتابخانه
import {
  LayoutDashboard,
  Users,
  Calendar,
  Dumbbell,
  Activity,
  Weight,
  Layers,
  Wrench,
  Pencil,
  Trash2, // اصلاح شده از Trash-2
  ShieldCheck,
  Settings,
  Gauge,
  ArrowUpRight,
  ChevronLeft,
  Search,
  LucideAngularModule
} from 'lucide-angular';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(),
    importProvidersFrom(
      LucideAngularModule.pick({
        LayoutDashboard,
        Users,
        Calendar,
        Dumbbell,
        Activity,
        Weight,
        Layers,
        Wrench,
        Pencil,
        Trash2, // نام صحیح کلاس آیکون
        ShieldCheck,
        Settings,
        Gauge,
        ArrowUpRight,
        ChevronLeft,
        Search
      })
    )
  ]
};
