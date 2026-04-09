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
  Lock,
  Mail,
  PanelLeft,
  Pencil,
  RefreshCw,
  Search,
  Settings,
  Star,
  ShieldCheck,
  ShieldOff,
  Trash2,
  TrendingUp,
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
        Lock,
        Mail,
        PanelLeft,
        RefreshCw,
        Star,
        Zap,
        AlertTriangle,
        Eye,
        Edit3,
        ChevronRight,
        X,
        Filter,
        TrendingUp,
        ChevronDown,
        Check,
        MoreHorizontal,
        FileSpreadsheet,
        FileText
      })
    )
  ]
};
