import {ApplicationConfig, importProvidersFrom, provideZonelessChangeDetection} from '@angular/core';
import {provideRouter} from '@angular/router';
import {provideHttpClient} from '@angular/common/http';
import {routes} from './app.routes';

import {
  Activity,
  ArrowUpRight,
  Calendar,
  Check,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  Dumbbell,
  Edit3,
  Eye,
  Filter,
  Gauge,
  Info,
  Layers,
  LayoutDashboard,
  LucideAngularModule,
  Mail,
  Pencil,
  RefreshCw,
  Search,
  Settings,
  Star,
  ShieldCheck,
  ShieldOff,
  Trash2,
  Users,
  Zap,
  Weight,
  Wrench,
  X,
  AlertTriangle,
  MoreHorizontal,
   FileSpreadsheet,
        FileText
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
        Trash2, 
        ShieldCheck,
        Settings,
        Gauge,
        Info,
        ArrowUpRight,
        ChevronLeft,
        Search,
        ShieldOff,
        Mail,
        RefreshCw,
        Star,
        Zap,
        AlertTriangle,
        Eye,
        Edit3,
        ChevronRight,
        X,
        Filter,
        ChevronDown,
        Check,
        MoreHorizontal,
        FileSpreadsheet,
        FileText
      })
    )
  ]
};
