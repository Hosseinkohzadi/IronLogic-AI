import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  PLATFORM_ID,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { DashboardFilterService } from '@core/services/dashboard-filter.service';

interface SvgCountryPath {
  id: string;
  d: string;
}

interface PulseMarker {
  id: string;
  x: string;
  y: string;
}

@Component({
  selector: 'app-world-map',
  imports: [CommonModule],
  templateUrl: './world-map.component.html',
  styleUrl: './world-map.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorldMapComponent {
  private readonly http = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly dashboardFilterService = inject(DashboardFilterService);

  readonly selectedCountry = this.dashboardFilterService.selectedCountry;
  readonly compactMode = input(false);

  private readonly densityByCountry: Record<string, number> = {
    Canada: 50,
    Iran: 120,
    USA: 30,
    Germany: 15,
  };

  readonly countries = signal<SvgCountryPath[]>([]);
  readonly viewBox = signal('0 0 2000 857');
  readonly hoveredCountry = signal<string | null>(null);
  readonly tooltipPosition = signal({ x: 0, y: 0 });

  readonly legendBins = [
    { label: '<10', color: '#e0e7ff' },
    { label: '10-50', color: '#818cf8' },
    { label: '50-100', color: '#4f46e5' },
    { label: '>100', color: '#4338ca' },
  ] as const;

  readonly pulseMarkers: PulseMarker[] = [
    { id: 'Iran', x: '69.8%', y: '39.2%' },
    { id: 'Canada', x: '32.5%', y: '16.8%' },
    { id: 'USA', x: '27.5%', y: '27.8%' },
  ];

  readonly activeCountriesCount = computed(
    () => this.countries().filter((country) => this.getCountryUsers(country.id) > 0).length,
  );

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadWorldSvg();
    }
  }

  getCountryUsers(countryId: string): number {
    return this.densityByCountry[countryId] ?? 0;
  }

  getCountryColor(countryId: string): string {
    const users = this.getCountryUsers(countryId);
    if (users > 100) return '#4338ca';
    if (users > 50) return '#4f46e5';
    if (users >= 10) return '#818cf8';
    if (users > 0) return '#e0e7ff';
    return '#f1f5f9';
  }

  onCountryClick(countryId: string): void {
    this.dashboardFilterService.setFilter(countryId);
  }

  onCountryHover(event: MouseEvent, countryId: string): void {
    if (this.compactMode()) {
      return;
    }

    const target = event.currentTarget as SVGPathElement;
    const svg = target.ownerSVGElement;
    if (!svg) return;

    const rect = svg.getBoundingClientRect();
    const x = Math.max(90, Math.min(rect.width - 90, event.clientX - rect.left));
    const y = Math.max(30, event.clientY - rect.top);
    this.tooltipPosition.set({ x, y });
    this.hoveredCountry.set(countryId);
  }

  clearHover(): void {
    this.hoveredCountry.set(null);
  }

  private loadWorldSvg(): void {
    this.http.get('assets/maps/world.svg', { responseType: 'text' }).subscribe((svgText) => {
      const parser = new DOMParser();
      const doc = parser.parseFromString(svgText, 'image/svg+xml');
      const svgEl = doc.querySelector('svg');
      const viewBox =
        svgEl?.getAttribute('viewBox') ?? svgEl?.getAttribute('viewbox') ?? '0 0 2000 857';
      this.viewBox.set(viewBox);

      const parsedPaths = Array.from(doc.querySelectorAll('path'))
        .map((path, index) => {
          const d = path.getAttribute('d');
          if (!d) return null;

          const rawId =
            path.getAttribute('id') ??
            path.getAttribute('name') ??
            path.getAttribute('class')?.split(' ')[0] ??
            `country-${index}`;

          const id = this.normalizeCountryId(rawId);
          return { id, d } as SvgCountryPath;
        })
        .filter((item): item is SvgCountryPath => item !== null);

      this.countries.set(parsedPaths);
    });
  }

  private normalizeCountryId(rawId: string): string {
    const map: Record<string, string> = {
      IR: 'Iran',
      CA: 'Canada',
      US: 'USA',
      USA: 'USA',
      DE: 'Germany',
      Germany: 'Germany',
      Iran: 'Iran',
      Canada: 'Canada',
      'United States': 'USA',
      'United States of America': 'USA',
    };

    return map[rawId] ?? rawId;
  }
}
