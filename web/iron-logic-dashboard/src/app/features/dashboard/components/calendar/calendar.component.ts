import { Component, AfterViewInit, Input, ElementRef, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
// @ts-expect-error - flowbite-datepicker currently has no TypeScript type definitions
import Datepicker from 'flowbite-datepicker/Datepicker';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.css'
})
export class CalendarComponent implements AfterViewInit, OnChanges {
  @ViewChild('datepicker') datepickerElement!: ElementRef;
  @Input() workoutDates: string[] = [];
  datepicker: any;

  constructor(private datePipe: DatePipe) {}

  ngAfterViewInit() {
    this.initDatepicker();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['workoutDates'] && this.datepicker) {
      // Use setTimeout to ensure the Angular render cycle is complete
      setTimeout(() => this.datepicker.refresh(), 100);
    }
  }

  initDatepicker() {
    if (!this.datepickerElement) return;

    this.datepicker = new Datepicker(this.datepickerElement.nativeElement, {
      autohide: false,
      todayBtn: true,
      todayBtnMode: 1,
      beforeShowDay: (date: Date) => {
        const formattedDate = this.datePipe.transform(date, 'yyyy-MM-dd');
        // The class name must exactly match your CSS file (trained-day-highlight)
        const isTrained = this.workoutDates.includes(formattedDate || '');
        return isTrained ? { classes: 'trained-day-highlight' } : {};
      }
    });
  }
}
