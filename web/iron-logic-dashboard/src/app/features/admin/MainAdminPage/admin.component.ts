import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { interval, of } from 'rxjs';
import { catchError, startWith, switchMap } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {
  public api = inject(IronLogicApiService);
  private destroyRef = inject(DestroyRef);

  serverStatus = signal<'OPERATIONAL' | 'DOWN' | 'CHECKING'>('CHECKING');
  totalUsers = signal<number>(0);
  totalWorkouts = signal<number>(0);

  ngOnInit() {
    this.startHealthCheck();
    this.fetchGlobalStats();
  }

private fetchGlobalStats() {
    this.api.getWorkoutStatsWithAdvice().subscribe(stats => {
      if (stats?.dailyWorkouts) {
        const count = stats.dailyWorkouts.reduce((acc: any, curr: any) => acc + curr.workoutSessionDtos.length, 0);
        this.totalWorkouts.set(count);
      }
    });

    // تعیین نوع any[] برای برطرف شدن خطای TS7006
    this.api.getUsers().subscribe((users: any[]) => {
      if (users) this.totalUsers.set(users.length);
    });
  }

  startHealthCheck() {
    interval(15000).pipe(
      startWith(0),
      switchMap(() => this.api.pingServer().pipe(catchError(() => of(false)))),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(isUp => {
      this.serverStatus.set(isUp ? 'OPERATIONAL' : 'DOWN');
    });
  }
}
